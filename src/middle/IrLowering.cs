using Best_language.src.front;
using Microsoft.Extensions.FileProviders;
using System.Net;
using System.Numerics;

namespace Best_language.src.middle
{
    public partial class IrLowering
    {
        private int _nextId;
        private int _nextLebelId;
        private TypeTable _typeTable = null!;
        private readonly IrProgram _currentProgram = new();
        private readonly Dictionary<string, IrGlobal> _globalVariables = [];
        private readonly Dictionary<VariableSymbol, IrValue> _variables = [];
        private readonly Stack<string> _breaks = [];
        private readonly Stack<string> _continues = [];

        private IrValue NewIrValue(TypeSymbol type) => new(_nextId++, type);
        private string NewLabelName(string name) => $"{name}_{_nextLebelId++}";

        public IrProgram Lower(BoundProgram mainNode, TypeTable typeTable)
        {
            _typeTable = typeTable;

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
                case BoundVariablesDeclaration variables:
                    foreach (var variable in variables.Variables) LowerNode(variable);
                    return;
                case BoundVariableDeclaration variable when variable.Variable.Type is ArrayTypeSymbol arrayType && variable.Initializer is BoundArrExpression initializer:
                    {
                        var array = LowerExpression(initializer);
                        _variables.Add(variable.Variable, array);
                        return;
                    }
                case BoundGlobalVariablesDeclaration variables:
                    foreach (var variable in variables.Variables) LowerNode(variable);
                    return;
                case BoundGlobalVariableDeclaration variable:
                    {
                        var name = variable.Variable.Name;
                        var type = variable.Variable.Type;

                        if (variable.Initializer is null)
                        {
                            var tmp = new IrGlobal(name, type);

                            Emit(tmp);

                            _globalVariables.Add(name, tmp);
                            return;
                        }

                        byte[] value = GetBytesFromConstant(variable.Initializer, type);
                        var global = new IrGlobal(name, type, value);

                        Emit(global);

                        _globalVariables.Add(name, global);
                        return;
                    }
                case BoundVariableDeclaration variable when variable.Variable.Type is ArrayTypeSymbol arrayType && variable.Initializer is BoundArrExpression initializer:
                    {
                        var array = LowerExpression(initializer);
                        _variables.Add(variable.Variable, array);
                        return;
                    }
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

                case BoundFuncDeclaration func:
                    LowerFunc(func);
                    return;

                case BoundSwitchDeclaration @switch:
                    string endLabel = NewLabelName("switch_end");
                    List<string> labels = [];
                    string? defaultStartLabel = null;

                    _breaks.Push(endLabel);

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
                            foreach (var statement in @case.CodeBlock) LowerNode(statement);
                        }
                        else
                        {
                            Emit(new IrLabel(labels[idx++]));
                            foreach (var statement in @case.CodeBlock) LowerNode(statement);
                        }
                    }

                    _breaks.Pop();

                    Emit(new IrLabel(endLabel));
                    return;

                case BoundCaseDeclaration @case:
                    if (@case.Expression is not null)
                    {
                        string caseEndLabel = NewLabelName("case_end");

                        IrValue condition = LowerExpression(@case.Expression);

                        Emit(new IrJumpIfNotTrue(condition, caseEndLabel));

                        foreach (var statement in @case.CodeBlock)
                            LowerNode(statement);

                        Emit(new IrLabel(caseEndLabel));
                    }
                    else
                    {
                        defaultStartLabel = NewLabelName("default_start");

                        Emit(new IrLabel(defaultStartLabel));

                        foreach (var statement in @case.CodeBlock)
                            LowerNode(statement);
                    }
                    return;

