using Best_language.src.front;

namespace Best_language.src.middle
{
    public partial class BoundTree
    {
        private Dictionary<(Operation op, System.Type, System.Type), BinaryRule> _binaryRules = new()
        {
            // IntegerArithmetic
            [(Operation.Plus, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.IntegerArithmetic,
            [(Operation.Minus, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.IntegerArithmetic,
            [(Operation.Multiply, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.IntegerArithmetic,
            [(Operation.Divide, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.IntegerArithmetic,
            //

            // Arithmetic
            [(Operation.Plus, typeof(FloatTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.Arithmetic,
            [(Operation.Minus, typeof(FloatTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.Arithmetic,
            [(Operation.Multiply, typeof(FloatTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.Arithmetic,
            [(Operation.Divide, typeof(FloatTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.Arithmetic,

            [(Operation.Plus, typeof(IntegerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Arithmetic,
            [(Operation.Minus, typeof(IntegerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Arithmetic,
            [(Operation.Multiply, typeof(IntegerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Arithmetic,
            [(Operation.Divide, typeof(IntegerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Arithmetic,

            [(Operation.Plus, typeof(FloatTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Arithmetic,
            [(Operation.Minus, typeof(FloatTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Arithmetic,
            [(Operation.Multiply, typeof(FloatTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Arithmetic,
            [(Operation.Divide, typeof(FloatTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Arithmetic,
            //

            // Logical
            [(Operation.And, typeof(IntegerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Logical,
            [(Operation.Or, typeof(IntegerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Logical,

            [(Operation.And, typeof(FloatTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Logical,
            [(Operation.Or, typeof(FloatTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Logical,

            [(Operation.And, typeof(IntegerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Logical,
            [(Operation.Or, typeof(IntegerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Logical,

            [(Operation.And, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.Logical,
            [(Operation.Or, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.Logical,

            [(Operation.And, typeof(PointerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.Logical,
            [(Operation.Or, typeof(PointerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.Logical,

            [(Operation.And, typeof(IntegerTypeSymbol), typeof(PointerTypeSymbol))] = BinaryRule.Logical,
            [(Operation.Or, typeof(IntegerTypeSymbol), typeof(PointerTypeSymbol))] = BinaryRule.Logical,

            [(Operation.And, typeof(PointerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Logical,
            [(Operation.Or, typeof(PointerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.Logical,

            [(Operation.And, typeof(FloatTypeSymbol), typeof(PointerTypeSymbol))] = BinaryRule.Logical,
            [(Operation.Or, typeof(FloatTypeSymbol), typeof(PointerTypeSymbol))] = BinaryRule.Logical,

            [(Operation.And, typeof(PointerTypeSymbol), typeof(PointerTypeSymbol))] = BinaryRule.Logical,
            [(Operation.Or, typeof(PointerTypeSymbol), typeof(PointerTypeSymbol))] = BinaryRule.Logical,
            //

            // ArithmeticComparison
            [(Operation.EqualsEquals, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.EqualsEquals, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.LessThan, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.LessThanOrEqual, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.GreaterThan, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.GreaterThanOrEqual, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.ArithmeticComparison,

            [(Operation.EqualsEquals, typeof(IntegerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.NotEquals, typeof(IntegerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.LessThan, typeof(IntegerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.LessThanOrEqual, typeof(IntegerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.GreaterThan, typeof(IntegerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.GreaterThanOrEqual, typeof(IntegerTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.ArithmeticComparison,

            [(Operation.EqualsEquals, typeof(FloatTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.NotEquals, typeof(FloatTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.LessThan, typeof(FloatTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.LessThanOrEqual, typeof(FloatTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.GreaterThan, typeof(FloatTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.GreaterThanOrEqual, typeof(FloatTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.ArithmeticComparison,

            [(Operation.EqualsEquals, typeof(FloatTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.NotEquals, typeof(FloatTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.LessThan, typeof(FloatTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.LessThanOrEqual, typeof(FloatTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.GreaterThan, typeof(FloatTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.ArithmeticComparison,
            [(Operation.GreaterThanOrEqual, typeof(FloatTypeSymbol), typeof(FloatTypeSymbol))] = BinaryRule.ArithmeticComparison,
            //

            // IntegerArithmetic
            [(Operation.DivisionRem, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.IntegerArithmetic,
            [(Operation.BitAnd, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.IntegerArithmetic,
            [(Operation.BitOr, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.IntegerArithmetic,
            [(Operation.BitXor, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.IntegerArithmetic,
            // 

            // Shift
            [(Operation.LeftShift, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.Shift,
            [(Operation.RightShift, typeof(IntegerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.Shift,
            //

            //  PointerArithmetic
            [(Operation.Plus, typeof(PointerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.PointerArithmetic,
            [(Operation.Plus, typeof(IntegerTypeSymbol), typeof(PointerTypeSymbol))] = BinaryRule.PointerArithmetic,
            [(Operation.Minus, typeof(PointerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.PointerArithmetic,
            //

            // PointerComparison
            [(Operation.EqualsEquals, typeof(PointerTypeSymbol), typeof(PointerTypeSymbol))] = BinaryRule.PointerComparison,
            [(Operation.NotEquals, typeof(PointerTypeSymbol), typeof(PointerTypeSymbol))] = BinaryRule.PointerComparison,
            [(Operation.LessThan, typeof(PointerTypeSymbol), typeof(PointerTypeSymbol))] = BinaryRule.PointerComparison,
            [(Operation.LessThanOrEqual, typeof(PointerTypeSymbol), typeof(PointerTypeSymbol))] = BinaryRule.PointerComparison,
            [(Operation.GreaterThan, typeof(PointerTypeSymbol), typeof(PointerTypeSymbol))] = BinaryRule.PointerComparison,
            [(Operation.GreaterThanOrEqual, typeof(PointerTypeSymbol), typeof(PointerTypeSymbol))] = BinaryRule.PointerComparison,

            [(Operation.EqualsEquals, typeof(IntegerTypeSymbol), typeof(PointerTypeSymbol))] = BinaryRule.PointerComparison,
            [(Operation.NotEquals, typeof(IntegerTypeSymbol), typeof(PointerTypeSymbol))] = BinaryRule.PointerComparison,

            [(Operation.EqualsEquals, typeof(PointerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.PointerComparison,
            [(Operation.NotEquals, typeof(PointerTypeSymbol), typeof(IntegerTypeSymbol))] = BinaryRule.PointerComparison,
            //
        };

        private Dictionary<(Operation op, System.Type), UnaryRule> _unaryRules = new()
        {
            [(Operation.PlusPlusLeft, typeof(IntegerTypeSymbol))] = UnaryRule.Arithmetic,
            [(Operation.PlusPlusRight, typeof(IntegerTypeSymbol))] = UnaryRule.Arithmetic,
            [(Operation.MinusMinusLeft, typeof(IntegerTypeSymbol))] = UnaryRule.Arithmetic,
            [(Operation.MinusMinusRight, typeof(IntegerTypeSymbol))] = UnaryRule.Arithmetic,

            [(Operation.PlusPlusLeft, typeof(FloatTypeSymbol))] = UnaryRule.Arithmetic,
            [(Operation.PlusPlusRight, typeof(FloatTypeSymbol))] = UnaryRule.Arithmetic,
            [(Operation.MinusMinusLeft, typeof(FloatTypeSymbol))] = UnaryRule.Arithmetic,
            [(Operation.MinusMinusRight, typeof(FloatTypeSymbol))] = UnaryRule.Arithmetic,

            [(Operation.PlusPlusLeft, typeof(PointerTypeSymbol))] = UnaryRule.Arithmetic,
            [(Operation.PlusPlusRight, typeof(PointerTypeSymbol))] = UnaryRule.Arithmetic,
            [(Operation.MinusMinusLeft, typeof(PointerTypeSymbol))] = UnaryRule.Arithmetic,
            [(Operation.MinusMinusRight, typeof(PointerTypeSymbol))] = UnaryRule.Arithmetic,

            [(Operation.Plus, typeof(IntegerTypeSymbol))] = UnaryRule.PlusOrMinus,
            [(Operation.Minus, typeof(IntegerTypeSymbol))] = UnaryRule.PlusOrMinus,
            [(Operation.Plus, typeof(FloatTypeSymbol))] = UnaryRule.PlusOrMinus,
            [(Operation.Minus, typeof(FloatTypeSymbol))] = UnaryRule.PlusOrMinus,

            [(Operation.BitNot, typeof(IntegerTypeSymbol))] = UnaryRule.Arithmetic,

            [(Operation.Not, typeof(FloatTypeSymbol))] = UnaryRule.Logical,
            [(Operation.Not, typeof(PointerTypeSymbol))] = UnaryRule.Logical,
            [(Operation.Not, typeof(IntegerTypeSymbol))] = UnaryRule.Logical,

            [(Operation.ReferenceOf, typeof(IntegerTypeSymbol))] = UnaryRule.ReferenceOf,
            [(Operation.ReferenceOf, typeof(PointerTypeSymbol))] = UnaryRule.ReferenceOf,
            [(Operation.ReferenceOf, typeof(FloatTypeSymbol))] = UnaryRule.ReferenceOf,
            [(Operation.ReferenceOf, typeof(EnumTypeSymbol))] = UnaryRule.ReferenceOf,
            [(Operation.ReferenceOf, typeof(StructTypeSymbol))] = UnaryRule.ReferenceOf,

            [(Operation.Dereference, typeof(PointerTypeSymbol))] = UnaryRule.Dereference,
        };

        private BoundBinaryExpression BindBinaryExpression(BinaryExpression binary)
        {
            var left = BindExpression(binary.Left);
            var right = BindExpression(binary.Right);

            var key = (binary.Op, left.Type.GetType(), right.Type.GetType());

            if (!_binaryRules.TryGetValue(key, out var val)) throw new ArgumentException($"Operator: {binary.Op} cannot be applied to types {left.Type} and {right.Type}");

            return BindByRule(val, binary.Op, left, right);
        }

        private BoundBinaryExpression BindByRule(BinaryRule rule, Operation op, BoundExpression left, BoundExpression right)
        {
            switch (rule)
            {
                case BinaryRule.IntegerArithmetic:
                case BinaryRule.Arithmetic:
                    var type = GetCommonArithmeticType(left.Type, right.Type);

                    if (type != left.Type) left = BindConversion(left, type);
                    if (type != right.Type) right = BindConversion(right, type);

                    return new BoundBinaryExpression(type, op, left, right);
                
                case BinaryRule.Logical:
                    return new BoundBinaryExpression(_typeTable.Int32, op, BindConversion(left, _typeTable.Int32), BindConversion(right, _typeTable.Int32));
                
                case BinaryRule.Shift:
                    if ((left.Type is IntegerTypeSymbol integer3 && (int)integer3.Size < 4) || left.Type is BoolTypeSymbol) 
                        left = BindConversion(left, _typeTable.Int32);
                    
                    if ((right.Type is IntegerTypeSymbol integer4 && (int)integer4.Size < 4) || right.Type is IntegerTypeSymbol) 
                        right = BindConversion(right, _typeTable.Int32);

                    return new BoundBinaryExpression(left.Type, op, left, right);

                case BinaryRule.PointerComparison:
                case BinaryRule.ArithmeticComparison:
                    return new BoundBinaryExpression(_typeTable.Int32, op, left, right);

                case BinaryRule.PointerArithmetic:
                    if (left.Type is PointerTypeSymbol && right.Type is PointerTypeSymbol) goto case default;

                    if (left.Type is PointerTypeSymbol)
                        return new BoundBinaryExpression(left.Type, op, left, right);
                    else if (right.Type is PointerTypeSymbol)
                        return new BoundBinaryExpression(right.Type, op, left, right);

                    goto default;

                default: throw new NotImplementedException();
            }
        }

        private BoundUnaryExpression BindByRule(UnaryRule rule, Operation op, BoundExpression right)
        {
            switch (rule)
            {
                case UnaryRule.PlusOrMinus:
                    return new BoundUnaryExpression(right.Type, op, right);

                case UnaryRule.Arithmetic:
                    if (right.Type is BoolTypeSymbol || right.Type is IntegerTypeSymbol integer && (int)integer.Size < 4)
                        right = BindConversion(right, _typeTable.Int32);

                    return new BoundUnaryExpression(right.Type, op, right);

                case UnaryRule.Logical:
                    return new BoundUnaryExpression(_typeTable.Int32, op, right);

                case UnaryRule.ReferenceOf:
                    {
                        bool canAddress = CanTakeAddress(right);

                        if (!canAddress) throw new ArgumentException("Invalid operand for &");

                        if (right is BoundMemberAccessExpression st)
                            return new BoundUnaryExpression(_typeTable.GetPointer(st.Field.Type), op, right);
                        else 
                            return new BoundUnaryExpression(_typeTable.GetPointer(right.Type), op, right);
                    }
                case UnaryRule.Dereference:
                    {
                        var tmp = (PointerTypeSymbol)right.Type;
                        return new BoundUnaryExpression(tmp.ElementType, op, right);
                    }

                default: throw new NotImplementedException();
            }
        }

        private BoundUnaryExpression BindUnaryExpression(UnaryExpression unary)
        {
            var right = BindExpression(unary.Right);
            var key = (unary.Op, right.Type.GetType());

            if (!_unaryRules.TryGetValue(key, out var val)) throw new ArgumentException($"Operator: {unary.Op} cannot be applied to type {right.Type}");

            return BindByRule(val, unary.Op, right);
        }

        private BoundExpression BindConversion(BoundExpression expression, TypeSymbol targetType)
        {
            TypeSymbol type = expression.Type;

            if (expression is BoundMemberAccessExpression tmp) type = tmp.Field.Type;

            if (type == targetType) return expression;

            bool sourceNumeric = type is IntegerTypeSymbol or BoolTypeSymbol or FloatTypeSymbol;
            bool targetNumeric = targetType is IntegerTypeSymbol or BoolTypeSymbol or FloatTypeSymbol;

            if (sourceNumeric && targetNumeric) return new BoundConversionExpression(expression, targetType);

            throw new ArgumentException($"Cannot convert {expression.Type.Name} to {targetType}");
        }

        private TypeSymbol GetCommonArithmeticType(TypeSymbol left, TypeSymbol right)
        {
            if (left is BoolTypeSymbol || left is IntegerTypeSymbol left_ && (int)left_.Size < 4) left = _typeTable.Int32;

            if (right is BoolTypeSymbol || right is IntegerTypeSymbol right_ && (int)right_.Size < 4) right = _typeTable.Int32;

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
            if (!_scopes.TryAddVariable(var)) throw new ArgumentException($"Variable {var.Name} already exist");
        }

        private void GetVariable(string name, out VariableSymbol variable)
        {
            if (!_scopes.TryGetVariable(name, out variable)) throw new ArgumentException($"Variable {name} not exist");
        }

        private Operation GetOperationFromCompoundOperation(Operation op) => op switch
        {
            Operation.MinusEquals => Operation.Minus,
            Operation.PlusEquals => Operation.Plus,
            Operation.DivideEquals => Operation.Divide,
            Operation.DivisionRemEquals => Operation.DivisionRem,
            Operation.MultiplyEquals => Operation.Multiply,

            Operation.BitAndEquals => Operation.BitAnd,
            Operation.BitOrEquals => Operation.BitOr,
            Operation.BitXorEquals => Operation.BitXor,

            Operation.LeftShiftEquals => Operation.LeftShift,
            Operation.RightShiftEquals => Operation.RightShift,

            _ => throw new ArgumentException("Unknown compound operation")
        };

        private bool CanTakeAddress(BoundExpression ex) => ex switch
        {
            BoundVariableExpression => true,
            BoundMemberAccessExpression => true,
            BoundUnaryExpression unary when unary.Op is Operation.Dereference => true,

            _ => false,
        };

        private TypeSymbol ResolveEnumType(TypeSymbol type)
        {
            if (type is PointerTypeSymbol pointer)
                return _typeTable.GetPointer(ResolveEnumType(pointer.ElementType));

            if (type is EnumTypeSymbol @enum)
            {
                if (_scopes.TryGetEnum(@enum.Name, out var enumType)) return enumType.EnumType;
                else throw new ArgumentException($"Unknown enum name: {@enum.Name}");
            }

            return type;
        }

        private TypeSymbol ResolveStructType(TypeSymbol type)
        {
            if (type is PointerTypeSymbol pointer)
                return _typeTable.GetPointer(ResolveStructType(pointer.ElementType));

            if (type is StructTypeSymbol @struct)
            {
                if (_scopes.TryGetStruct(@struct.Name, out var structType)) return structType.Item1;
                else throw new ArgumentException($"Unknown enum name: {@struct.Name}");
            }

            return type;
        }

        private bool TryGetOffsetAndType(string structName, string memberName, out int offset, out TypeSymbol type)
        {
            if (!_scopes.TryGetStruct(structName, out var val))
            {
                offset = int.MinValue;
                type = null!;
                return false;
            }
            int i = 0;
            var names = val.Item2;

            for (; i < names.Count; i++) if (names[i] == memberName) break;

            if (i == names.Count)
            {
                offset = int.MinValue;
                type = null!;
                return false;
            }

            type = val.Item1.Types[i];
            offset = val.Item1.Alignment * i;
            return true;
        }
    }
}
