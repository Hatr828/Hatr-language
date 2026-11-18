using Best_language.src.front;
using System.ComponentModel;

namespace Best_language.src.middle
{
    public class BoundTree
    {
        private BoundScopes scopes = new();
        private Dictionary<string, int> _gotoLabels = new();
        private TypeTable _typeTable = new TypeTable();
        private int _labelId;

        public BoundProgram BindProgram(ProgramNode mainNode, TypeTable typeTable)
        {
            _typeTable = typeTable;

            PreProcessAst(mainNode.Nodes);

            List<BoundStatement> statements = new();
            scopes.AddScope();

            foreach (AstNode node in mainNode.Nodes)
            {
                statements.Add(BindStatement(node));
            }
            return new BoundProgram(statements);
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
                        {
                            var initializer = BindExpression(variable.Initializer);
                            temp = new BoundVariableDeclaration(variable.Type, variable.Name, BindConversion(initializer, variable.Type));
                        }
                        else
                            temp = new BoundVariableDeclaration(variable.Type, variable.Name, null);

                        if (!scopes.TryAddVariable(new(variable.Type, variable.Name))) throw new Exception($"Error in TryAddVariable");
                        return temp;
                    }
                case IfDeclaration ifDeclaration:
                    {
                        var If = new BoundIfDeclaration(BindExpression(ifDeclaration.Expression));
                        scopes.AddScope();
                        foreach (var statement in ifDeclaration.CodeBlock) If.Statements.Add(BindStatement(statement));
                        scopes.RemoveScope();

                        if (ifDeclaration.ElseIfs is not null)
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
                case WhileDeclaration whileDeclaration:
                    {
                        var @while = new BoundWhileDeclaration(BindExpression(whileDeclaration.Expression));
                        scopes.AddScope();
                        foreach (var statement in whileDeclaration.CodeBlock) @while.Statements.Add(BindStatement(statement));
                        scopes.RemoveScope();
                        return @while;
                    }
                case FuncDeclaration func:
                    {
                        List<BoundStatement> codeBlock = new();
                        List<BoundParamsDeclaration> @params = new();
                        scopes.AddScope();
                        foreach (var param in func.Params)
                        {
                            if(param.Initializer is not null)
                                @params.Add(new(param.Type, param.Name, BindExpression(param.Initializer)));
                            else
                                @params.Add(new(param.Type, param.Name));

                            if (!scopes.TryAddVariable(new(param.Type, param.Name))) throw new ArgumentException($"variable already exist: {param.Name}");
                        }
                        foreach (var statement in func.CodeBlock) codeBlock.Add(BindStatement(statement));
                        scopes.RemoveScope();
                        return new BoundFunction(func.ReturnType, func.Name, @params, codeBlock);
                    }                             
                case ForDeclaration forDeclaration:
                    {
                        scopes.AddScope();
                        BoundStatement? initializer = null;
                        BoundExpression? step = null, ex = null;

                        if (forDeclaration.Initializer is not null) initializer = BindStatement(forDeclaration.Initializer);
                        if (forDeclaration.Step is not null) step = BindExpression(forDeclaration.Step);
                        if (forDeclaration.Expression is not null) ex = BindExpression(forDeclaration.Expression);

                        BoundForDeclaration @for = new(initializer, ex, step);

                        foreach (var statement in forDeclaration.CodeBlock) @for.Statements.Add(BindStatement(statement));

                        scopes.RemoveScope();
                        return @for;
                    }
                case GotoDeclaration @goto:
                    {
                        if (_gotoLabels.TryGetValue(@goto.Name, out int id))
                            return new BoundGotoDeclaration(id);
                        throw new Exception("Some odd Error in BoundTree");
                    }
                case GotoStatement @goto:
                    {
                        if (_gotoLabels.TryGetValue(@goto.To, out int id))
                            return new BoundGotoStatement(id);
                        throw new Exception($"@goto label: {@goto.To} not found");
                    }
                case ReturnStatement st:
                    {
                        return new BoundReturnStatement(BindExpression(st.Expression));
                    }
                case AssignmentStatement assignment:
                    {
                        if (!scopes.Lookup(assignment.Name, out TypeSymbol type)) throw new Exception($"Variable {assignment.Name} not exist");

                        BoundExpression ex;

                        if (assignment.Op is Operations op)
                        {
                            ex = BindExpression(new BinaryExpression(new VariableExpression(assignment.Name), op, assignment.Right));
                            return new BoundAssignmentStatement(assignment.Name, ex, type);
                        }

                        ex = BindExpression(assignment.Right!);
                        return new BoundAssignmentStatement(assignment.Name, ex, type);
                    }
                case UnaryExpression unary:
                    {
                        return new BoundExpressionStatement(BindExpression(unary));
                    }
            }
            throw new Exception("Failed to bind a statement");
        }