                case BoundBreakStatement:
                    Emit(new IrJump(_breaks.Peek()));
                        return;
                case BoundContinueStatement:
                    Emit(new IrJump(_continues.Peek()));
                    return;
                case BoundReturnStatement rt:
                    {
                        IrValue? res = null;
                        if (rt.Expression is not null) res = LowerExpression(rt.Expression);
                        Emit(new IrRet(res));
                        return;
                    }
                case BoundGotoDeclaration @goto:
                    Emit(new IrLabel("user_" + @goto.Id)); return;
                case BoundGotoStatement @goto:
                    Emit(new IrJump("user_" + @goto.ToId)); return;
                case BoundExpressionStatement ex:
                    {
                        LowerExpression(ex.Expression);
                        return;
                    }
                case BoundEmptyStatement:
                    return;
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
                case BoundVariableExpression variable when _globalVariables.TryGetValue(variable.Variable.Name, out var irGlobal):
                    {
                        var (address, offset) = LowerAddress(variable);
                        var res = NewIrValue(variable.Type);

                        Emit(new IrLoad(res, address, offset));
                        return res;
                    }
                case BoundVariableExpression variable:
                    return ResolveVariable(variable.Variable);
                case BoundBinaryExpression { Op: Operation.And or Operation.Or } binary:
                    {
                        bool isAnd = binary.Op is Operation.And;
                        var left = LowerExpression(binary.Left);
                        var result = NewIrValue(binary.Type);
                        string endLabel = NewLabelName("logical_end");

                        Emit(new IrVariable(result, EmitConstant(isAnd ? 0UL : 1UL, binary.Type)));

                        Emit(isAnd ? new IrJumpIfNotTrue(left, endLabel) : new IrJumpIfTrue(left, endLabel));

                        var right = LowerExpression(binary.Right);

                        Emit(new IrVariable(result, EmitConstant(isAnd ? 1UL : 0UL, binary.Type)));

                        Emit(isAnd ? new IrJumpIfNotTrue(right, endLabel) : new IrJumpIfTrue(right, endLabel));

                        Emit(new IrLabel(endLabel));
                        return result;
                    }
                case BoundBinaryExpression binary:
                    {
                        var left = LowerExpression(binary.Left);    
                        var right = LowerExpression(binary.Right);
                        var result = NewIrValue(binary.Type);

                        Emit(new IrBinary(binary.Op, left, right, result));

                        return result;
                    }
                case BoundFuncExpression func:
                    {
                        List<IrValue> @params = [];
                        for (int i = 0; i < func.Params.Count; i++)
                        {
                            @params.Add(LowerExpression(func.Params[i])); 
                        }

                        var res = NewIrValue(func.Type);

                        Emit(new IrCall(func.Name, @params, res));

                        return res;
                    } 
                case BoundMemberAccessExpression memberAccess:
                    {
                        var (address, offset) = LowerAddress(memberAccess);
                        var res = NewIrValue(memberAccess.Type);

                        Emit(new IrLoad(res, address, offset));
                        return res;
                    }
                case BoundAssignmentExpression { Op: Operation op} assignment:
                    {
                        var (address, offset) = LowerAddress(assignment.Left);
                        var tmp = EmitCopy(address);

                        var oldVal = NewIrValue(assignment.Left.Type);
                        Emit(new IrLoad(oldVal, tmp, offset));

                        var left = ConvertIrValue(oldVal, assignment.Left.Type);
                        var right = ConvertIrValue(LowerExpression(assignment.Right), assignment.Right.Type);

                        var res = NewIrValue(assignment.Type);
                        Emit(new IrBinary(op, left, right, res));

                        var val = ConvertIrValue(res, assignment.Left.Type);
                        Emit(new IrStore(tmp, offset, val));

                        return val;
                    }
                case BoundAssignmentExpression assignment when assignment.Left.Type is StructTypeSymbol st:
                    {
                        var right = LowerStruct(assignment.Right, st);
                        var (left, leftOffset) = LowerAddress(assignment.Left);

                        Emit(new IrCopyAggregate(left, leftOffset, right, 0, st.Size));

                        return right;
                    }
                case BoundAssignmentExpression assignment:
                    {
                        var val = LowerExpression(assignment.Right);

                        if (assignment.Left is BoundVariableExpression variable && !_globalVariables.ContainsKey(variable.Variable.Name))
                        {
                            Emit(new IrVariable(ResolveVariable(variable.Variable), val));
                        }
                        else
                        {
                            var (address, offset) = LowerAddress(assignment.Left);
                            Emit(new IrStore(address, offset, val));
                        }

                        return val;
                    }
                case BoundArrayAccessExpression arrAccess:
                    {
                        var (address, offset) = LowerAddress(arrAccess);
                        var res = NewIrValue(arrAccess.Type);

                        Emit(new IrLoad(res, address, offset));
                        return res;
                    }
                case BoundUnaryExpression { Op: Operation.ReferenceOf, Right: BoundArrayAccessExpression el}:
                    {
                        var (address, _) = LowerAddress(el);
                        return address;
                    }
                case BoundUnaryExpression unary:
                    {
                        bool increment = unary.Op is Operation.PlusPlusLeft or Operation.PlusPlusRight or Operation.MinusMinusLeft or Operation.MinusMinusRight;
                        
                        if (increment)
                        {
                            var (address, offset) = LowerAddress(unary.Right);

                            var oldVal = NewIrValue(unary.Right.Type);
                            Emit(new IrLoad(oldVal, address, offset));

                            var newVal = NewIrValue(unary.Type);
                            Emit(new IrUnary(unary.Op, oldVal, newVal));

                            Emit(new IrStore(address, offset, newVal));

                            return unary.Op is Operation.PlusPlusRight or Operation.MinusMinusRight ? oldVal : newVal;
                        }

                        var right = LowerExpression(unary.Right);
                        var result = NewIrValue(unary.Type);

                        Emit(new IrUnary(unary.Op, right, result));

                        return result;
                    }
                case BoundArrExpression st:
                    {
                        List<IrValue> Irs = [];

                        foreach (var element in st.Right) Irs.Add(LowerExpression(element));

                        return EmitConstant(Irs, st.Type);
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

            List<string> labels = [];
            for (int i = 0; i < @if.ElseIfs?.Count; i++) labels.Add(NewLabelName("elseIf"));
            string ifLabel = NewLabelName("if");
            string endLabel = NewLabelName("if_end");
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
                foreach (var statement in @if.ElseIfs[i].CodeBlock)
                    LowerNode(statement);
                Emit(new IrJump(endLabel));
            }
            // else part
            if (@if.Else is not null)
            {
                Emit(new IrLabel(elseLabel));
                foreach (var statement in @if.Else.CodeBlock)
                    LowerNode(statement);
            }

