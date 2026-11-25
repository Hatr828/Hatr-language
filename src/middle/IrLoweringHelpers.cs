using Best_language.src.back.Instructions;
using Best_language.src.front;

namespace Best_language.src.middle
{
    public partial class IrLowering
    {
        private IrValue EmitConstant(ulong value, TypeSymbol type)
        {
            var result = NewIrValue(type);
            Emit(new IrConstant(result, value, type));
            return result;
        }
        private IrValue EmitConstant(double value, TypeSymbol type)
        {
            var result = NewIrValue(type);
            Emit(new IrFloatConstant(result, value, type));
            return result;
        }
        private IrValue EmitConstant(List<IrValue> right, TypeSymbol type)
        {
            var result = NewIrValue(type);
            Emit(new IrArrayConstant(result, right, type));
            return result;
        }

        private IrValue EmitCopy(IrValue source)
        {
            var result = NewIrValue(source.Type);
            Emit(new IrVariable(result, source));
            return result;
        }

        private void Emit(IrInstruction instruction)
        {
            _currentProgram.Instructions.Add(instruction);
        }
        private IrValue ResolveVariable(VariableSymbol variable)
        {
            if (_variables.TryGetValue(variable, out var value))
                return value;

            throw new InvalidOperationException($"Unknown variable: '{variable.Name}'.");
        }

        private int GetTypeSize(TypeSymbol type) => type switch
        {
            BoolTypeSymbol => 1,
            IntegerTypeSymbol tmp => (int)tmp.Size,
            FloatTypeSymbol tmp => (int)tmp.Size,
            PointerTypeSymbol => 8,
            _ => throw new ArgumentException()
        };

        private (IrValue address, int offset) LowerAddress(BoundExpression expression)
        {
            switch (expression)
            {
                case BoundVariableExpression variable:
                    {
                        var storage = ResolveVariable(variable.Variable);
                        var address = NewIrValue(_typeTable.GetPointer(variable.Type));

                        Emit(new IrUnary(Operation.ReferenceOf, storage, address));
                        return (address, 0);
                    }
                case BoundUnaryExpression { Op: Operation.Dereference } unary:
                    {
                        return (LowerExpression(unary.Right), 0);
                    }

                case BoundMemberAccessExpression member:
                    {
                        var (baseAddress, offset) = LowerAddress(member.Target);
                        return (baseAddress, offset + member.Field.Offset); 
                    }
                case BoundArrayAccessExpression arrayAccess:
                    {
                        var variable = ResolveVariable(arrayAccess.Variable);
                        IrValue baseAddress = variable;

                        if (arrayAccess.Variable.Type is ArrayTypeSymbol)
                        {
                            baseAddress = NewIrValue(_typeTable.GetPointer(arrayAccess.Variable.Type));
                            Emit(new IrUnary(Operation.ReferenceOf, variable, baseAddress));
                        }

                        var idx = LowerExpression(arrayAccess.Idx);

                        if (GetTypeSize(idx.Type) < 8)
                        {
                            var idx64 = NewIrValue(_typeTable.Int64);
                            Emit(new IrConversion(idx64, idx));
                            idx = idx64;
                        }

                        var elAddress = NewIrValue(_typeTable.GetPointer(arrayAccess.Variable.Type));
                        int sizeOf = GetTypeSize(arrayAccess.Type);

                        Emit(new IrIndexedAddress(elAddress, baseAddress, idx, sizeOf));
                        return (elAddress, 0);
                    }

                default: throw new InvalidOperationException($"Cannot take address of {expression}");
            }
        }
    }
}
