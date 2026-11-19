using System.Numerics;
using System.Runtime.CompilerServices;

namespace Best_language.src.front
{
    public closed class AstNode
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
    public class NumberExpression(ulong value, TypeSymbol type) : Expression
    {
        public ulong Value { get; } = value;
        public TypeSymbol Type { get; } = type;
        public override string DName => $"{nameof(NumberExpression)}: {Value} ({Type})";
        public override IEnumerable<AstNode> Children => [];
    }
    public class DoubleExpression(double value, TypeSymbol type) : Expression
    {
        public double Value { get; } = value;
        public TypeSymbol Type { get; } = type;
        public override string DName => $"{nameof(DoubleExpression)}: {Value} ({Type})";
        public override IEnumerable<AstNode> Children => [];
    }
    public class VariableExpression(string name) : Expression
    {
        public string Name { get; } = name;
        public override string DName => $"{nameof(VariableExpression)}: {Name}";
        public override IEnumerable<AstNode> Children => [];
    }
    public class BoolExpression(bool value, TypeSymbol type) : Expression
    {
        public bool Value { get; } = value;
        public TypeSymbol Type { get; } = type;
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
    public class VariableDeclaration(string name, TypeSymbol type, Expression? initializer = null) : Statement
    {
        public string Name { get; } = name;
        public Expression? Initializer { get; } = initializer;

        public TypeSymbol Type { get; } = type;
        public override string DName => $"{nameof(VariableDeclaration)}: {Name} ({Type})";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                if (Initializer is not null) yield return Initializer;
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
                if (Right is not null) yield return Right;
            }
        }
    }
    public class ExpressionStatement(Expression right) : Statement
    {
        public Expression Right { get; } = right;
        public override string DName => $"{nameof(ExpressionStatement)}";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                if (Right is not null) yield return Right;
            }
        }
    }
    public class GotoStatement(string to) : Statement
    {
        public string To { get; } = to;
        public override string DName => $"{nameof(GotoStatement)}: {To}";
        public override IEnumerable<AstNode> Children => [];
    }
    public class ReturnStatement(Expression? expression = null) : Statement
    {
        public Expression? Expression { get; } = expression;
        public override string DName => $"{nameof(ReturnStatement)}";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                if (Expression is not null) yield return Expression;
            }
        }
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
                foreach (AstNode node in CodeBlock) yield return node;
                foreach (AstNode node in ElseIfs ?? []) yield return node;
                if (Else is not null) yield return Else;
            }
        }

    }
    public class ParamDeclaration(string name, TypeSymbol type, Expression? initializer) : Statement
    {
        public string Name { get; } = name;
        public Expression? Initializer { get; } = initializer;
        public TypeSymbol Type { get; } = type;

        public override string DName => $"{nameof(ParamDeclaration)}: {Name} ({Type})";
        public override IEnumerable<AstNode> Children => [];
    }
    public class FuncDeclaration(TypeSymbol type, string name, List<ParamDeclaration> @params, List<AstNode> codeBlock) : Statement
    {
        public TypeSymbol ReturnType { get; } = type;
        public string Name { get; } = name;
        public List<AstNode> CodeBlock { get; } = codeBlock;
        public List<ParamDeclaration> Params { get; } = @params;

        public override string DName => $"{nameof(IfDeclaration)}:";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                foreach (AstNode node in Params) yield return node;
                foreach (AstNode node in CodeBlock) yield return node;
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
    public class WhileDeclaration(Expression expression, List<AstNode> codeBlock, bool isDoWhile = false) : Statement
    {
        public List<AstNode> CodeBlock { get; } = codeBlock;
        public Expression Expression { get; } = expression;
        public bool IsDoWhile { get; } = isDoWhile;
        public override string DName => $"{nameof(WhileDeclaration)}:";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                yield return Expression;
                foreach (AstNode node in CodeBlock ?? []) yield return node;
            }
        }
    }
    public class ForDeclaration(AstNode? initializer, Expression? expression, Expression? step, List<AstNode> codeBlock) : Statement
    {
        public List<AstNode> CodeBlock { get; } = codeBlock;
        public AstNode? Initializer { get; } = initializer;
        public Expression? Expression { get; } = expression;
        public Expression? Step { get; } = step;
        public override string DName => $"{nameof(ForDeclaration)}:";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                if (Initializer is not null) yield return Initializer;
                if (Expression is not null) yield return Expression;
                if (Step is not null) yield return Step;
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

    public enum Operations
    {
        // do not move
        PlusEquals,
        MinusEquals,
        MultiplyEquals,
        BitAndEquals,
        BitOrEquals,
        BitXorEquals,
        Plus,
        Minus,
        Multiply,
        BitXor,
        BitAnd,
        BitOr,

        DivideEquals,
        Divide,
        //
        DivisionRemEquals,
        DivisionRem,

        PlusPlusRight,
        PlusPlusLeft,
        MinusMinusRight,
        MinusMinusLeft,

        // do not move
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        NotEquals,
        EqualsEquals,
        //

        BitNot,

        RightShift,
        LeftShift,
        RightShiftEquals,
        LeftShiftEquals,

        And,
        Or,
        Not
    }
}
