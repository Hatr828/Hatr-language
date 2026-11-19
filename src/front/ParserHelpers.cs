using System;
using System.Collections.Generic;
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

        private (Operations, int) GetOperatorAndPriority() => _tokens[_idx++].kind switch
        {
            TokenKind.PlusPlus => (Operations.PlusPlusLeft, 90),
            TokenKind.MinusMinus => (Operations.MinusMinusLeft, 90),

            TokenKind.Not => (Operations.Not, 80),

            TokenKind.Multiply => (Operations.Multiply, 70),
            TokenKind.Divide => (Operations.Divide, 70),
            TokenKind.DivisionRem => (Operations.DivisionRem, 70),

            TokenKind.Plus => (Operations.Plus, 60),
            TokenKind.Minus => (Operations.Minus, 60),

            TokenKind.LeftShift => (Operations.LeftShift, 50),
            TokenKind.RightShift => (Operations.RightShift, 50),

            TokenKind.GreaterThan => (Operations.GreaterThan, 40),
            TokenKind.GreaterThanOrEquals => (Operations.GreaterThanOrEqual, 40),
            TokenKind.LessThan => (Operations.LessThan, 40),
            TokenKind.LessThanOrEquals => (Operations.LessThanOrEqual, 40),

            TokenKind.EqualsEquals => (Operations.EqualsEquals, 30),
            TokenKind.NotEquals => (Operations.NotEquals, 30),

            TokenKind.BitAnd => (Operations.BitAnd, 25),
            TokenKind.BitXor => (Operations.BitXor, 20),
            TokenKind.BitOr => (Operations.BitOr, 15),

            TokenKind.And => (Operations.And, 10),
            TokenKind.Or => (Operations.Or, 5),

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

        private Operations GetOperatorEquals() => _tokens[_idx++].kind switch
        {
            TokenKind.PlusEquals => Operations.PlusEquals,
            TokenKind.MinusEquals => Operations.MinusEquals,
            TokenKind.MultiplyEquals => Operations.MultiplyEquals,
            TokenKind.DivideEquals => Operations.DivideEquals,
            TokenKind.DivisionRemEquals => Operations.DivisionRemEquals,

            TokenKind.BitAndEquals => Operations.BitAndEquals,
            TokenKind.BitOrEquals => Operations.BitOrEquals,
            TokenKind.BitXorEquals => Operations.BitXorEquals,

            TokenKind.RightShiftEquals => Operations.RightShiftEquals,
            TokenKind.LeftShiftEquals => Operations.LeftShiftEquals,

            _ => throw new ArgumentException("Unknown Operation")
        };

        private TypeSymbol GetTypeFromToken(TokenKind kind, bool isSigned) => kind switch
        {
            TokenKind.Bool => _typeTable.Bool,

            TokenKind.Long => isSigned ? _typeTable.Int64 : _typeTable.UInt64,
            TokenKind.Int => isSigned ? _typeTable.Int32 : _typeTable.UInt32,
            TokenKind.Short => isSigned ? _typeTable.Int16 : _typeTable.UInt16,
            TokenKind.Char => isSigned ? _typeTable.Int8 : _typeTable.UInt8,
            _ => throw new ArgumentException("Unknown type")
        };

        private bool IsExpression(TokenKind kind) => kind switch
        {
            TokenKind.Plus or TokenKind.Minus or TokenKind.Not or TokenKind.BitNot or TokenKind.PlusPlus or TokenKind.MinusMinus => true,
            TokenKind.RightParen => true,
            TokenKind.Indent or TokenKind.Number => true,

            _ => false,
        };

        private TypeSymbol ParseType()
        {
            bool hasInt = false, hasChar = false, hasShort = false, hasDouble = false;
            int longCount = 0;
            bool? isSigned = null;

            int start = _idx;

            while (_tokens[_idx].kind is TokenKind.Signed or TokenKind.UnSigned or TokenKind.Long or TokenKind.Int or TokenKind.Char or TokenKind.Short or TokenKind.Short or TokenKind.Double)
            {
                if (_tokens[_idx].kind is TokenKind.Int)
                {
                    if (hasInt) throw new ArgumentException("cannot have two ints");
                    hasInt = true;
                }
                else if (_tokens[_idx].kind is TokenKind.Double)
                {
                    if (hasDouble) throw new ArgumentException("cannot have two doubles");
                    hasDouble = true;
                }
                else if (_tokens[_idx].kind is TokenKind.Short)
                {
                    if (hasShort) throw new ArgumentException("cannot have two shorts");
                    hasShort = true;
                }
                else if (_tokens[_idx].kind is TokenKind.Char)
                {
                    if (hasChar) throw new ArgumentException("cannot have two chars");
                    hasChar = true;
                }

                else if (_tokens[_idx].kind is TokenKind.Long) longCount++;

                else if (_tokens[_idx].kind is TokenKind.Signed)
                {
                    if (isSigned is not null) throw new ArgumentException("cannot have two signed/unsigned");
                    isSigned = true;
                }
                else if (_tokens[_idx].kind is TokenKind.UnSigned)
                {
                    if (isSigned is not null) throw new ArgumentException("cannot have two signed/unsigned");
                    isSigned = false;
                }
                _idx++;
            }

            if (start == _idx)
            {
                if (_tokens[_idx].kind is TokenKind.Bool)
                {
                    _idx++;
                    return _typeTable.Bool;
                }
                else if (_tokens[_idx].kind is TokenKind.Void)
                {
                    _idx++;
                    return _typeTable.Void;
                }
                else if (_tokens[_idx].kind is TokenKind.Float)
                {
                    _idx++;
                    return _typeTable.Float;
                }
            }

            if ((hasChar || hasShort) && longCount != 0) throw new ArgumentException("Cannot have char/short with long");

            if (isSigned is not null && hasDouble) throw new ArgumentException("Cannot have signed/unsigned with double");

            if ((hasChar || hasShort || hasInt) && hasDouble) throw new ArgumentException("Cannot have int/char/short with double");

            if (longCount > 2) throw new ArgumentException("Cannot have more than 2 longs");

            bool signed = isSigned ?? true;

            if (hasChar)
            {
                if (isSigned is null) return _typeTable.Char;

                return signed ? _typeTable.Int8 : _typeTable.UInt8;
            }

            if (hasShort) return signed ? _typeTable.Int16 : _typeTable.UInt16;

            if (longCount == 2) return signed ? _typeTable.Int64 : _typeTable.UInt64;

            if (hasDouble) return _typeTable.Double;

            if (longCount == 1) return signed ? _typeTable.Long : _typeTable.ULong;

            return signed ? _typeTable.Int32 : _typeTable.UInt32;
        }

    }
}
