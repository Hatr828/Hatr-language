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
    public class BoundEmptyStatement() : BoundStatement
    {
        public override string DName => $"{nameof(BoundEmptyStatement)}";
        public override IEnumerable<BoundNode> Children => [];
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
    public class BoundVariablesDeclaration : BoundStatement
    {
        public List<BoundVariableDeclaration> variables { get; } = new();
        public override string DName => $"{nameof(BoundVariablesDeclaration)}";
        public override IEnumerable<BoundNode> Children
        {
            get                                                          
            {
                foreach(var variable in variables) yield return variable;
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
    public class BoundAssignmentStatement(BoundExpression left, BoundExpression right) : BoundStatement
    {
        public BoundExpression Left { get; } = left;
        public BoundExpression Right { get; } = right;
        public override string DName => $"{nameof(BoundAssignmentStatement)}";
        public override IEnumerable<BoundNode> Children => [Right, Left];
    }
    public class BoundAssignmentMemberStatement(BoundExpression target, FieldSymbol field, BoundExpression right) : BoundStatement
    {
        public BoundExpression Target { get; } = target;
        public FieldSymbol Field { get; } = field;
        public BoundExpression Right { get; } = right;
        public override string DName => $"{nameof(BoundAssignmentMemberStatement)}: {Field} ({Target.Type})";
        public override IEnumerable<BoundNode> Children => [Target, Right];
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
    public class BoundSwitchDeclaration : BoundStatement
    {
        public List<BoundCaseDeclaration> Cases { get; } = new();
        public override string DName => $"{nameof(SwitchDeclaration)}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                foreach (BoundCaseDeclaration node in Cases) yield return node;
            }
        }
    }
    public class BoundCaseDeclaration(BoundExpression? expression, bool isDefault) : BoundStatement
    {
        public bool IsDefault { get; set; } = isDefault;
        public BoundExpression? Expression { get; } = expression;
        public List<BoundStatement> Statements { get; } = new();
        public override string DName => $"{nameof(CaseDeclaration)}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                if (Expression is not null) yield return Expression;
                foreach (BoundNode node in Statements) yield return node;
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
    public class BoundReturnStatement(BoundExpression? expression = null) : BoundStatement 
    {
        public BoundExpression? Expression { get; } = expression;
        public override string DName => $"{nameof(BoundReturnStatement)}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                if (Expression is not null) yield return Expression;
            }
        }
    }
    public class BoundBreakStatement : BoundStatement
    {
        public override string DName => $"{nameof(BoundBreakStatement)}";
        public override IEnumerable<BoundNode> Children => [];
    }
    public class BoundContinueStatement : BoundStatement
    {
        public override string DName => $"{nameof(BoundContinueStatement)}";
        public override IEnumerable<BoundNode> Children => [];
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
    public class BoundArrExpression(List<BoundExpression> right, TypeSymbol type) : BoundExpression(type)
    {
        public List<BoundExpression> Right { get; } = right;

        public override string DName => $"{nameof(BoundArrExpression)}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                foreach (var ex in Right) yield return ex;
            }
        }
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
    public class BoundMemberAccessExpression(BoundExpression target, FieldSymbol field) : BoundExpression(field.Type)
    {
        public BoundExpression Target { get; } = target;
        public FieldSymbol Field { get; } = field;

        public override string DName => $"{nameof(BoundMemberAccessExpression)}: {Field} ({Target.Type})";
        public override IEnumerable<BoundNode> Children => [Target];
    }
    public class BoundArrayAccessExpression(VariableSymbol variable, BoundExpression idx, TypeSymbol type) : BoundExpression(type)
    {
        public VariableSymbol Variable { get; } = variable;
        public BoundExpression Idx { get; } = idx;

        public override string DName => $"{nameof(BoundArrayAccessExpression)}: {Variable}";
        public override IEnumerable<BoundNode> Children => [Idx];
    }
    public class BoundArrayAssignment(BoundExpression left, BoundExpression idx, BoundExpression right) : BoundStatement
    {
        public BoundExpression Left { get; } = left;
        public BoundExpression Idx { get; } = idx;

        public BoundExpression Right { get; } = right;

        public override string DName => $"{nameof(BoundArrayAssignment)}";
        public override IEnumerable<BoundNode> Children => [Idx, Right, Left];
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
    public class BoundFuncDeclaration(FunctionTypeSymbol type, string name, List<BoundVariableDeclaration> @params, List<BoundStatement> codeBlock) : BoundStatement
    {
        public FunctionTypeSymbol Type { get; } = type;
        public string Name { get; } = name;
        public List<BoundStatement> CodeBlock { get; } = codeBlock;
        public List<BoundVariableDeclaration> Params { get; } = @params;

        public override string DName => $"{GetType().Name}:";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                foreach (BoundNode node in Params) yield return node;
                foreach (BoundNode node in CodeBlock) yield return node;
            }
        }

    }
    public class BoundFuncExpression(string name, List<BoundExpression> @params, FunctionTypeSymbol type) : BoundExpression(type.ReturnType)
    {
        public string Name { get; } = name;
        public List<BoundExpression> Params { get; } = @params;

        public override string DName => $"{GetType().Name}:";
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
