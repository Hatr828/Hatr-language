using Best_language.src.middle;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Best_language.src.front
{
    public abstract class AstNode
    {
        public abstract string DName { get; }
        public abstract IEnumerable<AstNode> Children { get; }
    }
    public class ProgramNode(List<AstNode> nodes) : AstNode
    {
        public List<AstNode> Nodes { get; } = nodes;
        public override string DName => $"{nameof(ProgramNode)}";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                foreach (AstNode node in Nodes) yield return node;
            }
        }
    }

    public abstract class Expression : AstNode;
    public class NumberExpression(long value, VariableType type) : Expression
    {
        public long Value { get; } = value;
        public VariableType Type { get; } = type;
        public override string DName => $"{nameof(NumberExpression)}: {Value} ({Type})";
        public override IEnumerable<AstNode> Children => [];
    }
    public class VariableExpression(string name) : Expression
    {
        public string Name { get; } = name;
        public override string DName => $"{nameof(VariableExpression)}: {Name}";
        public override IEnumerable<AstNode> Children => [];
    }
    public class BoolExpression(bool value) : Expression
    {
        public bool Value { get; } = value;
        public VariableType Type { get; } = VariableType.Bool;
        public override string DName => $"{nameof(BoolExpression)}: {Value}";
        public override IEnumerable<AstNode> Children => [];
    }
    public class BinaryExpression(Expression left, Operations op, Expression right) : Expression
    {
        public Expression Left { get; } = left;
        public Operations Op { get; } = op;
        public Expression Right { get; } = right;
        public override string DName => $"{nameof(BinaryExpression)}: {Op}";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                yield return Left;
                yield return Right;
            }
        }
    }
    public class UnaryExpression(Operations op, Expression right) : Expression
    {
        public Operations Op { get; } = op;
        public Expression Right { get; } = right;
        public override string DName => $"{nameof(UnaryExpression)}: {Op}";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                yield return Right;
            }
        }
    }

    public abstract class Statement : AstNode;
    public class VariableDeclaration(string name, VariableType type, Expression? initializer = null) : Statement
    {
        public string Name { get; } = name;
        public Expression? Initializer { get; } = initializer;

        public VariableType Type { get; } = type;
        public override string DName => $"{nameof(VariableDeclaration)}: {Name} ({Type})";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                if(Initializer is not null) yield return Initializer;
            }
        }
    }
    public class AssignmentStatement(string name, Operations? op, Expression right) : Statement
    {
        public string Name { get; } = name;

        public Operations? Op = op;
        public Expression Right { get; } = right;
        public override string DName => $"{nameof(AssignmentStatement)}: {Name} ({Op})";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                yield return Right;
            }
        }
    }
    public class GotoStatement(string to) : Statement
    {
        public string To { get; } = to;
        public override string DName => $"{nameof(GotoStatement)}: {To}";
        public override IEnumerable<AstNode> Children => [];
    }
    public class GotoDeclaration(string name) : Statement
    {
        public string Name { get; } = name;
        public override string DName => $"{nameof(GotoDeclaration)}: {Name}";
        public override IEnumerable<AstNode> Children => [];
    }
    public class IfDeclaration(Expression expression, List<AstNode> codeBlock) : Statement
    {
        public Expression Expression { get; } = expression;
        public List<AstNode> CodeBlock { get; } = codeBlock;

        public List<ElseIfDeclaration>? ElseIfs { get; set; }
        public ElseDeclaration? Else { get; set; }

        public override string DName => $"{nameof(IfDeclaration)}:";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                yield return Expression;
                foreach(AstNode node in CodeBlock) yield return node;
                foreach (AstNode node in ElseIfs ?? []) yield return node;
                if(Else is not null) yield return Else;
            }
        }

    }
    public class ElseIfDeclaration(Expression expression, List<AstNode> codeBlock) : Statement
    {
        public Expression Expression { get; } = expression;
        public List<AstNode> CodeBlock { get; } = codeBlock;
        public override string DName => $"{nameof(ElseIfDeclaration)}:";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                yield return Expression;
                foreach (AstNode node in CodeBlock) yield return node;
            }
        }
    }
    public class ElseDeclaration : Statement
    {
        public List<AstNode> CodeBlock { get; } = new();
        public override string DName => $"{nameof(ElseDeclaration)}:";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                foreach (AstNode node in CodeBlock ?? []) yield return node;
            }
        }
    }

    public static class AstPrinter
    {
        public static void Print(AstNode node, string indent = "", bool last = true)
        {
            Console.Write(indent);
            Console.Write(last ? "└── " : "├── ");
            Console.WriteLine(node.DName);

            indent += last ? "    " : "│   ";

            var children = node.Children.GetEnumerator();

            if (!children.MoveNext()) return;

            while (true)
            {
                AstNode child = children.Current;
                bool hasNext = children.MoveNext();

                Print(child, indent, !hasNext);

                if (!hasNext) break;
            }
        }
    }

    public enum VariableType
    {
        Int,
        Bool,
        String
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
}
