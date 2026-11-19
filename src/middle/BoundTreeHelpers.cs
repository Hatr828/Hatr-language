using Best_language.src.front;
using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.middle
{
    public partial class BoundTree
    {
        private BoundBinaryExpression BindBinaryExpression(BinaryExpression binary)
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
        private BoundUnaryExpression BindUnaryExpression(UnaryExpression unary)
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
            if (expression.Type == targetType) return expression;

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

        private void AddGotoLabel(string name)
        {
            if (_gotoLabels.ContainsKey(name)) throw new ArgumentException($"This label already exist: {name}");
            _gotoLabels.Add(name, _labelId++);
        }
        
        private void AddVariableToScope(VariableSymbol var)
        {
            if (!scopes.TryAddVariable(var)) throw new ArgumentException($"Variable {var.Name} already exist");
        }

        private void IsVariableExist(string name, out TypeSymbol type)
        {
            if (!scopes.Lookup(name, out type)) throw new ArgumentException($"Variable {name} not exist");
        }

        private void GetVariable(string name, out VariableSymbol variable)
        {
            if (!scopes.TryGetVariable(name, out variable)) throw new ArgumentException($"Variable {name} not exist");
        }

        private Operations GetOperationFromCompoundOperation(Operations op) => op switch
        {
            Operations.MinusEquals => Operations.Minus,
            Operations.PlusEquals => Operations.Plus,
            Operations.DivideEquals => Operations.Divide,
            Operations.DivisionRemEquals => Operations.DivisionRem,
            Operations.MultiplyEquals => Operations.Multiply,

            Operations.BitAndEquals => Operations.BitAnd,
            Operations.BitOrEquals => Operations.BitOr,
            Operations.BitXorEquals => Operations.BitXor,

            Operations.LeftShiftEquals => Operations.LeftShift,
            Operations.RightShiftEquals => Operations.RightShift,

            _ => throw new ArgumentException("Unknown compound operation")
        };
    }
}