           Emit(new IrLabel(endLabel));
        }

        private void LowerWhile(BoundWhileDeclaration @while)
        {
            string whileLabel = NewLabelName("while");
            string endLabel = NewLabelName("while_end");

            _continues.Push(whileLabel);
            _breaks.Push(endLabel);

            Emit(new IrLabel(whileLabel));

            IrValue condition = LowerExpression(@while.Expression);

            Emit(new IrJumpIfNotTrue(condition, endLabel));

            foreach (var statement in @while.CodeBlock)
                LowerNode(statement);

            _continues.Pop();
            _breaks.Pop();

            Emit(new IrJump(whileLabel));
            Emit(new IrLabel(endLabel));
        }

        private void LowerFunc(BoundFuncDeclaration func)
        {
            Emit(new IrFunc(func.Name));

            for (int i = 0; i < func.Params.Count; i++)
            {
                var param = func.Params[i];
                var slot = NewIrValue(param.Variable.Type);

                _variables.Add(param.Variable, slot);
                Emit(new IrParam(slot, i));
            }

            foreach (var statement in func.CodeBlock)
                LowerNode(statement);

            if (_currentProgram.Instructions[^1] is not IrRet) Emit(new IrRet());
        }

        private void LowerFor(BoundForDeclaration @for)
        {
            if (@for.Initializer is not null) LowerNode(@for.Initializer);

            string forLabel = NewLabelName("for");
            string endLabel = NewLabelName("end");

            _continues.Push(forLabel);
            _breaks.Push(endLabel);

            Emit(new IrLabel(forLabel));
            IrValue condition;

            if (@for.Expression is not null) condition = LowerExpression(@for.Expression);
            else condition = LowerExpression(new BoundBoolExpression(true, new BoolTypeSymbol()));

            Emit(new IrJumpIfNotTrue(condition, endLabel));

            foreach (var statement in @for.Statements)
                LowerNode(statement);

            if (@for.Step is not null) LowerExpression(@for.Step);

            _continues.Pop();
            _breaks.Pop();

            Emit(new IrJump(forLabel));
            Emit(new IrLabel(endLabel));
        }
    }
}
