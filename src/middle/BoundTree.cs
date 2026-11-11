using Best_language.src.front;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            return new BoundProgram(new List<BoundFunction>() { new BoundFunction(VariableType.Int, "main", statements) });
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

                        if(ifDeclaration.ElseIfs is not null)
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
                case GotoDeclaration @goto:
                    {
                        if(_gotoLabels.TryGetValue(@goto.Name, out int id))
                            return new BoundGotoDeclaration(id);
                        throw new Exception("Some odd Error in BoundTree");
                    }
                case GotoStatement @goto:
                    {
                        if (_gotoLabels.TryGetValue(@goto.To, out int id))
                            return new BoundGotoStatement(id);
                        throw new Exception($"@goto label: {@goto.To} not found");
                    }
                case AssignmentStatement assignment:
                    {
                        if (!scopes.Lookup(assignment.Name, out _)) throw new Exception($"Variable {assignment.Name} not exist");
                        if(assignment.Op is Operations op)
                            return new BoundAssignmentStatement(assignment.Name, BindExpression(new BinaryExpression(new VariableExpression(assignment.Name), op, assignment.Right)));
                        return new BoundAssignmentStatement(assignment.Name, BindExpression(assignment.Right));
                    }
            }
            throw new Exception("Failed to BindStatement");
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
