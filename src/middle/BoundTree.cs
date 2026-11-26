using Best_language.src.front;
using System.Diagnostics.Metrics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Best_language.src.middle
{
    public partial class BoundTree
    {
        private BoundScopes _scopes = new();
        private Dictionary<string, int> _gotoLabels = new();
        private TypeTable _typeTable = null!;
        private int _labelId;

        public BoundProgram BindProgram(ProgramNode mainNode, TypeTable typeTable)
        {
            _typeTable = typeTable;

            PreProcessAst(mainNode.Nodes);

            List<BoundStatement> statements = new();

            _scopes.AddScope();
            foreach (AstNode node in mainNode.Nodes)
            {
                statements.Add(BindStatement(node));
            }
            _scopes.RemoveScope();

            return new BoundProgram(statements);
        }
        private void PreProcessAst(List<AstNode> nodes)
        {
            foreach (AstNode node in nodes)
            {
                switch (node)
                {
                    case GotoDeclaration @goto:
                        {
                            AddGotoLabel(@goto.Name);
                            continue;
                        }
                    case IfDeclaration @if:
                        {
                            PreProcessAst(@if.CodeBlock);

                            foreach (var elseifs in @if.ElseIfs ?? [])
                                PreProcessAst(elseifs.CodeBlock);

                            if (@if.Else is not null) PreProcessAst(@if.Else.CodeBlock);

                            continue;
                        }
                    case FuncDeclaration func:
                        {
                            PreProcessAst(func.CodeBlock);

                            if (!_scopes.TryAddFunction(func.Name, func.Type)) throw new ArgumentException("Function already exist");

                            continue;
                        }
                    case SwitchDeclaration @switch:
                        {
                            foreach (var @case in @switch.Cases ?? [])
                                PreProcessAst(@case.Statements);
                            continue;
                        }
                    case ForDeclaration forDeclaration:
                        {
                            PreProcessAst(forDeclaration.CodeBlock);
                            continue;
                        }
                    case WhileDeclaration whileDeclaration:
                        {
                            PreProcessAst(whileDeclaration.CodeBlock);
                            continue;
                        }
                }
            }
        }

        private BoundStatement BindStatement(AstNode node)
        {
            switch (node)
            {
                case VariableDeclarations variables:
                    {
                        BoundVariablesDeclaration variables2 = new();
                        foreach (var variable in variables.Variables) variables2.variables.Add((BoundVariableDeclaration)BindStatement(variable));

                        return variables2;
                    }
                case VariableDeclaration variable:
                    {
                        BoundVariableDeclaration temp;

                        var type = ResolveEnumType(variable.Type);
                        type = ResolveStructType(variable.Type);

                        var tmp = new VariableSymbol(variable.Name, type);
                        AddVariableToScope(tmp);

                        if (variable.Initializer is not null)
                        {
                            if (variable.Initializer is ArrayExpression st && type is StructTypeSymbol tmpType)
                            {
                                List<BoundExpression> expressions = new();

                                for(int i = tmpType.Types.Count - 1; i > -1; i--)
                                {
                                    if (st.Right.Count > i)
                                        expressions.Add(BindConversion(BindExpression(st.Right[i]), tmpType.Types[i]));
                                    else
                                        expressions.Add(BindConversion(BindExpression(new NumberExpression(0, tmpType.Types[i])), tmpType.Types[i]));
                                }
                                temp = new BoundVariableDeclaration(tmp, new BoundArrExpression(expressions, type));
                            }
                            else if (variable.Initializer is ArrayExpression arrEx && type is ArrayTypeSymbol tempType)
                            {
                                List<BoundExpression> expressions = new();

                                for (int i = (int)tempType.Size - 1; i > -1; i--)
                                {
                                    if (arrEx.Right.Count > i)
                                        expressions.Add(BindConversion(BindExpression(arrEx.Right[i]), tempType.Type));
                                    else
                                        expressions.Add(BindConversion(BindExpression(new NumberExpression(0, tempType.Type)), tempType.Type));
                                }
                                temp = new BoundVariableDeclaration(tmp, new BoundArrExpression(expressions, type));
                            }
                            else
                            {
                                var initializer = BindExpression(variable.Initializer);
                                temp = new BoundVariableDeclaration(tmp, BindConversion(initializer, type));
                            }
                        }
                        else
                            temp = new BoundVariableDeclaration(tmp, null);

                        return temp;
                    }
                case EnumDeclaration @enum:
                    {
                        foreach (var dict in @enum.Type.Members)
                            if (!_scopes.TryAddEnum(dict.Key, dict.Value, @enum.Type.EnumType)) throw new ArgumentException($"Variable/Function/etc exist with same name as {dict.Key}"); 

                        if (@enum.Type.Name is not null && !_scopes.TryAddEnum(@enum.Type))
                                throw new ArgumentException($"Enum with same name already exist {@enum.Type.Name}");

                        BoundVariablesDeclaration variables = new();

                        foreach (var variable in @enum.Variables) variables.variables.Add((BoundVariableDeclaration)BindStatement(variable));

                        return variables;
                    }
                case StructDeclaration @struct:
                    {
                        if (@struct.Type.Name is not null && !_scopes.TryAddStruct(@struct.Type, @struct.Names))
                            throw new ArgumentException($"Enum with same name already exist {@struct.Type.Name}");

                        BoundVariablesDeclaration variables = new();

                        foreach (var variable in @struct.Variables ?? []) variables.variables.Add((BoundVariableDeclaration)BindStatement(variable));

                        return variables;
                    }
                case SwitchDeclaration @switch:
                    {
                        var res = new BoundSwitchDeclaration();

                        var switchEx = BindExpression(@switch.Expression);
                        if (switchEx.Type is not IntegerTypeSymbol and not BoolTypeSymbol) throw new ArgumentException("Expected integer");

                        foreach (var @case in @switch.Cases)
                        {
                            BoundExpression? caseEx = null;

                            if (@case.Expression is not null)
                                caseEx = BindExpression(@case.Expression, true);

                            if (caseEx is not null && caseEx.Type is not IntegerTypeSymbol and not BoolTypeSymbol) throw new ArgumentException("Expected integer");

                            if (caseEx is not null)
                            {
                                var ex = new BoundBinaryExpression(_typeTable.Bool, Operation.EqualsEquals, switchEx, caseEx);
                                res.Cases.Add(new(ex, false));
                            }
                            else res.Cases.Add(new(null, true));

                            _scopes.AddScope();
                            foreach (var statement in @case.Statements) res.Cases[^1].Statements.Add(BindStatement(statement));
                            _scopes.RemoveScope();
                        }
                        return res;
                    }

                case IfDeclaration @if:
                        return BindIf(@if);

                case ElseIfDeclaration elseIf:
                    {
                        var If = new BoundElseIfDeclaration(new 
                            BoundConversionExpression(BindExpression(elseIf.Expression), _typeTable.Bool));
                        _scopes.AddScope();
                        foreach (var statement in elseIf.CodeBlock) If.Statements.Add(BindStatement(statement));
                        _scopes.RemoveScope();
                        return If;
                    }
                case WhileDeclaration @while:
                    {
                        var res = new BoundWhileDeclaration(new 
                            BoundConversionExpression(BindExpression(@while.Expression), _typeTable.Bool));
                        _scopes.AddScope();
                        foreach (var statement in @while.CodeBlock) res.Statements.Add(BindStatement(statement));
                        _scopes.RemoveScope();
                        return res;
                    }
                case FuncDeclaration func:
                    {
                        List<BoundStatement> codeBlock = new();
                        List<BoundVariableDeclaration> @params = new();

                        _scopes.AddScope();
                        foreach (var param in func.Params)
                        {
                            if (param.Initializer is not null) throw new ArgumentException("Cannot have function with default params");
                            @params.Add((BoundVariableDeclaration)BindStatement(param));
                        }

                        foreach (var statement in func.CodeBlock) codeBlock.Add(BindStatement(statement));
                        _scopes.RemoveScope();

                        _scopes.TryGetFunction(func.Name, out var type);

                        return new BoundFuncDeclaration(type, func.Name, @params, codeBlock);
                    }                             
                case ForDeclaration @for:
                    {
                        _scopes.AddScope();
                        BoundStatement? initializer = null;
                        BoundExpression? step = null, ex = null;

                        if (@for.Initializer is not null) initializer = BindStatement(@for.Initializer);
                        if (@for.Step is not null) step = BindExpression(@for.Step);
                        if (@for.Expression is not null) 
                            ex = new  BoundConversionExpression(BindExpression(@for.Expression), _typeTable.Bool);

                        BoundForDeclaration res = new(initializer, ex, step);

                        foreach (var statement in @for.CodeBlock) res.Statements.Add(BindStatement(statement));

                        _scopes.RemoveScope();
                        return res;
                    }
                case GotoDeclaration @goto:
                    {
                        if (_gotoLabels.TryGetValue(@goto.Name, out int id))
                            return new BoundGotoDeclaration(id);
                        throw new ArgumentException("Some odd Error in BoundTree");
                    }
                case GotoStatement @goto:
                    {
                        if (_gotoLabels.TryGetValue(@goto.To, out int id))
                            return new BoundGotoStatement(id);
                        throw new ArgumentException($"@goto label: {@goto.To} not found");
                    }
                case ReturnStatement st:
                    {
                        if (st.Expression is not null)
                            return new BoundReturnStatement(BindExpression(st.Expression));
                        else
                            return new BoundReturnStatement();
                    }
                case BreakStatement:
                    {
                        return new BoundBreakStatement();
                    }
                case ContinueStatement:
                    {
                        return new BoundContinueStatement();
                    }
                case MemberAssignment assignment:
                    {
                        BoundExpression ex;

                        if (assignment.Op is Operation op)
                        {
                            ex = BindExpression(new BinaryExpression(new VariableExpression(assignment.Name),
                                GetOperationFromCompoundOperation(op), assignment.Initializer));
                        }
                        else ex = BindExpression(assignment.Initializer);

                        var target = BindExpression(assignment.Target);

                        if (target.Type is not StructTypeSymbol structType) throw new ArgumentException("Expected struct"); 

                        if (!TryGetOffsetAndType(structType.Name, assignment.Name, out var offset, out var type)) throw new ArgumentException("Unknown member or struct");

                        return new BoundAssignmentMemberStatement(target, new(assignment.Name, type, offset), ex);
                    }
                case AssignmentStatement assignment:
                    {
                        BoundExpression ex;

                        if (assignment.Op is Operation op)
                        {
                            ex = BindExpression(new BinaryExpression(assignment.Left, 
                                GetOperationFromCompoundOperation(op), assignment.Right));
                        }
                        else ex = BindExpression(assignment.Right);

                        return new BoundAssignmentStatement(BindExpression(assignment.Left), ex);
                    }
                case ArrayAssignment assignment:
                    {
                        BoundExpression ex;

                        if (assignment.Op is Operation op)
                        {
                            ex = BindExpression(new BinaryExpression(assignment.Left,
                                GetOperationFromCompoundOperation(op), assignment.Right));
                        }
                        else ex = BindExpression(assignment.Right);

                        BoundExpression idx = BindExpression(assignment.Idx);
                        BoundExpression left = BindExpression(assignment.Left);

                        if (idx.Type is not IntegerTypeSymbol) throw new ArgumentException("Expected integer");
                        if (left.Type is not ArrayTypeSymbol arrType) throw new ArgumentException("Expected array");

                        return new BoundArrayAssignment(left, ex, idx);
                    }
                case UnaryExpression unary:
                    {
                        return new BoundExpressionStatement(BindExpression(unary));
                    }
                case ExpressionStatement ex: 
                    {
                        return new BoundExpressionStatement(BindExpression(ex.Right));
                    }
                case EmptyNode:
                    return new BoundEmptyStatement();
            }
            throw new ArgumentException($"Failed to bind a statement: {node}");
        }

        private BoundExpression BindExpression(Expression expression, bool constantOnly = false)
        {
            switch (expression)
            {
                case NumberExpression number:
                    return new BoundNumberExpression(number.Type, number.Value);
                case DoubleExpression @double:
                    return new BoundDoubleExpression(@double.Type, @double.Value);
                case MemberAccessExpression member:
                    {
                        if (constantOnly) throw new ArgumentException("Expected constants only");

                        var target = BindExpression(member.Target);

                        if (target.Type is not StructTypeSymbol structType) throw new ArgumentException("Expected struct");

                        if (!TryGetOffsetAndType(structType.Name, member.Name, out var offset, out var type)) throw new ArgumentException("Unknown member or struct");

                        return new BoundMemberAccessExpression(target, new(member.Name, type, offset));
                    }
                case ArrayAccessExpression member:
                    {
                        if (constantOnly) throw new ArgumentException("Expected constants only");

                        var idx = BindExpression(member.Idx);

                        _scopes.TryGetVariable(member.Target, out var variable);

                        if (idx.Type is not IntegerTypeSymbol) throw new ArgumentException("Expected integer");

                        if (variable.Type is ArrayTypeSymbol tmp)
                            return new BoundArrayAccessExpression(variable, idx, tmp.Type);
                        else if (variable.Type is PointerTypeSymbol pt)
                            return new BoundArrayAccessExpression(variable, idx, pt.ElementType);
                        else throw new ArgumentException("Expected array or pointer");
                    }
                case FuncExpression func:
                    {
                        if (constantOnly) throw new ArgumentException("Expected constants only");
                        if (!_scopes.TryGetFunction(func.Name, out var type)) throw new ArgumentException("Unknown function");

                        List<BoundExpression> @params = new();

                        foreach (var param in func.Params)
                        {
                            var ex = BindExpression(param);
                            if (ex.Type is ArrayTypeSymbol st)
                            {
                                if (ex is not BoundVariableExpression variable) throw new ArgumentException("Expected variable");
                                ex = new BoundUnaryExpression(_typeTable.GetPointer(st.Type), Operation.ReferenceOf, ex);
                            }
                            @params.Add(ex);
                        }

                        return new BoundFuncExpression(func.Name, @params, type);
                    }
                case VariableExpression variable:
                    {
                        if (_scopes.TryGetEnumValue(variable.Name, out var val)) return new BoundNumberExpression(val.Item2, val.Item1);
                        if (constantOnly) throw new ArgumentException("Expected constants only");

                        GetVariable(variable.Name, out var variableSymbol);
                        return new BoundVariableExpression(variableSymbol);
                    }
                case BoolExpression ex:
                    {
                        return new BoundBoolExpression(ex.Value, ex.Type);
                    }
                case BinaryExpression binary:
                    {
                        return BindBinaryExpression(binary);
                    }
                case UnaryExpression unary:
                    {
                         return BindUnaryExpression(unary);
                    }
            }
            throw new ArgumentException($"Failed to BindExpression: {expression}");
        }

        private BoundStatement BindIf(IfDeclaration @if)
        {
            var If = new BoundIfDeclaration(new BoundConversionExpression(BindExpression(@if.Expression), _typeTable.Bool));

            _scopes.AddScope();

            foreach (var statement in @if.CodeBlock) If.Statements.Add(BindStatement(statement));

            _scopes.RemoveScope();

            if (@if.ElseIfs is not null)
            {
                If.ElseIfs ??= new();
                foreach (var elseIf in @if.ElseIfs) If.ElseIfs.Add((BoundElseIfDeclaration)BindStatement(elseIf));
            }
            if (@if.Else is not null)
            {
                _scopes.AddScope();
                If.Else ??= new();
                foreach (var statement in @if.Else.CodeBlock) If.Else.Statements.Add(BindStatement(statement));
                _scopes.RemoveScope();
            }

            return If;
        }
    }
}
