using Best_language.src.front;
using Microsoft.Extensions.FileProviders;
using System.Numerics;

namespace Best_language.src.middle
{
    public partial class IrLowering
    {
        private int _nextId;
        private int _nextLebelId;
        private IrProgram _currentProgram = new();

        private Dictionary<VariableSymbol, IrValue> _variables = new();
        private List<string> _breaks = new();
        private List<string> _continues = new();
        private List<string> _defaults = new();

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
                        if (variable.Initializer is null)
                        {
                            _variables.Add(variable.Variable, NewIrValue(variable.Variable.Type));
                            return;
                        }

                        var value = LowerExpression(variable.Initializer!);
                        var res = EmitCopy(value);
                        _variables.Add(variable.Variable, res);
                        return;
                    }
                case BoundWhileDeclaration @while:
                        LowerWhile(@while);
                        return;

                case BoundForDeclaration @for:
                        LowerFor(@for);
                        return;

                case BoundIfDeclaration @if:
                        LowerIf(@if);
                        return;

                case BoundSwitchDeclaration @switch:
                    string endLabel = NewLabelName("switch_end");
                    List<string> labels = new();
                    string? defaultStartLabel = null;

                    _breaks.Add(endLabel);

                    foreach (var @case in @switch.Cases)
                    {
                        if (@case.Expression is not null)
                        {
                            string caseEndLabel = NewLabelName("case_end");
                            IrValue condition = LowerExpression(@case.Expression);

                            Emit(new IrJumpIfTrue(condition, caseEndLabel));

                            labels.Add(caseEndLabel);
                        }
                        else defaultStartLabel = NewLabelName("default_start");
                    }

                    if (defaultStartLabel is not null) Emit(new IrJump(defaultStartLabel));

                    int idx = 0;
                    foreach (var @case in @switch.Cases)
                    {
                        if (@case.IsDefault)
                        {
                            if (defaultStartLabel is null) continue;
                            Emit(new IrLabel(defaultStartLabel));
                            foreach (var statement in @case.Statements) LowerNode(statement);
                        }
                        else
                        {
                            Emit(new IrLabel(labels[idx++]));
                            foreach (var statement in @case.Statements) LowerNode(statement);
                        }
                    }


                    _breaks.RemoveAt(_breaks.Count - 1);

                    Emit(new IrLabel(endLabel));
                    return;

                case BoundCaseDeclaration @case:
                    if (@case.Expression is not null)
                    {
                        string caseEndLabel = NewLabelName("case_end");

                        IrValue condition = LowerExpression(@case.Expression);

                        Emit(new IrJumpIfNotTrue(condition, caseEndLabel));

                        foreach (var statement in @case.Statements)
                            LowerNode(statement);

                        Emit(new IrLabel(caseEndLabel));
                    }
                    else
                    {
                        defaultStartLabel = NewLabelName("default_start");

                        Emit(new IrLabel(defaultStartLabel));

                        foreach (var statement in @case.Statements)
                            LowerNode(statement);
                    }
                    return;

                case BoundBreakStatement:
                    Emit(new IrJump(_breaks[^1]));
                        return;
                case BoundContinueStatement:
                    Emit(new IrJump(_continues[^1]));
                    return;
                case BoundGotoDeclaration @goto:
                    Emit(new IrLabel("user_" + @goto.Id)); return;
                case BoundGotoStatement @goto:
                    Emit(new IrJump("user_" + @goto.ToId)); return;
                case BoundAssignmentStatement assignment:
                    {
                        var res = ResolveVariable(assignment.Variable);
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

        private IrValue LowerExpression(BoundExpression expression)
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
                    return ResolveVariable(variable.Variable);
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
                        bool increment = unary.Op is Operation.PlusPlusLeft or Operation.PlusPlusRight or Operation.MinusMinusLeft or Operation.MinusMinusRight;
                        
                        if (increment)
                        {
                            if (unary.Right is not BoundVariableExpression variable) throw new ArgumentException("Expected variable");

                            var target = ResolveVariable(variable.Variable);
                            var right = EmitCopy(target);
                            var result = NewIrValue(unary.Type);

                            Emit(new IrUnary(unary.Op, right, result));
                            Emit(new IrVariable(target, result));

                            return unary.Op is Operation.PlusPlusRight or Operation.MinusMinusRight ? right : result;
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

        private void LowerIf(BoundIfDeclaration @if)
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
            else Emit(new IrJump(endLabel));

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

            if (@if.Else is null) Emit(new IrLabel(endLabel));
        }

        private void LowerWhile(BoundWhileDeclaration @while)
        {
            string whileLabel = NewLabelName("while");
            string endLabel = NewLabelName("end");

            _continues.Add(whileLabel);
            _breaks.Add(endLabel);

            Emit(new IrLabel(whileLabel));

            IrValue condition = LowerExpression(@while.Expression);

            Emit(new IrJumpIfNotTrue(condition, endLabel));

            foreach (var statement in @while.Statements)
                LowerNode(statement);

            _continues.RemoveAt(_continues.Count - 1);
            _breaks.RemoveAt(_breaks.Count - 1);

            Emit(new IrJump(whileLabel));
            Emit(new IrLabel(endLabel));
        }

        private void LowerFor(BoundForDeclaration @for)
        {
            if (@for.Initializer is not null) LowerNode(@for.Initializer);

            string forLabel = NewLabelName("for");
            string endLabel = NewLabelName("end");

            _continues.Add(forLabel);
            _breaks.Add(endLabel);

            Emit(new IrLabel(forLabel));
            IrValue condition;

            if (@for.Expression is not null) condition = LowerExpression(@for.Expression);
            else condition = LowerExpression(new BoundBoolExpression(true, new BoolTypeSymbol()));

            Emit(new IrJumpIfNotTrue(condition, endLabel));

            foreach (var statement in @for.Statements)
                LowerNode(statement);

            if (@for.Step is not null) LowerExpression(@for.Step);

            _continues.RemoveAt(_continues.Count - 1);
            _breaks.RemoveAt(_breaks.Count - 1);

            Emit(new IrJump(forLabel));
            Emit(new IrLabel(endLabel));
        }
    }
}
