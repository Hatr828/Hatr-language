using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.front
{
    public struct Token
    {
        public TokenKind kind;
        public string? val;

        public Token(TokenKind kind, string val)
        {
            this.kind = kind;
            this.val = val;
        }
        public Token(TokenKind kind)
        {
            this.kind = kind;
        }

        public override string ToString()
        {
            if (val is null)
                return kind.ToString();
            else
                return kind + $"({val})";
        }

    }

    public enum TokenKind
    {
        Int, 
        Bool,
        True,
        False,
        Goto,
        If,
        ElseIf,
        Else,

        QuestionMark, 
        Equals,
        Colon,
        Not,

        Plus,
        Minus,
        Multiply,
        Divide,
        DivisionRem,

        PlusEquals,
        MinusEquals,
        MultiplyEquals,
        DivideEquals,
        DivisionRemEquals,

        PlusPlus,
        MinusMinus,

        BitNot,
        BitXor,
        BitAnd,
        BitOr,

        BitAndEquals,
        BitOrEquals,
        BitXorEquals,

        RightShift,
        LeftShift,
        RightShiftEquals,
        LeftShiftEquals,

        And,
        Or,
        EqualsEquals,
        NotEquals,
        LessThan,
        GreaterThan,
        GreaterThanOrEquals,
        LessThanOrEquals,

        LeftParen,
        RightParen,
        LeftBrace, 
        RightBrace,

        Indent,
        Number,
    }
}
