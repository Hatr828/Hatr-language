using Best_language.src.front;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Best_language.src.middle
{
    public abstract class BoundNode
    {
        public abstract string DName { get; }
        public abstract IEnumerable<BoundNode> Children { get; }
    }
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
        public override string DName => $"{nameof(BoundProgram)}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                foreach (BoundNode node in Functions) yield return node;
            }       
        }
    }
    public class BoundFunction(VariableType type, string name, List<BoundStatement> statements) : BoundStatement
    {
        public VariableType Type { get; } = type;
        public string Name { get; } = name;
        public List<BoundStatement> Statements { get; } = statements;
        public override string DName => $"{nameof(BoundFunction)}: {Name} ({Type})";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                foreach (BoundNode node in Statements) yield return node;
            }
        }
    }
    public class BoundVariableDeclaration(VariableType type, string name, BoundExpression initializer) : BoundStatement
    {
        public VariableType Type { get; } = type;
        public string Name { get; } = name;
        public BoundExpression Initializer { get; } = initializer;
        public override string DName => $"{nameof(BoundVariableDeclaration)}: {Name} ({Type})";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Initializer;
            }
        }
    }
    public class BoundAssignmentStatement(string name, BoundExpression right) : BoundStatement
    {
        public string Name { get; } = name;
        public BoundExpression Right { get; } = right;
        public override string DName => $"{nameof(BoundAssignmentStatement)}: {Name}";
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
                if(Else is not null) yield return Else;
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
    public class BoundNumberExpression(VariableType type, long value) : BoundExpression(type)
    {
        public long Value { get; } = value;
        public override string DName => $"{nameof(BoundNumberExpression)}: {Value}";
        public override IEnumerable<BoundNode> Children => [];
    }
    public class BoundVariableExpression(VariableType type, string name) : BoundExpression(type)
    {
        public string Name { get; } = name;
        public override string DName => $"{nameof(BoundVariableExpression)}: {Name}";
        public override IEnumerable<BoundNode> Children => [];
    }
    public class BoundBoolExpression(bool value) : BoundExpression(VariableType.Bool)
    {
        public bool Value { get; } = value;
        public override string DName => $"{nameof(BoundBoolExpression)}: {Value}";
        public override IEnumerable<BoundNode> Children => [];
    }
    public class BoundBinaryExpression(VariableType type, Operations op, BoundExpression left, BoundExpression right) : BoundExpression(type)
    {
        public BoundExpression Left { get; } = left;
        public Operations Op { get; } = op;
        public BoundExpression Right { get; } = right;
        public override string DName => $"{nameof(BoundBinaryExpression)}: {Op}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Left;
                yield return Right;
            }
        }
    }
    public class BoundUnaryExpression(VariableType type, Operations op, BoundExpression right) : BoundExpression(type)
    {
        public Operations Op { get; } = op;
        public BoundExpression Right { get; } = right;
        public override string DName => $"{nameof(BoundUnaryExpression)}: {Op}";
        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Right;
            }
        }
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
