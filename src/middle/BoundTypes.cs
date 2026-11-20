using Best_language.src.front;

namespace Best_language.src.middle
{
    public enum BinaryRule : byte
    {
        Arithmetic,
        IntegerArithmetic,
        Shift,
        Logical,
        ArithmeticComparison,

        PointerComparison,
        PointerArithmetic,
    }

    public enum UnaryRule : byte
    {
        Logical,
        PlusOrMinus,
        Arithmetic,
        ReferenceOf,
        Dereference,
    }

    public abstract class BoundNode
    {
        public abstract string DName { get; }
        public abstract IEnumerable<BoundNode> Children { get; }
    }
    public abstract class BoundStatement : BoundNode;
    public abstract class BoundExpression(TypeSymbol type) : BoundNode
    {
        public TypeSymbol Type { get; } = type;
    }
    public class BoundVariable(VariableSymbol variable)
    {
        public VariableSymbol Variable { get; } = variable;
    }
    public class BoundExpressionStatement(BoundExpression expression) : BoundStatement
    {
        public BoundExpression Expression { get; } = expression;
        public override string DName => $"{nameof(BoundExpressionStatement)}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Expression;
            }
        }
    }
    public class BoundProgram(List<BoundStatement> statements) : BoundNode
    {
        public List<BoundStatement> Statements { get; } = statements;
        public override string DName => $"{nameof(BoundProgram)}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                foreach (BoundNode node in Statements) yield return node;
            }
        }
    }
    public class BoundParamsDeclaration(TypeSymbol type, string name, BoundExpression? initializer = null) : BoundStatement
    {
        public TypeSymbol Type { get; } = type;
        public string Name { get; } = name;
        public BoundExpression? Initializer { get; } = initializer;
        public override string DName => $"{nameof(BoundParamsDeclaration)}: {Name} ({Type})";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                if (Initializer is not null) yield return Initializer;
            }
        }
    }
    public class BoundFunction(TypeSymbol type, string name, List<BoundParamsDeclaration> @params,  List<BoundStatement> statements) : BoundStatement
    {
        public TypeSymbol Type { get; } = type;
        public string Name { get; } = name;
        public List<BoundStatement> Statements { get; } = statements;
        public List<BoundParamsDeclaration> Params { get; } = @params;
        public override string DName => $"{nameof(BoundFunction)}: {Name} ({Type})";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                foreach (BoundNode node in Params) yield return node;
                foreach (BoundNode node in Statements) yield return node;
            }
        }
    }
    public class BoundVariableDeclaration(VariableSymbol variable, BoundExpression? initializer) : BoundStatement
    {
        public VariableSymbol Variable { get; } = variable;
        public BoundExpression? Initializer { get; } = initializer;
        public override string DName => $"{nameof(BoundVariableDeclaration)}: {Variable.Name} ({Variable.Type})";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                if (Initializer is not null) yield return Initializer;
            }
        }
    }
    public class BoundAssignmentStatement(VariableSymbol variable, BoundExpression right) : BoundStatement
    {
        public VariableSymbol Variable { get; } = variable;
        public BoundExpression Right { get; } = right;
        public override string DName => $"{nameof(BoundAssignmentStatement)}: {Variable.Name}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Right;
            }
        }
    }
    public class BoundIfDeclaration(BoundExpression expression) : BoundStatement
    {
        public BoundExpression Expression { get; } = expression;
        public List<BoundStatement> Statements { get; } = new();
        public List<BoundElseIfDeclaration>? ElseIfs { get; set; }
        public BoundElseDeclaration? Else { get; set; }
        public override string DName => $"{nameof(BoundIfDeclaration)}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Expression;
                foreach (BoundNode node in Statements) yield return node;
                foreach (BoundNode node in ElseIfs ?? []) yield return node;
                if (Else is not null) yield return Else;
            }
        }
    }
    public class BoundElseIfDeclaration(BoundExpression expression) : BoundStatement
    {
        public BoundExpression Expression { get; } = expression;
        public List<BoundStatement> Statements { get; } = new();
        public override string DName => $"{nameof(BoundElseIfDeclaration)}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Expression;
                foreach (BoundNode node in Statements) yield return node;
            }
        }
    }
    public class BoundElseDeclaration : BoundStatement
    {
        public List<BoundStatement> Statements { get; } = new();
        public override string DName => $"{nameof(BoundElseDeclaration)}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                foreach (BoundNode node in Statements ?? []) yield return node;
            }
        }
    }
    public class BoundWhileDeclaration(BoundExpression expression, bool isDoWhile = false) : BoundStatement
    {
        public BoundExpression Expression { get; } = expression;
        public List<BoundStatement> Statements { get; } = new();
        public bool IsDoWhile { get; } = isDoWhile;
        public override string DName => $"{nameof(BoundElseDeclaration)}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Expression;
                foreach (BoundNode node in Statements ?? []) yield return node;
            }
        }
    }
    public class BoundForDeclaration(BoundStatement? initializer, BoundExpression? expression, BoundExpression? step) : BoundStatement
    {
        public List<BoundStatement> Statements { get; set; } = new();
        public BoundStatement? Initializer { get; } = initializer;
        public BoundExpression? Expression { get; } = expression;
        public BoundExpression? Step { get; } = step;
        public override string DName => $"{nameof(ForDeclaration)}:";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                if (Initializer is not null) yield return Initializer;
                if (Expression is not null) yield return Expression;
                if (Step is not null) yield return Step;
                foreach (BoundStatement node in Statements ?? []) yield return node;
            }
        }
    }
    public class BoundReturnStatement(BoundExpression expression) : BoundStatement 
    {
        public BoundExpression Expression { get; } = expression;
        public override string DName => $"{nameof(BoundReturnStatement)}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Expression;
            }
        }
    }
    public class BoundGotoDeclaration(int id) : BoundStatement
    {
        public int Id { get; } = id;
        public override string DName => $"{nameof(BoundGotoDeclaration)}: {Id}";
        public override IEnumerable<BoundNode> Children => [];
    }
    public class BoundGotoStatement(int to) : BoundStatement
    {
        public int ToId { get; } = to;
        public override string DName => $"{nameof(BoundGotoStatement)}: {ToId}";
        public override IEnumerable<BoundNode> Children => [];
    }
    public class BoundNumberExpression(TypeSymbol type, ulong value) : BoundExpression(type)
    {
        public ulong Value { get; } = value;
        public override string DName => $"{nameof(BoundNumberExpression)}: {Value}";
        public override IEnumerable<BoundNode> Children => [];
    }
    public class BoundDoubleExpression(TypeSymbol type, double value) : BoundExpression(type)
    {
        public double Value { get; } = value;
        public override string DName => $"{nameof(BoundDoubleExpression)}: {Value}";
        public override IEnumerable<BoundNode> Children => [];
    }
    public class BoundConversionExpression(BoundExpression expression, TypeSymbol targetype) : BoundExpression(targetype)
    {
        public BoundExpression Expression { get; } = expression;
        public override string DName => $"{nameof(BoundConversionExpression)}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Expression;
            }
        }
    }
    public class BoundVariableExpression(VariableSymbol variable) : BoundExpression(variable.Type)
    {
        public VariableSymbol Variable { get; } = variable;
        public override string DName => $"{nameof(BoundVariableExpression)}: {Variable.Name} ({Variable.Type})";
        public override IEnumerable<BoundNode> Children => [];
    }
    public class BoundBoolExpression(bool value, TypeSymbol type) : BoundExpression(type)
    {
        public bool Value { get; } = value;
        public override string DName => $"{nameof(BoundBoolExpression)}: {Value}";
        public override IEnumerable<BoundNode> Children => [];
    }
    public class BoundBinaryExpression(TypeSymbol type, Operation op, BoundExpression left, BoundExpression right) : BoundExpression(type)
    {
        public BoundExpression Left { get; } = left;
        public Operation Op { get; } = op;
        public BoundExpression Right { get; } = right;
        public override string DName => $"{nameof(BoundBinaryExpression)}: {Op} ({Type})";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Left;
                yield return Right;
            }
        }
    }
    public class BoundUnaryExpression(TypeSymbol type, Operation op, BoundExpression right) : BoundExpression(type)
    {
        public Operation Op { get; } = op;
        public BoundExpression Right { get; } = right;
        public override string DName => $"{nameof(BoundUnaryExpression)}: {Op} ({Type})";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                if (Right is not null) yield return Right;
            }
        }
    }

    public class VariableSymbol(string name, TypeSymbol type)
    {
        public string Name { get; } = name;
        public TypeSymbol Type { get; } = type;
    }

    public class BoundScopes
    {
        private readonly List<Dictionary<string, VariableSymbol>> _variables = new();

        public bool TryAddVariable(VariableSymbol variable)
        {
            return _variables[^1].TryAdd(variable.Name, variable);
        }
        public bool DoesExistInSameScope(string name)
        {
            if (_variables[^1].TryGetValue(name, out var variable))
            {
                return true;
            }
            return false;
        }
        public bool Lookup(string name, out TypeSymbol type)
        {
            for (int i = 1; i <= _variables.Count; i++)
            {
                if (_variables[^i].TryGetValue(name, out var variable))
                {
                    type = variable.Type;
                    return true;
                }
            }
            type = new VoidTypeSymbol();
            return false;
        }
        public bool TryGetVariable(string name, out VariableSymbol variable)
        {
            for (int i = 1; i <= _variables.Count; i++)
            {
                if (_variables[^i].TryGetValue(name, out variable!))
                {
                    return true;
                }
            }
            variable = null!;
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
        public static void Print(BoundNode node, string indent = "", bool last = true)
        {
            Console.Write(indent);
            Console.Write(last ? "└── " : "├── ");
            Console.WriteLine(node.DName);

            indent += last ? "    " : "│   ";

            var children = node.Children.GetEnumerator();

            if (!children.MoveNext()) return;

            while (true)
            {
                BoundNode child = children.Current;
                bool hasNext = children.MoveNext();

                Print(child, indent, !hasNext);

                if (!hasNext) break;
            }
        }
    }
}
