using Best_language.src.front;

namespace Best_language.src.middle
{
    public class BoundTree
    {
        private BoundScopes scopes = new();
        private Dictionary<string, int> _gotoLabels = new();
        private int _labelId;

        public BoundProgram BindProgram(ProgramNode mainNode)
        {
            PreProcessAst(mainNode.Nodes);

            List<BoundStatement> statements = new();
            scopes.AddScope();

            foreach (AstNode node in mainNode.Nodes)
            {
                statements.Add(BindStatement(node));
            }
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
                            if (_gotoLabels.ContainsKey(@goto.Name)) throw new Exception($"This label already exist: {@goto.Name}");
                            _gotoLabels.Add(@goto.Name, _labelId++);
                            continue;
                        }
                    case IfDeclaration ifDeclaration:
                        {
                            PreProcessAst(ifDeclaration.CodeBlock);
                            continue;
                        }
                }
            }
        }

        public BoundStatement BindStatement(AstNode node)
        {
            switch (node)
            {
                case VariableDeclaration variable:
                    {
                        if (scopes.Lookup(variable.Name, out _)) throw new Exception($"Variable {variable.Name} already exist");

                        BoundVariableDeclaration temp;

                        if (variable.Initializer is not null)
                            temp = new BoundVariableDeclaration(variable.Type, variable.Name, BindExpression(variable.Initializer));
                        else
                            temp = new BoundVariableDeclaration(variable.Type, variable.Name, null!);

                        if (!scopes.TryAddVariable(new(variable.Type, variable.Name))) throw new Exception($"Error in TryAddVariable");
                        return temp;
                    }
                case IfDeclaration ifDeclaration:
                    {
                        var If = new BoundIfDeclaration(BindExpression(ifDeclaration.Expression));
                        scopes.AddScope();
                        foreach (var statement in ifDeclaration.CodeBlock) If.Statements.Add(BindStatement(statement));
                        scopes.RemoveScope();

                        if (ifDeclaration.ElseIfs is not null)
                        {
                            If.ElseIfs ??= new();
                            foreach (var elseIf in ifDeclaration.ElseIfs) If.ElseIfs.Add((BoundElseIfDeclaration)BindStatement(elseIf));
                        }
                        if (ifDeclaration.Else is not null)
                        {
                            scopes.AddScope();
                            If.Else ??= new();
                            foreach (var statement in ifDeclaration.Else.CodeBlock) If.Else.Statements.Add(BindStatement(statement));
                            scopes.RemoveScope();
                        }

                        return If;
                    }
                case ElseIfDeclaration elseIfDeclaration:
                    {
                        var If = new BoundElseIfDeclaration(BindExpression(elseIfDeclaration.Expression));
                        scopes.AddScope();
                        foreach (var statement in elseIfDeclaration.CodeBlock) If.Statements.Add(BindStatement(statement));
                        scopes.RemoveScope();
                        return If;
                    }
                case WhileDeclaration whileDeclaration:
                    {
                        var @while = new BoundWhileDeclaration(BindExpression(whileDeclaration.Expression));
                        scopes.AddScope();
                        foreach (var statement in whileDeclaration.CodeBlock) @while.Statements.Add(BindStatement(statement));
                        scopes.RemoveScope();
                        return @while;
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

                            if (!scopes.TryAddVariable(new(param.Type, param.Name))) throw new ArgumentException($"variable already exist: {param.Name}");
                        }
                        foreach (var statement in func.CodeBlock) codeBlock.Add(BindStatement(statement));
                        scopes.RemoveScope();
                        return new BoundFunction(func.ReturnType, func.Name, @params, codeBlock);
                    }                             
                case ForDeclaration forDeclaration:
                    {
                        scopes.AddScope();
                        BoundStatement? initializer = null;
                        BoundExpression? step = null, ex = null;

                        if (forDeclaration.Initializer is not null) initializer = BindStatement(forDeclaration.Initializer);
                        if (forDeclaration.Step is not null) step = BindExpression(forDeclaration.Step);
                        if (forDeclaration.Expression is not null) ex = BindExpression(forDeclaration.Expression);

                        BoundForDeclaration @for = new(initializer, ex, step);

                        foreach (var statement in forDeclaration.CodeBlock) @for.Statements.Add(BindStatement(statement));

                        scopes.RemoveScope();
                        return @for;
                    }
                case GotoDeclaration @goto:
                    {
                        if (_gotoLabels.TryGetValue(@goto.Name, out int id))
                            return new BoundGotoDeclaration(id);
                        throw new Exception("Some odd Error in BoundTree");
                    }
                case GotoStatement @goto:
                    {
                        if (_gotoLabels.TryGetValue(@goto.To, out int id))
                            return new BoundGotoStatement(id);
                        throw new Exception($"@goto label: {@goto.To} not found");
                    }
                case ReturnStatement st:
                    {
                        return new BoundReturnStatement(BindExpression(st.Expression));
                    }
                case AssignmentStatement assignment:
                    {
                        if (!scopes.Lookup(assignment.Name, out VariableType type)) throw new Exception($"Variable {assignment.Name} not exist");

                        BoundExpression ex;

                        if (assignment.Op is Operations op)
                        {
                            ex = BindExpression(new BinaryExpression(new VariableExpression(assignment.Name), op, assignment.Right));
                            return new BoundAssignmentStatement(assignment.Name, ex, type);
                        }

                        ex = BindExpression(assignment.Right!);
                        return new BoundAssignmentStatement(assignment.Name, ex, type);
                    }
                case UnaryExpression unary:
                    {
                        return new BoundExpressionStatement(BindExpression(unary));
                    }
            }
            throw new Exception("Failed to bind a statement");
        }

        public BoundExpression BindExpression(Expression expression)
        {
            switch (expression)
            {
                case NumberExpression number:
                    {
                        return new BoundNumberExpression(number.Type, number.Value);
                    }
                case VariableExpression variable:
                    {
                        if (!scopes.Lookup(variable.Name, out VariableType type)) throw new Exception($"Variable {variable.Name} not exist");
                        return new BoundVariableExpression(type, variable.Name);
                    }
                case BoolExpression ex:
                    {
                        return new BoundBoolExpression(ex.Value);
                    }
                case BinaryExpression binary:
                    {
                        var left = BindExpression(binary.Left);
                        var right = BindExpression(binary.Right);

                        if (left.Type != right.Type) throw new Exception("Types of both sides should be the same");

                        return new BoundBinaryExpression(left.Type, binary.Op, left, right);
                    }
                case UnaryExpression unary:
                    {
                        var right = BindExpression(unary.Right);

                        return new BoundUnaryExpression(right.Type, unary.Op, right);
                    }
            }
            throw new Exception("Failed to BindExpression");
        }
    }
}