        public BoundExpression BindExpression(Expression expression)
        {
            switch (expression)
            {
                case NumberExpression number:
                    return new BoundNumberExpression(number.Type, number.Value);
                case DoubleExpression @double:
                    return new BoundDoubleExpression(@double.Type, @double.Value);
                case VariableExpression variable:
                    {
                        if (!scopes.Lookup(variable.Name, out TypeSymbol type)) throw new Exception($"Variable {variable.Name} not exist");
                        return new BoundVariableExpression(type, variable.Name);
                    }
                case BoolExpression ex:
                    {
                        return new BoundBoolExpression(ex.Value, ex.Type);
                    }
                case BinaryExpression binary:
                    {
                        return BindBinaryExpression(binary);
                    }
                case UnaryExpression unary:
                    {
                         return BindUnaryExpression(unary);
                    }
            }
            throw new Exception("Failed to BindExpression");
        }
        public BoundBinaryExpression BindBinaryExpression(BinaryExpression binary)
        {
            var left = BindExpression(binary.Left);
            var right = BindExpression(binary.Right);

            bool leftInteger = left.Type is IntegerTypeSymbol or BoolTypeSymbol;
            bool rightInteger = right.Type is IntegerTypeSymbol or BoolTypeSymbol;

            bool leftArithmetic = leftInteger || left.Type is FloatTypeSymbol;
            bool rightArithmetic = rightInteger || right.Type is FloatTypeSymbol;

            if (binary.Op is Operations.And or Operations.Or)
            {
                if ((!leftArithmetic && left.Type is not PointerTypeSymbol) || (!rightArithmetic && right.Type is not PointerTypeSymbol))
                    throw new ArgumentException("Expected scalar");

                return new BoundBinaryExpression(_typeTable.Int32, binary.Op, left, right);
            }

            if (!leftArithmetic || !rightArithmetic) throw new ArgumentException("Expected integer or floating point");

            bool integerOnly = binary.Op is Operations.DivisionRem or Operations.BitOr or Operations.BitXor or Operations.BitAnd or Operations.LeftShift or Operations.RightShift;

            if (integerOnly && (leftInteger || rightInteger)) throw new ArgumentException("Expected integer");

            if (left.Type is BoolTypeSymbol || left.Type is IntegerTypeSymbol leftInt && (int)leftInt.Size < 4)
            {
                right = BindConversion(right, _typeTable.Int32);
            }

            if (binary.Op is Operations.LeftShift or Operations.RightShift) return new BoundBinaryExpression(left.Type, binary.Op, left, right);

            TypeSymbol operandType = GetCommonArithmeticType(left.Type, right.Type);

            left = BindConversion(left, operandType);
            right = BindConversion(right, operandType);

            TypeSymbol resType = binary.Op switch
            {
                Operations.GreaterThan or Operations.GreaterThanOrEqual
                or Operations.LessThan or Operations.LessThanOrEqual
                or Operations.EqualsEquals or Operations.NotEquals => _typeTable.Int32,

                Operations.Plus or Operations.Minus
                or Operations.Multiply or Operations.Divide or Operations.DivisionRem
                or Operations.BitAnd or Operations.BitXor or Operations.BitOr => operandType,

                _ => throw new NotSupportedException("Unknown operation")
            };

            return new BoundBinaryExpression(resType, binary.Op, left, right);
        }
        public BoundUnaryExpression BindUnaryExpression(UnaryExpression unary)
        {
            var right = BindExpression(unary.Right);

            bool isInteger = right.Type is IntegerTypeSymbol or BoolTypeSymbol;
            bool isArithmetic = isInteger || right.Type is FloatTypeSymbol;

            switch (unary.Op)
            {
                case Operations.Not:
                    if (!isArithmetic && right.Type is not PointerTypeSymbol) throw new ArgumentException("invalid operation for !");
                    return new BoundUnaryExpression(_typeTable.Int32, unary.Op, right);

                case Operations.Plus:
                case Operations.Minus:
                case Operations.BitNot:
                    {
                        if (unary.Op is Operations.BitNot ? !isInteger : !isArithmetic) throw new ArgumentException($"invalid operation for {unary.Op}");

                        if (right.Type is BoolTypeSymbol || right.Type is IntegerTypeSymbol integer && (int)integer.Size < 4)
                            right = BindConversion(right, _typeTable.Int32);

                        return new BoundUnaryExpression(right.Type, unary.Op, right);
                    }

                case Operations.PlusPlusLeft:
                case Operations.PlusPlusRight:
                case Operations.MinusMinusLeft:
                case Operations.MinusMinusRight:
                    {
                        if (right is not BoundVariableExpression) throw new ArgumentException("Expected a variable");

                        if (!isArithmetic) throw new ArgumentException("Invalid operand");

                        return new BoundUnaryExpression(right.Type, unary.Op, right);
                    }
                default: throw new NotSupportedException($"Unsupported unary: {unary.Op}");
            }
        }

