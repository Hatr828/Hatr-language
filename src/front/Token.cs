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
        Void,
        Long,
        Char,
        Short,
        Bool,
        Float, 
        Double,

        Signed, 
        UnSigned,

        U, L, LU, UL, LL, ULL, LLU, F,

        True,
        False,

        Goto,
        If,
        ElseIf,
        Else,

        For,
        Do,
        While,

        Return,
        Break,
        Continue,

        SemiColon,
        Comma,
        Dot,
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

        RightParen,
        LeftParen,
        RightBrace,
        LeftBrace,

        Indent,
        Number,
    }

    public static class CSyntax
    {
        public static Dictionary<string, TokenKind> keywords = new()
        {
            ["int"] = TokenKind.Int,
            ["void"] = TokenKind.Void,
            ["double"] = TokenKind.Double,
            ["float"] = TokenKind.Float,
            ["long"] = TokenKind.Long,
            ["char"] = TokenKind.Char,
            ["short"] = TokenKind.Short,
            ["bool"] = TokenKind.Bool,

            ["signed"] = TokenKind.Signed,
            ["unsigned"] = TokenKind.UnSigned,

            ["if"] = TokenKind.If,
            ["else"] = TokenKind.Else,
            ["true"] = TokenKind.True,
            ["false"] = TokenKind.False,

            ["return"] = TokenKind.Return,

            ["while"] = TokenKind.While,
            ["for"] = TokenKind.For,
            ["do"] = TokenKind.Do,

            ["goto"] = TokenKind.Goto,

            ["U"] = TokenKind.U,
            ["L"] = TokenKind.L,
            ["UL"] = TokenKind.UL,
            ["LU"] = TokenKind.LU,
            ["ULL"] = TokenKind.ULL,
            ["LLU"] = TokenKind.LLU,
            ["F"] = TokenKind.F,

            ["u"] = TokenKind.U,
            ["l"] = TokenKind.L,
            ["ul"] = TokenKind.UL,
            ["lu"] = TokenKind.LU,
            ["ull"] = TokenKind.ULL,
            ["llu"] = TokenKind.LLU,
            ["f"] = TokenKind.F,
        };

        public static Dictionary<char, TokenKind> chars = new()
        {
            ['*'] = TokenKind.Multiply,
            ['+'] = TokenKind.Plus,
            ['-'] = TokenKind.Minus,
            ['/'] = TokenKind.Divide,
            ['%'] = TokenKind.DivisionRem,

            ['('] = TokenKind.LeftParen,
            [')'] = TokenKind.RightParen,
            ['{'] = TokenKind.LeftBrace,
            ['}'] = TokenKind.RightBrace,

            ['='] = TokenKind.Equals,
            [';'] = TokenKind.SemiColon,
            ['>'] = TokenKind.GreaterThan,
            ['<'] = TokenKind.LessThan,
            ['!'] = TokenKind.Not,

            [':'] = TokenKind.Colon,
            [','] = TokenKind.Comma,
            ['.'] = TokenKind.Dot,

            ['~'] = TokenKind.BitNot,
            ['|'] = TokenKind.BitOr,
            ['&'] = TokenKind.BitAnd,
            ['^'] = TokenKind.BitXor,
        };

        public static Dictionary<string, TokenKind> doubleChars = new()
        {
            [">="] = TokenKind.GreaterThanOrEquals,
            ["=="] = TokenKind.EqualsEquals,
            ["!="] = TokenKind.NotEquals,
            ["<="] = TokenKind.LessThanOrEquals,
            ["&&"] = TokenKind.And,
            ["||"] = TokenKind.Or,

            ["++"] = TokenKind.PlusPlus,
            ["--"] = TokenKind.MinusMinus,

            ["<<"] = TokenKind.LeftShift,
            [">>"] = TokenKind.RightShift,

            ["+="] = TokenKind.PlusEquals,
            ["-="] = TokenKind.MinusEquals,
            ["*="] = TokenKind.MultiplyEquals,
            ["/="] = TokenKind.DivideEquals,
            ["%="] = TokenKind.DivisionRemEquals,

            ["^="] = TokenKind.BitXorEquals,
            ["|="] = TokenKind.BitOrEquals,
            ["&="] = TokenKind.BitAndEquals,
        };

        public static Dictionary<(string, string), TokenKind> specialKeywords = new()
        {
            [("else", "if")] = TokenKind.ElseIf,
        };
    }
}
