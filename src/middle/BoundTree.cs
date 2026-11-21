using Best_language.src.front;

namespace Best_language.src.middle
{
    public partial class BoundTree
    {
        private BoundScopes scopes = new();
        private Dictionary<string, int> _gotoLabels = new();
        private TypeTable _typeTable = new TypeTable();
        private int _labelId;

        public BoundProgram BindProgram(ProgramNode mainNode, TypeTable typeTable)
        {
            _typeTable = typeTable;

            PreProcessAst(mainNode.Nodes);

            List<BoundStatement> statements = new();

            scopes.AddScope();
            foreach (AstNode node in mainNode.Nodes)
            {
                statements.Add(BindStatement(node));
            }
            scopes.RemoveScope();

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
                        foreach (var variable in variables.variables) variables2.variables.Add((BoundVariableDeclaration)BindStatement(variable));

                        return variables2;
                    }
                case VariableDeclaration variable:
                    {
                        BoundVariableDeclaration temp;

                        var tmp = new VariableSymbol(variable.Name, variable.Type);
                        AddVariableToScope(tmp);

                        if (variable.Initializer is not null)
                        {
                            var initializer = BindExpression(variable.Initializer);
                            temp = new BoundVariableDeclaration(tmp, BindConversion(initializer, variable.Type));
                        }
                        else
                            temp = new BoundVariableDeclaration(tmp, null);

                        return temp;
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

                            scopes.AddScope();
                            foreach (var statement in @case.Statements) res.Cases[^1].Statements.Add(BindStatement(statement));
                            scopes.RemoveScope();
                        }
                        return res;
                    }

                case IfDeclaration @if:
                        return BindIf(@if);

                case ElseIfDeclaration elseIf:
                    {
                        var If = new BoundElseIfDeclaration(new 
                            BoundConversionExpression(BindExpression(elseIf.Expression), _typeTable.Bool));
                        scopes.AddScope();
                        foreach (var statement in elseIf.CodeBlock) If.Statements.Add(BindStatement(statement));
                        scopes.RemoveScope();
                        return If;
                    }
                case WhileDeclaration @while:
                    {
                        var res = new BoundWhileDeclaration(new 
                            BoundConversionExpression(BindExpression(@while.Expression), _typeTable.Bool));
                        scopes.AddScope();
                        foreach (var statement in @while.CodeBlock) res.Statements.Add(BindStatement(statement));
                        scopes.RemoveScope();
                        return res;
                    }
                case FuncDeclaration func:
                    {
                        List<BoundStatement> codeBlock = new();
                        List<BoundParamsDeclaration> @params = new();
                        scopes.AddScope();
                        foreach (var param in func.Params)
                        {
                            if(param.Initializer is not null)
                                @params.Add(new(param.Type, param.Name, BindExpression(param.Initializer)));
                            else
                                @params.Add(new(param.Type, param.Name));

                            AddVariableToScope(new VariableSymbol(param.Name, param.Type));
                        }
                        foreach (var statement in func.CodeBlock) codeBlock.Add(BindStatement(statement));
                        scopes.RemoveScope();
                        return new BoundFunction(func.ReturnType, func.Name, @params, codeBlock);
                    }                             
                case ForDeclaration @for:
                    {
                        scopes.AddScope();
                        BoundStatement? initializer = null;
                        BoundExpression? step = null, ex = null;

                        if (@for.Initializer is not null) initializer = BindStatement(@for.Initializer);
                        if (@for.Step is not null) step = BindExpression(@for.Step);
                        if (@for.Expression is not null) 
                            ex = new  BoundConversionExpression(BindExpression(@for.Expression), _typeTable.Bool);

                        BoundForDeclaration res = new(initializer, ex, step);

                        foreach (var statement in @for.CodeBlock) res.Statements.Add(BindStatement(statement));

                        scopes.RemoveScope();
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
                        return new BoundReturnStatement(BindExpression(st.Expression!));
                    }
                case BreakStatement:
                    {
                        return new BoundBreakStatement();
                    }
                case ContinueStatement:
                    {
                        return new BoundContinueStatement();
                    }
                case AssignmentStatement assignment:
                    {
                        IsVariableExist(assignment.Name, out TypeSymbol type);

                        BoundExpression ex;

                        if (assignment.Op is Operation op)
                        {
                            ex = BindExpression(new BinaryExpression(new VariableExpression(assignment.Name), 
                                GetOperationFromCompoundOperation(op), assignment.Right));

                            GetVariable(assignment.Name, out var variable);

                            return new BoundAssignmentStatement(variable, ex);
                        }

                        ex = BindExpression(assignment.Right);
                        GetVariable(assignment.Name, out var variable2);

                        return new BoundAssignmentStatement(variable2, ex);
                    }
                case UnaryExpression unary:
                    {
                        return new BoundExpressionStatement(BindExpression(unary));
                    }
                case ExpressionStatement ex: 
                    {
                        return new BoundExpressionStatement(BindExpression(ex.Right));
                    }
            }
            throw new ArgumentException("Failed to bind a statement");
        }

        private BoundExpression BindExpression(Expression expression, bool constantOnly = false)
        {
            switch (expression)
            {
                case NumberExpression number:
                    return new BoundNumberExpression(number.Type, number.Value);
                case DoubleExpression @double:
                    return new BoundDoubleExpression(@double.Type, @double.Value);
                case VariableExpression variable:
                    {
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
            throw new ArgumentException("Failed to BindExpression");
        }

        private BoundStatement BindIf(IfDeclaration @if)
        {
            var If = new BoundIfDeclaration(new BoundConversionExpression(BindExpression(@if.Expression), _typeTable.Bool));

            scopes.AddScope();

            foreach (var statement in @if.CodeBlock) If.Statements.Add(BindStatement(statement));

            scopes.RemoveScope();

            if (@if.ElseIfs is not null)
            {
                If.ElseIfs ??= new();
                foreach (var elseIf in @if.ElseIfs) If.ElseIfs.Add((BoundElseIfDeclaration)BindStatement(elseIf));
            }
            if (@if.Else is not null)
            {
                scopes.AddScope();
                If.Else ??= new();
                foreach (var statement in @if.Else.CodeBlock) If.Else.Statements.Add(BindStatement(statement));
                scopes.RemoveScope();
            }

            return If;
        }
    }
}
