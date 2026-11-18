using Best_language.src.front;
using System.Numerics;

namespace Best_language.src.middle
{
    public class IrLowering
    {
        private int _nextId;
        private int _nextLebelId;
        private IrProgram _currentProgram = new();

        private Dictionary<string, IrValue> _variables = new();

        private IrValue NewIrValue(TypeSymbol type) => new(_nextId++, type);
        private string NewLabelName(string name) => $"{name}_{_nextLebelId++}";

        public IrProgram Lower(BoundProgram mainNode)
        {
            foreach (BoundStatement st in mainNode.Statements)
            {
                LowerNode(st);
            }

            return _currentProgram;
        }

        public void LowerNode(BoundStatement node)
        {
            switch (node)
            {
                case BoundVariableDeclaration variable:
                    {
                        if (variable.Initializer is null) return;

                        var value = LowerExpression(variable.Initializer);
                        var res = EmitCopy(value);
                        _variables.Add(variable.Name, res);
                        return;
                    }
                case BoundWhileDeclaration @while:
                    {
                        string whileLabel = NewLabelName("while");
                        string endLabel = NewLabelName("end");

                        Emit(new IrLabel(whileLabel));

                        IrValue condition = LowerExpression(@while.Expression); ;

                        Emit(new IrJumpIfNotTrue(condition, endLabel));

                        foreach (var statement in @while.Statements)
                            LowerNode(statement);

                        Emit(new IrJump(whileLabel));
                        Emit(new IrLabel(endLabel));
                        return;
                    }
                case BoundForDeclaration @for:
                    {
                        if (@for.Initializer is not null) LowerNode(@for.Initializer);

                        string forLabel = NewLabelName("for");
                        string endLabel = NewLabelName("end");

                        Emit(new IrLabel(forLabel));
                        IrValue condition;

                        if (@for.Expression is not null) condition = LowerExpression(@for.Expression);
                        else condition = LowerExpression(new BoundBoolExpression(true, new BoolTypeSymbol()));

                        Emit(new IrJumpIfNotTrue(condition, endLabel));

                        foreach (var statement in @for.Statements)
                            LowerNode(statement);

                        if (@for.Step is not null) LowerExpression(@for.Step);

                        Emit(new IrJump(forLabel));
                        Emit(new IrLabel(endLabel));
                        return;
                    }
                case BoundIfDeclaration @if:
                    {
                        IrValue condition = LowerExpression(@if.Expression);

                        List<string> labels = new();
                        for (int i = 0; i < @if.ElseIfs?.Count; i++) labels.Add(NewLabelName("elseIf"));
                        string ifLabel = NewLabelName("if");
                        string endLabel = NewLabelName("end");
                        string elseLabel = NewLabelName("else");

                        Emit(new IrJumpIfTrue(condition, ifLabel));

                        for (int i = 0; i < @if.ElseIfs?.Count; i++) Emit(new IrJumpIfTrue(LowerExpression(@if.ElseIfs[i].Expression), labels[i]));
                        if (@if.Else is not null) Emit(new IrJump(elseLabel));
                        Emit(new IrJump(endLabel)); //end label if there is no else and everything before is false. Redundant if there is else, possible size reduction for asm and therefore for .exe.

                        //if part
                        Emit(new IrLabel(ifLabel));
                        foreach (var statement in @if.Statements)
                            LowerNode(statement);
                        Emit(new IrJump(endLabel));

                        // elseIfs parts
                        for (int i = 0; i < @if.ElseIfs?.Count; i++)
                        {
                            Emit(new IrLabel(labels[i]));
                            foreach (var statement in @if.ElseIfs[i].Statements)
                                LowerNode(statement);
                            Emit(new IrJump(endLabel));
                        }
                        // else part
                        if (@if.Else is not null)
                        {
                            Emit(new IrLabel(elseLabel));
                            foreach (var statement in @if.Else.Statements)
                                LowerNode(statement);
                        }
                        // end label
                        Emit(new IrLabel(endLabel));
                        return;
                    }
                case BoundGotoDeclaration @goto:
                    Emit(new IrLabel("user_" + @goto.Id)); return;
                case BoundGotoStatement @goto:
                    Emit(new IrJump("user_" + @goto.ToId)); return;
                case BoundAssignmentStatement assignment:
                    {
                        var res = ResolveVariable(assignment.Name);
                        var source = LowerExpression(assignment.Right);

                        Emit(new IrVariable(res, source));
                        return;
                    }
                case BoundExpressionStatement ex:
                    {
                        LowerExpression(ex.Expression);
                        return;
                    }
            }
            throw new Exception("Cannot lower: " + node.GetType());
        }

        public IrValue LowerExpression(BoundExpression expression)
        {
            switch (expression)
            {
                case BoundNumberExpression num:
                    return EmitConstant(num.Value, num.Type);
                case BoundDoubleExpression num:
                    return EmitConstant(num.Value, num.Type);
                case BoundBoolExpression ex:
                    return EmitConstant((ulong)(ex.Value ? 1 : 0), ex.Type);
                case BoundVariableExpression variable:
                    return ResolveVariable(variable.Name);
                case BoundBinaryExpression binary:
                    {
                        var left = LowerExpression(binary.Left);    
                        var right = LowerExpression(binary.Right);
                        var result = NewIrValue(binary.Type);

                        Emit(new IrBinary(binary.Op, left, right, result));

                        return result;
                    }
                case BoundUnaryExpression unary:
                    {
                        bool increment = unary.Op is Operations.PlusPlusLeft or Operations.PlusPlusRight or Operations.MinusMinusLeft or Operations.MinusMinusRight;
                        
                        if (increment)
                        {
                            if (unary.Right is not BoundVariableExpression variable) throw new ArgumentException("Expected variable");

                            var target = ResolveVariable(variable.Name);
                            var right = EmitCopy(target);
                            var result = NewIrValue(unary.Type);

                            Emit(new IrUnary(unary.Op, right, result));
                            Emit(new IrVariable(target, result));

                            return unary.Op is Operations.PlusPlusRight or Operations.MinusMinusRight ? right : result;
                        }

                        var right_ = LowerExpression(unary.Right);
                        var result_ = NewIrValue(unary.Type);

                        Emit(new IrUnary(unary.Op, right_, result_));

                        return result_;
                    }
                case BoundConversionExpression conversion:
                    {
                        var right = LowerExpression(conversion.Expression);

                        if (right.Type == conversion.Type) return right;

                        IrValue left = NewIrValue(conversion.Type);

                        Emit(new IrConversion(left, right));

                        return left;
                    }
            }
            throw new Exception("Cannot lower: " + expression.GetType());
        }
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
        private IrValue ResolveVariable(string name)
        {
            if (_variables.TryGetValue(name, out var value))
                return value;

            throw new InvalidOperationException($"Unknown variable: '{name}'.");
        }
    }
}
