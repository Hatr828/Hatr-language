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

    public closed class BoundNode
    {
        public virtual string PrintName => GetType().Name;
        public virtual IEnumerable<BoundNode> Children => [];
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
    public class BoundEmptyStatement() : BoundStatement;
    public class BoundExpressionStatement(BoundExpression expression) : BoundStatement
    {
        public BoundExpression Expression { get; } = expression;
        public override IEnumerable<BoundNode> Children => [Expression];
    }
    public class BoundProgram(List<BoundStatement> statements) : BoundNode
    {
        public List<BoundStatement> Statements { get; } = statements;
        public override IEnumerable<BoundNode> Children => Statements;
    }
    public class BoundVariablesDeclaration : BoundStatement
    {
        public List<BoundVariableDeclaration> Variables { get; } = new();
        public override IEnumerable<BoundNode> Children => Variables;
    }
    public class BoundVariableDeclaration(VariableSymbol variable, BoundExpression? initializer) : BoundStatement
    {
        public VariableSymbol Variable { get; } = variable;
        public BoundExpression? Initializer { get; } = initializer;
        public override string PrintName => $"{GetType().Name}: {Variable.Name} ({Variable.Type})";
        public override IEnumerable<BoundNode> Children => Initializer is { } t ? [t] : [];
    }
    public class BoundAssignmentExpression(BoundExpression left, BoundExpression right, Operation? op = null) : BoundExpression(left.Type)
    {
        public Operation? Op { get; } = op;
        public BoundExpression Left { get; } = left;
        public BoundExpression Right { get; } = right;
        public override IEnumerable<BoundNode> Children => [Right, Left];
    }
    public class BoundIfDeclaration(BoundExpression expression) : BoundStatement
    {
        public BoundExpression Expression { get; } = expression;
        public List<BoundStatement> Statements { get; } = new();
        public List<BoundElseIfDeclaration>? ElseIfs { get; set; }
        public BoundElseDeclaration? Else { get; set; }
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
    public class BoundSwitchDeclaration : BoundStatement
    {
        public List<BoundCaseDeclaration> Cases { get; } = new();
        public override IEnumerable<BoundNode> Children => Cases;
    }
    public class BoundCaseDeclaration(BoundExpression? expression, bool isDefault) : BoundStatement
    {
        public bool IsDefault { get; set; } = isDefault;
        public BoundExpression? Expression { get; } = expression;
        public List<BoundStatement> CodeBlock { get; } = new();
        public override IEnumerable<BoundNode> Children => CodeBlock.Concat<BoundNode>(Expression is { } t ? [t] : []);
    }
    public class BoundElseIfDeclaration(BoundExpression expression) : BoundStatement
    {
        public BoundExpression Expression { get; } = expression;
        public List<BoundStatement> CodeBlock { get; } = new();
        public override IEnumerable<BoundNode> Children => CodeBlock.Concat<BoundNode>([Expression]);
    }
    public class BoundElseDeclaration : BoundStatement
    {
        public List<BoundStatement> CodeBlock { get; } = new();
        public override IEnumerable<BoundNode> Children => CodeBlock;
    }
    public class BoundWhileDeclaration(BoundExpression expression, bool isDoWhile = false) : BoundStatement
    {
        public BoundExpression Expression { get; } = expression;
        public List<BoundStatement> CodeBlock { get; } = new();
        public bool IsDoWhile { get; } = isDoWhile;
        public override IEnumerable<BoundNode> Children => CodeBlock.Concat<BoundNode>([Expression]);
    }
    public class BoundForDeclaration(BoundStatement? initializer, BoundExpression? expression, BoundExpression? step) : BoundStatement
    {
        public List<BoundStatement> Statements { get; set; } = new();
        public BoundStatement? Initializer { get; } = initializer;
        public BoundExpression? Expression { get; } = expression;
        public BoundExpression? Step { get; } = step;
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
    public class BoundReturnStatement(BoundExpression? expression = null) : BoundStatement 
    {
        public BoundExpression? Expression { get; } = expression;
        public override IEnumerable<BoundNode> Children => Expression is { } ? [Expression] : [];
    }
    public class BoundBreakStatement : BoundStatement;
    public class BoundContinueStatement : BoundStatement;

    public class BoundGotoDeclaration(int id) : BoundStatement
    {
        public int Id { get; } = id;
        public override string PrintName => $"{GetType().Name}: {Id}";
    }
    public class BoundGotoStatement(int to) : BoundStatement
    {
        public int ToId { get; } = to;
        public override string PrintName => $"{GetType().Name}: {ToId}";
    }
    public class BoundNumberExpression(TypeSymbol type, ulong value) : BoundExpression(type)
    {
        public ulong Value { get; } = value;
        public override string PrintName => $"{GetType().Name}: {Value}";
    }
    public class BoundDoubleExpression(TypeSymbol type, double value) : BoundExpression(type)
    {
        public double Value { get; } = value;
        public override string PrintName => $"{GetType().Name}: {Value}";
    }
    public class BoundArrExpression(List<BoundExpression> right, TypeSymbol type) : BoundExpression(type)
    {
        public List<BoundExpression> Right { get; } = right;
        public override IEnumerable<BoundNode> Children => Right;
    }
    public class BoundConversionExpression(BoundExpression expression, TypeSymbol targetype) : BoundExpression(targetype)
    {
        public BoundExpression Expression { get; } = expression;
        public override IEnumerable<BoundNode> Children => [Expression];
    }
    public class BoundVariableExpression(VariableSymbol variable) : BoundExpression(variable.Type)
    {
        public VariableSymbol Variable { get; } = variable;
        public override string PrintName => $"{GetType().Name}: {Variable.Name} ({Variable.Type})";
    }
    public class BoundMemberAccessExpression(BoundExpression target, FieldSymbol field) : BoundExpression(field.Type)
    {
        public BoundExpression Target { get; } = target;
        public FieldSymbol Field { get; } = field;

        public override string PrintName => $"{GetType().Name}: {Field} ({Target.Type})";
        public override IEnumerable<BoundNode> Children => [Target];
    }
    public class BoundArrayAccessExpression(VariableSymbol variable, BoundExpression idx, TypeSymbol type) : BoundExpression(type)
    {
        public VariableSymbol Variable { get; } = variable;
        public BoundExpression Idx { get; } = idx;

        public override string PrintName => $"{GetType().Name}: {Variable}";
        public override IEnumerable<BoundNode> Children => [Idx];
    }
    public class FieldSymbol(string name, TypeSymbol type, int offset)
    {
        public string Name { get; } = name;
        public TypeSymbol Type { get; } = type;
        public int Offset { get; } = offset;

        public override string ToString() => $"{Name}, {Offset} ({Type})";
    }
    public class BoundBoolExpression(bool value, TypeSymbol type) : BoundExpression(type)
    {
        public bool Value { get; } = value;
        public override string PrintName => $"{GetType().Name}: {Value}";
    }
    public class BoundBinaryExpression(TypeSymbol type, Operation op, BoundExpression left, BoundExpression right) : BoundExpression(type)
    {
        public BoundExpression Left { get; } = left;
        public Operation Op { get; } = op;
        public BoundExpression Right { get; } = right;
        public override string PrintName => $"{GetType().Name}: {Op} ({Type})";
        public override IEnumerable<BoundNode> Children => [Left, Right];
    }
    public class BoundUnaryExpression(TypeSymbol type, Operation op, BoundExpression right) : BoundExpression(type)
    {
        public Operation Op { get; } = op;
        public BoundExpression Right { get; } = right;
        public override string PrintName => $"{GetType().Name}: {Op} ({Type})";
        public override IEnumerable<BoundNode> Children => [Right];
    }
    public class BoundFuncDeclaration(FunctionTypeSymbol type, string name, List<BoundVariableDeclaration> @params, List<BoundStatement> codeBlock) : BoundStatement
    {
        public FunctionTypeSymbol Type { get; } = type;
        public string Name { get; } = name;
        public List<BoundStatement> CodeBlock { get; } = codeBlock;
        public List<BoundVariableDeclaration> Params { get; } = @params;

        public override IEnumerable<BoundNode> Children => Params.Concat<BoundNode>(CodeBlock);
    }
    public class BoundFuncExpression(string name, List<BoundExpression> @params, FunctionTypeSymbol type) : BoundExpression(type.ReturnType)
    {
        public string Name { get; } = name;
        public List<BoundExpression> Params { get; } = @params;

        public override IEnumerable<BoundNode> Children => Params;

    }

    public class VariableSymbol(string name, TypeSymbol type)
    {
        public string Name { get; } = name;
        public TypeSymbol Type { get; } = type;
    }

    public class BoundScopes
    {
        private readonly List<Dictionary<string, VariableSymbol>> _userVariables = new();
        private readonly List<Dictionary<string, (ulong, IntegerTypeSymbol)>> _enums = new();
        private readonly List<Dictionary<string, EnumTypeSymbol>> _userEnums = new();
        private readonly Dictionary<string, FunctionTypeSymbol> _userFunctions = new();
        private readonly List<Dictionary<string, (StructTypeSymbol, List<string>)>> _userStructs = new();

        public bool TryAddVariable(VariableSymbol variable)
        {
            return _userVariables[^1].TryAdd(variable.Name, variable);
        }
        public bool TryAddEnum(string name, ulong val, IntegerTypeSymbol type)
        {
            return _enums[^1].TryAdd(name, (val, type));
        }
        public bool TryAddEnum(EnumTypeSymbol type)
        {
            return _userEnums[^1].TryAdd(type.Name, type);
        }
        public bool TryAddStruct(StructTypeSymbol type, List<string> names)
        {
            return _userStructs[^1].TryAdd(type.Name, (type, names));
        }
        public bool TryAddFunction(string name, FunctionTypeSymbol type)
        {
            return _userFunctions.TryAdd(name, type);
        }

        public bool TryGetVariable(string name, out VariableSymbol variable)
        {
            for (int i = 1; i <= _userVariables.Count; i++)
            {
                if (_userVariables[^i].TryGetValue(name, out variable!))
                {
                    return true;
                }
            }
            variable = null!;
            return false;
        }
        public bool TryGetEnumValue(string name, out (ulong, IntegerTypeSymbol) val)
        {
            for (int i = 1; i <= _enums.Count; i++)
            {
                if (_enums[^i].TryGetValue(name, out val!))
                {
                    return true;
                }
            }
            val = (ulong.MaxValue, null!);
            return false;
        }
        public bool TryGetEnum(string name, out EnumTypeSymbol val)
        {
            for (int i = 1; i <= _userEnums.Count; i++)
            {
                if (_userEnums[^i].TryGetValue(name, out val!))
                {
                    return true;
                }
            }
            val = null!;
            return false;
        }
        public bool TryGetStruct(string name, out (StructTypeSymbol, List<string>) val)
        {
            for (int i = 1; i <= _userStructs.Count; i++)
            {
                if (_userStructs[^i].TryGetValue(name, out val!))
                {
                    return true;
                }
            }
            val = (null!, null!);
            return false;
        }
        public bool TryGetFunction(string name, out FunctionTypeSymbol type) =>  _userFunctions.TryGetValue(name, out type!);

        public void AddScope()
        {
            _userVariables.Add(new());
            _enums.Add(new());
            _userEnums.Add(new());
            _userStructs.Add(new());
        }
        public void RemoveScope()
        {
            _userVariables.RemoveAt(_userVariables.Count - 1);
            _enums.RemoveAt(_enums.Count - 1);
            _userEnums.RemoveAt(_userEnums.Count - 1);
            _userStructs.RemoveAt(_userStructs.Count - 1);
        }
    }
    public static class BoundTreePrinter
    {
        public static void Print(BoundNode node, string indent = "", bool last = true)
        {
            Console.Write(indent);
            Console.Write(last ? "└── " : "├── ");
            Console.WriteLine(node.PrintName);

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
