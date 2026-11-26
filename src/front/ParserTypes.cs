using Best_language.src.middle;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Xml.Linq;

namespace Best_language.src.front
{
    public closed class AstNode
    {
        public abstract string DName { get; }
        public abstract IEnumerable<AstNode> Children { get; }
    }
    public class EmptyNode : AstNode
    {
        public override string DName => $"{nameof(EmptyNode)}";
        public override IEnumerable<AstNode> Children => [];
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
    public class MemberAccessExpression(Expression target, string memberName) : Expression
    {
        public Expression Target { get; } = target;
        public string Name { get; } = memberName;

        public override string DName => $"{nameof(MemberAccessExpression)}: target.{Name}";
        public override IEnumerable<AstNode> Children => [Target];
    }
    public class ArrayAccessExpression(string target, Expression idx) : Expression
    {
        public string Target { get; } = target;
        public Expression Idx { get; } = idx;

        public override string DName => $"{nameof(ArrayAccessExpression)}: {Target}";
        public override IEnumerable<AstNode> Children => [Idx];
    }
    public class MemberAssignment(Expression target, string memberName, Operation? op, Expression initializer) : Expression
    {
        public Expression Target { get; } = target;
        public string Name { get; } = memberName;
        public Operation? Op { get; } = op;
        public Expression Initializer { get; } = initializer;

        public override string DName => $"{nameof(MemberAssignment)}: target.{Name} {Op}";
        public override IEnumerable<AstNode> Children => [Target, Initializer];
    }
    public class ArrayAssignment(Expression left, Expression idx, Operation? op, Expression right) : Expression
    {
        public Expression Left { get; } = left;
        public Expression Idx { get; } = idx;

        public Operation? Op = op;
        public Expression Right { get; } = right;

