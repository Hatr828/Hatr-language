using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Best_language.src.front
{
    public partial class Parser
    {
        private void Requires(params TokenKind[] kinds)
        {
            foreach(var kind in kinds) if (_tokens[_idx++].kind != kind) throw new ArgumentException($"Requires {kind}");
        }

        private string Requires(bool getVal, params TokenKind[] kinds)
        {
            foreach (var kind in kinds) if (_tokens[_idx++].kind != kind) throw new ArgumentException($"Requires {kind}");
            if (getVal) return _tokens[_idx - 1].val!;
            return null!;
        }

        public bool CheckKind(params TokenKind[] kinds)
        {
            for (int i = 0; i < kinds.Length; i++)
            {
                if (_tokens[_idx + i].kind != kinds[i]) return false;
            }

            return true;
        }

        public bool Consume(params TokenKind[] kinds)
        {
            for (int i = 0; i < kinds.Length; i++)
            {
                if (_tokens[_idx + i].kind != kinds[i]) return false;
            }
            _idx += kinds.Length;
            return true;
        }

        private void GetBlockCode<T>(List<T> list) where T : AstNode
        {
            _scopes.AddScope();

            Requires(TokenKind.RightBrace);

            while (!Consume(TokenKind.LeftBrace))
            {
                list.Add((T)GetAstNode());
            }

            _scopes.RemoveScope();
        }

        private TypeSymbol GetCommonArithmeticType(TypeSymbol left, TypeSymbol right)
        {
            if (left is BoolTypeSymbol || left is IntegerTypeSymbol left_ && (int)left_.Size < 4) left = typeTable.Int32;

            if (right is BoolTypeSymbol || right is IntegerTypeSymbol right_ && (int)right_.Size < 4) right = typeTable.Int32;

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

        private (Operation, int) GetOperatorAndPriority() => _tokens[_idx].kind switch
        {
            TokenKind.Multiply => (Operation.Multiply, 70),
            TokenKind.Divide => (Operation.Divide, 70),
            TokenKind.DivisionRem => (Operation.DivisionRem, 70),

            TokenKind.Plus => (Operation.Plus, 60),
            TokenKind.Minus => (Operation.Minus, 60),

            TokenKind.LeftShift => (Operation.LeftShift, 50),
            TokenKind.RightShift => (Operation.RightShift, 50),

            TokenKind.GreaterThan => (Operation.GreaterThan, 40),
            TokenKind.GreaterThanOrEquals => (Operation.GreaterThanOrEqual, 40),
            TokenKind.LessThan => (Operation.LessThan, 40),
            TokenKind.LessThanOrEquals => (Operation.LessThanOrEqual, 40),

            TokenKind.EqualsEquals => (Operation.EqualsEquals, 30),
            TokenKind.NotEquals => (Operation.NotEquals, 30),

            TokenKind.BitAnd => (Operation.BitAnd, 25),
            TokenKind.BitXor => (Operation.BitXor, 20),
            TokenKind.BitOr => (Operation.BitOr, 15),

            TokenKind.And => (Operation.And, 10),
            TokenKind.Or => (Operation.Or, 5),

            _ => throw new ArgumentException($"Unknown Operation: {_tokens[_idx].kind}")
        };

        private (Operation, int) GetOperatorAndPriorityUnary() => _tokens[_idx++].kind switch
        {
            TokenKind.PlusPlus => (Operation.PlusPlusLeft, 90),
            TokenKind.MinusMinus => (Operation.MinusMinusLeft, 90),
            TokenKind.Plus => (Operation.Plus, 90),
            TokenKind.Minus => (Operation.Minus, 90),

            TokenKind.Not => (Operation.Not, 90),
            TokenKind.BitNot => (Operation.BitNot, 90),
            TokenKind.BitAnd => (Operation.ReferenceOf, 90),

            TokenKind.Multiply => (Operation.Dereference, 90),

            _ => throw new ArgumentException($"Unknown Operation: {_tokens[_idx - 1].kind}")
        };

        private bool IsOperatorEquals() => _tokens[_idx].kind switch
        {
            TokenKind.PlusEquals or TokenKind.MinusEquals => true,

            TokenKind.DivideEquals or TokenKind.MultiplyEquals or TokenKind.DivisionRemEquals => true,

            TokenKind.BitAndEquals or TokenKind.BitOrEquals or TokenKind.BitXorEquals => true,

            TokenKind.LeftShiftEquals or TokenKind.RightShiftEquals => true,

            _ => false
        };

        private Operation GetOperatorEquals() => _tokens[_idx++].kind switch
        {
            TokenKind.PlusEquals => Operation.PlusEquals,
            TokenKind.MinusEquals => Operation.MinusEquals,
            TokenKind.MultiplyEquals => Operation.MultiplyEquals,
            TokenKind.DivideEquals => Operation.DivideEquals,
            TokenKind.DivisionRemEquals => Operation.DivisionRemEquals,

            TokenKind.BitAndEquals => Operation.BitAndEquals,
            TokenKind.BitOrEquals => Operation.BitOrEquals,
            TokenKind.BitXorEquals => Operation.BitXorEquals,

            TokenKind.RightShiftEquals => Operation.RightShiftEquals,
            TokenKind.LeftShiftEquals => Operation.LeftShiftEquals,

            _ => throw new ArgumentException("Unknown Operation")
        };

        private NumberExpression CombineUnary(Operation op, NumberExpression num) => op switch
        {
            Operation.Plus => new(num.Value, num.Type),
            Operation.Minus => new(~num.Value + 1, num.Type),
            Operation.Not => new(num.Value == 0 ? 1UL : 0UL, num.Type),
            Operation.BitNot => new(~num.Value, num.Type),
            _ => throw new ArgumentException($"Unexpected unary operation: {op}")
        };
        private DoubleExpression CombineUnary(Operation op, DoubleExpression num) => op switch
        {
            Operation.Plus => new(num.Value, num.Type),
            Operation.Minus => new(-num.Value, num.Type),
            Operation.Not => new(num.Value == 0.0 ? 1 : 0, num.Type),
            _ => throw new ArgumentException($"Unexpected unary operation: {op}")
        };
        private NumberExpression CombineBinary(Operation op, NumberExpression left, NumberExpression right) => op switch
        {
            Operation.Plus => new(left.Value + right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.Minus => new(left.Value - right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.Multiply => new(left.Value * right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.Divide => new(left.Value / right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.DivisionRem => new(left.Value % right.Value, GetCommonArithmeticType(left.Type, right.Type)),

            Operation.LeftShift => new(left.Value << (int)right.Value, left.Type),
            Operation.RightShift => new(left.Value >> (int)right.Value, left.Type),

            Operation.BitAnd => new(left.Value & right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.BitOr => new(left.Value | right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.BitXor => new(left.Value ^ right.Value, GetCommonArithmeticType(left.Type, right.Type)),

            Operation.EqualsEquals => new(left.Value == right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.NotEquals => new(left.Value != right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.LessThan => new(left.Value < right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.LessThanOrEqual => new(left.Value <= right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.GreaterThan => new(left.Value > right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.GreaterThanOrEqual => new(left.Value >= right.Value ? 1UL : 0UL, typeTable.Int32),

            _ => throw new ArgumentException($"Unexpected unary operation: {op}")
        };
        private Expression CombineBinary(Operation op, DoubleExpression left, NumberExpression right) => op switch
        {
            Operation.Plus => new DoubleExpression(left.Value + right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.Minus => new DoubleExpression(left.Value - right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.Multiply => new DoubleExpression(left.Value * right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.Divide => new DoubleExpression(left.Value / right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.DivisionRem => new DoubleExpression(left.Value % right.Value, GetCommonArithmeticType(left.Type, right.Type)),

            Operation.EqualsEquals => new NumberExpression(left.Value == right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.NotEquals => new NumberExpression(left.Value != right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.LessThan => new NumberExpression(left.Value < right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.LessThanOrEqual => new NumberExpression(left.Value <= right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.GreaterThan => new NumberExpression(left.Value > right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.GreaterThanOrEqual => new NumberExpression(left.Value >= right.Value ? 1UL : 0UL, typeTable.Int32),

            _ => throw new ArgumentException($"Unexpected unary operation: {op}")
        };
        private Expression CombineBinary(Operation op, DoubleExpression left, DoubleExpression right) => op switch
        {
            Operation.Plus => new DoubleExpression(left.Value + right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.Minus => new DoubleExpression(left.Value - right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.Multiply => new DoubleExpression(left.Value * right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.Divide => new DoubleExpression(left.Value / right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.DivisionRem => new DoubleExpression(left.Value % right.Value, GetCommonArithmeticType(left.Type, right.Type)),

            Operation.EqualsEquals => new NumberExpression(left.Value == right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.NotEquals => new NumberExpression(left.Value != right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.LessThan => new NumberExpression(left.Value < right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.LessThanOrEqual => new NumberExpression(left.Value <= right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.GreaterThan => new NumberExpression(left.Value > right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.GreaterThanOrEqual => new NumberExpression(left.Value >= right.Value ? 1UL : 0UL, typeTable.Int32),

            _ => throw new ArgumentException($"Unexpected unary operation: {op}")
        };
        private Expression CombineBinary(Operation op, NumberExpression left, DoubleExpression right) => op switch
        {
            Operation.Plus => new DoubleExpression(left.Value + right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.Minus => new DoubleExpression(left.Value - right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.Multiply => new DoubleExpression(left.Value * right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.Divide => new DoubleExpression(left.Value / right.Value, GetCommonArithmeticType(left.Type, right.Type)),
            Operation.DivisionRem => new DoubleExpression(left.Value % right.Value, GetCommonArithmeticType(left.Type, right.Type)),

            Operation.EqualsEquals => new NumberExpression(left.Value == right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.NotEquals => new NumberExpression(left.Value != right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.LessThan => new NumberExpression(left.Value < right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.LessThanOrEqual => new NumberExpression(left.Value <= right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.GreaterThan => new NumberExpression(left.Value > right.Value ? 1UL : 0UL, typeTable.Int32),
            Operation.GreaterThanOrEqual => new NumberExpression(left.Value >= right.Value ? 1UL : 0UL, typeTable.Int32),

            _ => throw new ArgumentException($"Unexpected unary operation: {op}")
        };

        private bool IsExpression(TokenKind kind) => kind switch
        {
            TokenKind.Plus or TokenKind.Minus or TokenKind.Not or TokenKind.BitNot or TokenKind.PlusPlus or TokenKind.MinusMinus => true,
            TokenKind.RightParen => true,
            TokenKind.Indent or TokenKind.Number => true,
            TokenKind.Multiply => true, // Dereference

            _ => false,
        };

        private int GetSizeForStruct(List<TypeSymbol> types, out int alignment)
        {
            alignment = 0;

            foreach (var type in types) alignment = Math.Max(alignment, typeTable.GetTypeSize(type));

            return alignment * types.Count;
        }

        private readonly Dictionary<TokenKind[], TypeSymbol> _types = new(new TokenKindSpanComparer())
        {
            [[TokenKind.Bool]] = typeTable.Bool,
            [[TokenKind.Void]] = typeTable.Void,
            [[TokenKind.Enum]] = typeTable.Enum,
            [[TokenKind.Struct]] = typeTable.Struct,

            [[TokenKind.Char]] = typeTable.Char,
            [[TokenKind.Signed, TokenKind.Char]] = typeTable.Int8,
            [[TokenKind.UnSigned, TokenKind.Char]] = typeTable.Int8,

            [[TokenKind.Short]] = typeTable.Int16,
            [[TokenKind.Short, TokenKind.Int]] = typeTable.Int16,
            [[TokenKind.Signed, TokenKind.Short]] = typeTable.Int16,
            [[TokenKind.Signed, TokenKind.Short, TokenKind.Int]] = typeTable.Int16,

            [[TokenKind.UnSigned, TokenKind.Short, TokenKind.Int]] = typeTable.UInt16,
            [[TokenKind.UnSigned, TokenKind.Short]] = typeTable.UInt16,

            [[TokenKind.Int]] = typeTable.Int32,
            [[TokenKind.Signed, TokenKind.Int]] = typeTable.Int32,
            [[TokenKind.Signed]] = typeTable.Int32,

            [[TokenKind.UnSigned, TokenKind.Int]] = typeTable.UInt32,
            [[TokenKind.UnSigned]] = typeTable.UInt32,

            [[TokenKind.Long]] = typeTable.Int32,
            [[TokenKind.Long, TokenKind.Int]] = typeTable.Int32,
            [[TokenKind.Signed, TokenKind.Long]] = typeTable.Int32,
            [[TokenKind.Signed, TokenKind.Long, TokenKind.Int]] = typeTable.Int32,

            [[TokenKind.UnSigned, TokenKind.Long]] = typeTable.UInt32,
            [[TokenKind.UnSigned, TokenKind.Long, TokenKind.Int]] = typeTable.UInt32,

            [[TokenKind.Long, TokenKind.Long]] = typeTable.Int64,
            [[TokenKind.Long, TokenKind.Long, TokenKind.Int]] = typeTable.Int64,
            [[TokenKind.Signed, TokenKind.Long, TokenKind.Long]] = typeTable.Int64,
            [[TokenKind.Signed, TokenKind.Long, TokenKind.Long, TokenKind.Int]] = typeTable.Int64,

            [[TokenKind.UnSigned, TokenKind.Long, TokenKind.Long]] = typeTable.UInt64,
            [[TokenKind.UnSigned, TokenKind.Long, TokenKind.Long, TokenKind.Int]] = typeTable.UInt64,

            [[TokenKind.Float]] = typeTable.Float,

            [[TokenKind.Double]] = typeTable.Double,
            [[TokenKind.Double, TokenKind.Long]] = typeTable.Double,

        };
        private TypeSymbol ParseType()
        {
            List<TokenKind> tmp = new();
            while (_tokens[_idx].kind is >= TokenKind.Int and <= TokenKind.UnSigned)
            {
                tmp.Add(_tokens[_idx++].kind); 
            }

            if (!_types.GetAlternateLookup<ReadOnlySpan<TokenKind>>().TryGetValue(CollectionsMarshal.AsSpan(tmp), out var res))
                throw new InvalidOperationException($"Unknown type: {string.Join(" ", tmp)}");
            return res;
        }

        private class TokenKindSpanComparer : IEqualityComparer<TokenKind[]>, IAlternateEqualityComparer<ReadOnlySpan<TokenKind>, TokenKind[]>
        {
            public bool Equals(TokenKind[]? x, TokenKind[]? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                return x.AsSpan().SequenceEqual(y);
            }

            public bool Equals(ReadOnlySpan<TokenKind> alternate, TokenKind[] other)
            {
                return alternate.SequenceEqual(other);
            }

            public TokenKind[] Create(ReadOnlySpan<TokenKind> alternate)
            {
                return alternate.ToArray();
            }

            public int GetHashCode(TokenKind[] obj) => GetHashCode(obj.AsSpan());

            public int GetHashCode(ReadOnlySpan<TokenKind> alternate)
            {
                var hash = new HashCode();
                hash.AddBytes(MemoryMarshal.AsBytes(alternate));
                return hash.ToHashCode();
            }

        }
    }
}
