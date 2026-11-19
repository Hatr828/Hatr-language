using Best_language.src.front;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
