using Best_language.src.middle;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Best_language.src.front
{
    public abstract class AstNode;
    public class ProgramNode(List<AstNode> nodes) : AstNode
    {
        public List<AstNode> Nodes { get; } = nodes;
    }

    public abstract class Expression : AstNode;
    public class NumberExpression(long value, VariableType type) : Expression
    {
        public long Value { get; } = value;
        public VariableType Type { get; } = type;
    }
    public class VariableExpression(string name) : Expression
    {
        public string Name { get; } = name;
    }
    public class BoolExpression(bool value) : Expression
    {
        public bool Value { get; } = value;
        public VariableType Type { get; } = VariableType.Bool;
    }
    public class BinaryExpression(Expression left, Operations op, Expression right) : Expression
    {
        public Expression Left { get; } = left;
        public Operations Op { get; } = op;
        public Expression Right { get; } = right;
    }
    public class UnaryExpression(Operations op, Expression right) : Expression
    {
        public Operations Op { get; } = op;
        public Expression Right { get; } = right;
    }
    public enum Operations
    {
        Plus,
        Minus,
        Multiply,
        Divide,
        DivisionRem,

        PlusEquals,
        MinusEquals,
        MultiplyEquals,
        DivideEquals,
        DivisionRemEquals,

        PlusPlusRight,
        PlusPlusLeft,
        MinusMinusRight,
        MinusMinusLeft,

        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        EqualsEquals,
        NotEquals,

        BitNot,
        BitXor,
        BitAnd,
        BitOr,

        BitAndEquals,
        BitOrEquals,
        BitXorEquals,

        RightShift,
        LeftShift,
        RightShiftEquals,
        LeftShiftEquals,

        And,
        Or,
        Not
    }

    public abstract class Statement : AstNode;
    public class VariableDeclaration(string name, VariableType type, Expression? initializer = null) : Statement
    {
        public string Name { get; } = name;
        public Expression? Initializer { get; } = initializer;

        public VariableType Type { get; } = type;
    }
    public class AssignmentStatement(string name, Operations? op, Expression right) : Statement
    {
        public string Name { get; } = name;

        public Operations? Op = op;
        public Expression Right { get; } = right;
    }
    public class GotoStatement(string to) : Statement
    {
        public string To { get; } = to;
    }
    public class GotoDeclaration(string name) : Statement
    {
        public string Name { get; } = name;
    }
    public class IfDeclaration(Expression expression, List<AstNode> codeBlock) : Statement
    {
        public Expression Expression { get; } = expression;
        public List<AstNode> CodeBlock { get; } = codeBlock;
    }

    public enum VariableType
    {
        Int,
        Bool,
        String
    }

    public static class AstPrinter
    {
        public static void Print(AstNode root)
        {
            Console.WriteLine("AST");
            PrintNode(root, "", true);
        }

        private static void PrintNode(AstNode node, string indent, bool last)
        {
            Console.Write(indent);
            Console.Write(last ? "└── " : "├── ");

            switch (node)
            {
                case ProgramNode main:
                    {
                            Console.WriteLine($"{nameof(ProgramNode)}");
                            for (int i = 0; i < main.Nodes.Count; i++)
                            {
                                PrintNode(main.Nodes[i], indent + (last ? "    " : "│   "), i == main.Nodes.Count - 1);
                            }
                            break;
                    }
                case VariableDeclaration variable:
                    {
                            Console.WriteLine($"{nameof(VariableDeclaration)}: {variable.Name} ({variable.Type})");
                            if (variable.Initializer is not null)
                            {
                                PrintNode(variable.Initializer, indent + (last ? "    " : "│   "), true);
                            }
                            break;
                    }
                case AssignmentStatement assignment:
                    {
                        Console.WriteLine($"{nameof(AssignmentStatement)}: {assignment.Name}");
                        PrintNode(assignment.Right, indent + (last ? "    " : "│   "), true);
                        break;
                    }
                case GotoDeclaration got:
                    {
                        Console.WriteLine($"{nameof(GotoDeclaration)}: {got.Name}");
                        break;
                    }
                case GotoStatement got:
                    {
                        Console.WriteLine($"{nameof(GotoStatement)}: {got.To}");
                        break;
                    }
                case IfDeclaration If:
                    {
                        Console.WriteLine($"{nameof(IfDeclaration)}:");
                        PrintNode(If.Expression, indent + (last ? "    " : "│   "), false);
                        for (int i = 0; i < If.CodeBlock.Count; i++)
                        {
                            PrintNode(If.CodeBlock[i], indent + (last ? "    " : "│   "), i == If.CodeBlock.Count - 1);
                        }
                        break;
                    }
                case UnaryExpression unary:
                    {
                        Console.WriteLine($"{nameof(UnaryExpression)}: {unary.Op}");
                        PrintNode(unary.Right, indent + (last ? "    " : "│   "), true);
                        break;
                    }
                case VariableExpression expression:
                    {
                        Console.WriteLine($"{nameof(VariableExpression)}: {expression.Name}");
                        break;
                    }
                case NumberExpression number:
                    {
                         Console.WriteLine($"{nameof(NumberExpression)}: {number.Value} ({number.Type})");
                         break;
                    }
                case BoolExpression expression:
                    {
                        Console.WriteLine($"{nameof(BoolExpression)}: {expression.Value} ({expression.Type})");
                        break;
                    }
                case BinaryExpression binary:
                    {
                        Console.WriteLine($"{nameof(BinaryExpression)}: {binary.Op}");
                        PrintNode(binary.Left, indent + (last ? "    " : "│   "), false);
                        PrintNode(binary.Right, indent + (last ? "    " : "│   "), true);
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