        public override string DName => $"{nameof(ArrayAssignment)}: {Op}";
        public override IEnumerable<AstNode> Children => [Idx, Right, Left];
    }
    public class BoolExpression(bool value, TypeSymbol type) : Expression
    {
        public bool Value { get; } = value;
        public TypeSymbol Type { get; } = type;
        public override string DName => $"{nameof(BoolExpression)}: {Value}";
        public override IEnumerable<AstNode> Children => [];
    }
    public class BinaryExpression(Expression left, Operation op, Expression right) : Expression
    {
        public Expression Left { get; } = left;
        public Operation Op { get; } = op;
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
    public class UnaryExpression(Operation op, Expression right) : Expression
    {
        public Operation Op { get; } = op;
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
    public class EnumDeclaration(EnumTypeSymbol type, List<VariableDeclaration> variables) : Statement
    {
        public EnumTypeSymbol Type { get; } = type;
        public List<VariableDeclaration> Variables { get; } = variables;
        public override string DName => $"{nameof(EnumDeclaration)} ({Type})";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                foreach (var variable in Variables) yield return variable;
            }
        }
    }
    public class StructDeclaration(StructTypeSymbol type, List<string> names) : Statement
    {
        public StructTypeSymbol Type { get; } = type;
        public List<string> Names { get; } = names;
        public List<VariableDeclaration>? Variables { get; set; }
        public override string DName => $"{nameof(StructDeclaration)} ({Type})";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                if (Variables is not null)
                    foreach (var variable in Variables) yield return variable;
            }
        }
    }
    public class VariableDeclarations : Statement
    {
        public List<VariableDeclaration> Variables { get; set; } = new();
        public override string DName => $"{nameof(VariableDeclarations)}";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                 foreach(var variable in Variables) yield return variable;
            }
        }
    }
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
    public class AssignmentStatement(Expression left, Operation? op, Expression right) : Statement
    {
        public Expression Left { get; } = left;
        public Operation? Op = op;
        public Expression Right { get; } = right;
        public override string DName => $"{nameof(AssignmentStatement)}: ({Op})";
        public override IEnumerable<AstNode> Children => [Left, Right];
    }

    public class ArrayExpression(List<Expression> right) : Expression
    {
        public List<Expression> Right { get; } = right;

        public override string DName => $"{nameof(ArrayExpression)}";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                foreach (var ex in Right) yield return ex;
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
    public class BreakStatement() : Statement
    {
        public override string DName => $"{nameof(BreakStatement)}";
        public override IEnumerable<AstNode> Children => [];
    }
    public class ContinueStatement() : Statement
    {
        public override string DName => $"{nameof(ContinueStatement)}";
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
                foreach (AstNode node in CodeBlock) yield return node;
                foreach (AstNode node in ElseIfs ?? []) yield return node;
                if (Else is not null) yield return Else;
            }
        }

    }
    public class SwitchDeclaration(Expression expression, List<CaseDeclaration> cases) : Statement
    {
        public Expression Expression { get; } = expression;
        public List<CaseDeclaration> Cases { get; } = cases;
        public override string DName => $"{nameof(SwitchDeclaration)}:";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                yield return Expression;
                foreach (AstNode node in Cases) yield return node;
            }
        }

    }
    public class CaseDeclaration(Expression? expression, List<AstNode> codeBlock, bool isDefault = false) : Statement
    {
        public bool IsDefault { get; set; } = isDefault;
        public Expression? Expression { get; } = expression;
        public List<AstNode> Statements { get; } = codeBlock;
        public override string DName => $"{nameof(CaseDeclaration)}:";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                if (Expression is not null) yield return Expression;
                foreach (AstNode node in Statements) yield return node;
            }
        }
    }
    public class FuncDeclaration(FunctionTypeSymbol type, string name, List<VariableDeclaration> @params, List<AstNode> codeBlock) : Statement
    {
        public FunctionTypeSymbol Type { get; } = type;
        public string Name { get; } = name;
        public List<AstNode> CodeBlock { get; } = codeBlock;
        public List<VariableDeclaration> Params { get; } = @params;

        public override string DName => $"{GetType().Name}:";
        public override IEnumerable<AstNode> Children
        {
            get
            {
                foreach (AstNode node in Params) yield return node;
                foreach (AstNode node in CodeBlock) yield return node;
            }
        }

    }
    public class FuncExpression(string name, List<Expression> @params) : Expression
    {
        public string Name { get; } = name;
        public List<Expression> Params { get; } = @params;

        public override string DName => $"{GetType().Name}:";
        public override IEnumerable<AstNode> Children => Params;

    }
    public class ElseIfDeclaration(Expression expression, List<AstNode> codeBlock) : Statement
    {
        public Expression Expression { get; } = expression;
        public List<AstNode> CodeBlock { get; } = codeBlock;
        public override string DName => $"{GetType().Name}:";
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
        public override string DName => $"{GetType().Name}:";
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
        public override string DName => $"{GetType().Name}:";
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
        public override string DName => $"{GetType().Name}:";
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

    public class ParserScopes
    {
        private readonly List<Dictionary<string, NameKind>> _nameKinds = new();
        private List<Dictionary<string, TypeSymbol>> _typedefs = new();
        private List<Dictionary<string, EnumTypeSymbol>> _enums = new();
        private List<Dictionary<string, (ulong, TypeSymbol)>> _enumNames = new();
        public bool TryAddVariable(string name, NameKind kind)
        {
            return _nameKinds[^1].TryAdd(name, kind);
        }
        public bool TryAddTypedef(string name, TypeSymbol kind)
        {
            return _typedefs[^1].TryAdd(name, kind);
        }
        public bool TryAddEnum(string name, EnumTypeSymbol type)
        {
            return _enums[^1].TryAdd(name, type);
        }
        public bool TryAddEnumMember(string name, (ulong, TypeSymbol) val)
        {
            return _enumNames[^1].TryAdd(name, val);
        }

        public bool TryGetVariable(string name, out NameKind type)
        {
            for (int i = 1; i <= _nameKinds.Count; i++)
            {
                if (_nameKinds[^i].TryGetValue(name, out type))
                {
                    return true;
                }
            }
            type = (NameKind)(-1);
            return false;
        }
        public bool TryGetTypedef(string name, out TypeSymbol type)
        {
            for (int i = 1; i <= _typedefs.Count; i++)
            {
                if (_typedefs[^i].TryGetValue(name, out type!))
                {
                    return true;
                }
            }
            type = null!;
            return false;
        }
        public bool TryGetEnum(string name, out EnumTypeSymbol type)
        {
            for (int i = 1; i <= _enums.Count; i++)
            {
                if (_enums[^i].TryGetValue(name, out type!))
                {
                    return true;
                }
            }
            type = null!;
            return false;
        }
        public bool TryGetEnumMember(string name, out (ulong, TypeSymbol) val)
        {
            for (int i = 1; i <= _enumNames.Count; i++)
            {
                if (_enumNames[^i].TryGetValue(name, out val))
                {
                    return true;
                }
            }
            val = (ulong.MaxValue, null!);
            return false;
        }

        public void AddScope()
        {
            _nameKinds.Add(new());
            _typedefs.Add(new());
            _enums.Add(new());
            _enumNames.Add(new());
        }
        public void RemoveScope()
        {
            _nameKinds.RemoveAt(_nameKinds.Count - 1);
            _typedefs.RemoveAt(_typedefs.Count - 1);
            _enums.RemoveAt(_enums.Count - 1);
            _enumNames.RemoveAt(_enumNames.Count - 1);
        }
        public bool IsEmpty() => _nameKinds.Count == 0;
    }

    public enum NameKind
    {
        TypeDef,
        Variable,
        Enum,
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

    public enum Operation
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
        Dereference,
        ReferenceOf,
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
        EqualsEquals,
        NotEquals,
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
