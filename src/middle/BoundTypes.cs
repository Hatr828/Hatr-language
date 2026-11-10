using Best_language.src.front;
using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.middle
{
    public abstract class BoundNode;
    public abstract class BoundStatement : BoundNode;
    public abstract class BoundExpression(VariableType type) : BoundNode
    {
        public VariableType Type { get; } = type;
    }
    public class BoundVariable(VariableType type, string name)
    {
        public VariableType Type { get; } = type;
        public string Name { get; } = name;
    }
    public class BoundProgram(List<BoundFunction> functions) : BoundNode
    {
        public List<BoundFunction> Functions { get; } = functions;
    }
    public class BoundFunction(VariableType type, string name, List<BoundStatement> statements) : BoundStatement
    {
        public VariableType Type { get; } = type;
        public string Name { get; } = name;
        public List<BoundStatement> Statements { get; } = statements;
    }
    public class BoundVariableDeclaration(VariableType type, string name, BoundExpression initializer) : BoundStatement
    {
        public VariableType Type { get; } = type;
        public string Name { get; } = name;
        public BoundExpression Initializer { get; } = initializer;
    }
    public class BoundAssignmentStatement(string name, BoundExpression right) : BoundStatement
    {
        public string Name { get; } = name;
        public BoundExpression Right { get; } = right;
    }
    public class BoundIfDeclaration(BoundExpression expression) : BoundStatement
    {
        public BoundExpression Expression { get; } = expression;
        public List<BoundStatement> Statements { get; } = new();
    }
    public class BoundGotoDeclaration(int id) : BoundStatement
    {
        public int Id { get; } = id;
    }
    public class BoundGotoStatement(int to) : BoundStatement
    {
        public int ToId { get; } = to;
    }
    public class BoundNumberExpression(VariableType type, long value) : BoundExpression(type)
    {
        public long Value { get; } = value;
    }
    public class BoundVariableExpression(VariableType type, string name) : BoundExpression(type)
    {
        public string Name { get; } = name;
    }
    public class BoundBoolExpression(bool value) : BoundExpression(VariableType.Bool)
    {
        public bool Value { get; } = value;
    }
    public class BoundBinaryExpression(VariableType type, Operations op, BoundExpression left, BoundExpression right) : BoundExpression(type)
    {
        public BoundExpression Left { get; } = left;
        public Operations Op { get; } = op;
        public BoundExpression Right { get; } = right;
    }
    public class BoundUnaryExpression(VariableType type, Operations op, BoundExpression right) : BoundExpression(type)
    {
        public Operations Op { get; } = op;
        public BoundExpression Right { get; } = right;
    }

    public class BoundScopes
    {
        private readonly List<Dictionary<string, BoundVariable>> _variables = new();

        public bool TryAddVariable(BoundVariable variable)
        {
            return _variables[^1].TryAdd(variable.Name, variable);
        }
        public bool Lookup(string name, out VariableType type)
        {
            for(int i = 1; i <= _variables.Count; i++)
            {
                if (_variables[^i].TryGetValue(name, out var variable))
                {
                    type = variable.Type;
                    return true;
                }
            }
            type = VariableType.Int;
            return false;
        }
        public void AddScope()
        {
            _variables.Add(new());
        }
        public void RemoveScope()
        {
            _variables.RemoveAt(_variables.Count - 1);
        }
    }
    public static class BoundTreePrinter
    {
        public static void Print(BoundNode root)
        {
            Console.WriteLine("BoundTree");
            PrintBoundTree(root, "", true);
        }

        private static void PrintBoundTree(BoundNode node, string indent, bool last)
        {
            Console.Write(indent);
            Console.Write(last ? "└── " : "├── ");

            switch (node)
            {
                case BoundProgram main:
                    {
                        Console.WriteLine($"{nameof(BoundProgram)}");
                        for (int i = 0; i < main.Functions.Count; i++)
                        {
                            PrintBoundTree(main.Functions[i], indent + (last ? "    " : "│   "), i == main.Functions.Count - 1);
                        }
                        break;
                    }
                case BoundVariableDeclaration variable:
                    {
                        Console.WriteLine($"{nameof(BoundVariableDeclaration)}: {variable.Name} ({variable.Type})");
                        if (variable.Initializer is not null)
                        {
                            PrintBoundTree(variable.Initializer, indent + (last ? "    " : "│   "), true);
                        }
                        break;
                    }
                case BoundIfDeclaration If:
                    {
                        Console.WriteLine($"{nameof(BoundIfDeclaration)}:");
                        PrintBoundTree(If.Expression, indent + (last ? "    " : "│   "), false);
                        for (int i = 0; i < If.Statements.Count; i++)
                        {
                            PrintBoundTree(If.Statements[i], indent + (last ? "    " : "│   "), i == If.Statements.Count - 1);
                        }
                        break;
                    }
                case BoundGotoDeclaration @goto:
                    {
                        Console.WriteLine($"{nameof(BoundGotoDeclaration)}: {@goto.Id}");
                        break;
                    }
                case BoundGotoStatement @goto:
                    {
                        Console.WriteLine($"{nameof(BoundGotoStatement)}: {@goto.ToId}");
                        break;
                    }
                case BoundNumberExpression number:
                    {
                        Console.WriteLine($"{nameof(BoundNumberExpression)}: {number.Value} ({number.Type})");
                        break;
                    }
                case BoundVariableExpression number:
                    {
                        Console.WriteLine($"{nameof(BoundVariableExpression)}: {number.Name} ({number.Type})");
                        break;
                    }
                case BoundBoolExpression number:
                    {
                        Console.WriteLine($"{nameof(BoundBoolExpression)}: {number.Value} ({number.Type})");
                        break;
                    }
                case BoundBinaryExpression binary:
                    {
                        Console.WriteLine($"{nameof(BoundVariableExpression)}: {binary.Op} {binary.Type}");
                        PrintBoundTree(binary.Left, indent + (last ? "    " : "│   "), false);
                        PrintBoundTree(binary.Right, indent + (last ? "    " : "│   "), true);
                        break;
                    }
                case BoundUnaryExpression binary:
                    {
                        Console.WriteLine($"{nameof(BoundUnaryExpression)}: {binary.Op} {binary.Type}");
                        PrintBoundTree(binary.Right, indent + (last ? "    " : "│   "), true);
                        break;
                    }
                case BoundFunction function:
                    {
                        Console.WriteLine($"{nameof(BoundFunction)}: {function.Name}");
                        for (int i = 0; i < function.Statements.Count; i++)
                        {
                            PrintBoundTree(function.Statements[i], indent + (last ? "    " : "│   "), i == function.Statements.Count - 1);
                        }
                        break;
                    }

                default:
                    {
                        Console.WriteLine(node.GetType());
                        break;
                    }
            }
        }
    }
}