        private BoundExpression BindConversion(BoundExpression expression, TypeSymbol targetType)
        {
            if(expression.Type == targetType) return expression;

            bool sourceNumeric = expression.Type is IntegerTypeSymbol or BoolTypeSymbol or FloatTypeSymbol;
            bool targetNumeric = targetType is IntegerTypeSymbol or BoolTypeSymbol or FloatTypeSymbol;

            if (sourceNumeric && targetNumeric) return new BoundConversionExpression(expression, targetType);

            throw new ArgumentException($"Cannot convert {expression.Type.Name} to {targetType.Name}");
        }

        private TypeSymbol GetCommonArithmeticType(TypeSymbol left, TypeSymbol right)
        {
            if (left is BoolTypeSymbol || left is IntegerTypeSymbol left_ && (int)left_.Size < 4) left = _typeTable.Int32;

            if (right is BoolTypeSymbol || right is IntegerTypeSymbol right_ && (int)right_.Size < 4) right = _typeTable.Int32;

            bool leftArithmetic = left is IntegerTypeSymbol or FloatTypeSymbol;
            bool rightArithmetic = right is IntegerTypeSymbol or FloatTypeSymbol;

            if (left == right) return left;

            if (left is FloatTypeSymbol leftFloat && right is FloatTypeSymbol rightFloat)
                return (int)leftFloat.Size > (int)rightFloat.Size ? left : right;

            if (left is FloatTypeSymbol) return left;
            if (right is FloatTypeSymbol) return right;

            var leftInteger = (IntegerTypeSymbol)left;
            var rightInteger = (IntegerTypeSymbol)right;

            if (leftInteger.IsSigned == rightInteger.IsSigned) return (int)leftInteger.Size > (int)rightInteger.Size ? left : right;

            IntegerTypeSymbol signed = leftInteger.IsSigned ? leftInteger : rightInteger;
            IntegerTypeSymbol unsigned = rightInteger.IsSigned ? rightInteger : leftInteger;

            if ((int)unsigned.Size >= (int)signed.Size) return unsigned;

            return signed;
        }
    }
}
