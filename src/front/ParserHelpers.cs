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

        private void GetBlockCode<T>(List<T> list) where T : AstNode
        {
            Requires(TokenKind.RightBrace);

            while (_tokens[_idx].kind is not TokenKind.LeftBrace)
            {
                list.Add((T)GetAstNode());
            }
            _idx++;
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

            _ => throw new ArgumentException($"Unknown Operation: {_tokens[_idx - 1].kind}")
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

        private bool IsExpression(TokenKind kind) => kind switch
        {
            TokenKind.Plus or TokenKind.Minus or TokenKind.Not or TokenKind.BitNot or TokenKind.PlusPlus or TokenKind.MinusMinus => true,
            TokenKind.RightParen => true,
            TokenKind.Indent or TokenKind.Number => true,

            _ => false,
        };
        private Dictionary<TokenKind[], TypeSymbol> _types = new(new TokenKindSpanComparer())
        {
            [[TokenKind.Bool]] = _typeTable.Bool,

            [[TokenKind.Char]] = _typeTable.Char,
            [[TokenKind.Signed, TokenKind.Char]] = _typeTable.Int8,
            [[TokenKind.UnSigned, TokenKind.Char]] = _typeTable.Int8,

            [[TokenKind.Short]] = _typeTable.Int16,
            [[TokenKind.Short, TokenKind.Int]] = _typeTable.Int16,
            [[TokenKind.Signed, TokenKind.Short]] = _typeTable.Int16,
            [[TokenKind.Signed, TokenKind.Short, TokenKind.Int]] = _typeTable.Int16,

            [[TokenKind.UnSigned, TokenKind.Short, TokenKind.Int]] = _typeTable.UInt16,
            [[TokenKind.UnSigned, TokenKind.Short]] = _typeTable.UInt16,

            [[TokenKind.Int]] = _typeTable.Int32,
            [[TokenKind.Signed, TokenKind.Int]] = _typeTable.Int32,
            [[TokenKind.Signed]] = _typeTable.Int32,

            [[TokenKind.UnSigned, TokenKind.Int]] = _typeTable.UInt32,
            [[TokenKind.UnSigned]] = _typeTable.UInt32,

            [[TokenKind.Long]] = _typeTable.Int32,
            [[TokenKind.Long, TokenKind.Int]] = _typeTable.Int32,
            [[TokenKind.Signed, TokenKind.Long]] = _typeTable.Int32,
            [[TokenKind.Signed, TokenKind.Long, TokenKind.Int]] = _typeTable.Int32,

            [[TokenKind.UnSigned, TokenKind.Long]] = _typeTable.UInt32,
            [[TokenKind.UnSigned, TokenKind.Long, TokenKind.Int]] = _typeTable.UInt32,

            [[TokenKind.Long, TokenKind.Long]] = _typeTable.Int64,
            [[TokenKind.Long, TokenKind.Long, TokenKind.Int]] = _typeTable.Int64,
            [[TokenKind.Signed, TokenKind.Long, TokenKind.Long]] = _typeTable.Int64,
            [[TokenKind.Signed, TokenKind.Long, TokenKind.Long, TokenKind.Int]] = _typeTable.Int64,

            [[TokenKind.UnSigned, TokenKind.Long, TokenKind.Long]] = _typeTable.UInt64,
            [[TokenKind.UnSigned, TokenKind.Long, TokenKind.Long, TokenKind.Int]] = _typeTable.UInt64,

            [[TokenKind.Float]] = _typeTable.Float,

            [[TokenKind.Double]] = _typeTable.Double,
            [[TokenKind.Double, TokenKind.Long]] = _typeTable.Double,

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
