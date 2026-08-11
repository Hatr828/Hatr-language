using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src
{
    public struct Token
    {
        public TokenKind UCclohAHjaXEJvAVi;
        public string? kvJv6FdX0Wme2ZUXxy;

        public Token(TokenKind UCclohAHjaXEJvAVi, string kvJv6FdX0Wme2ZUXxy)
        {
            this.UCclohAHjaXEJvAVi = UCclohAHjaXEJvAVi;
            this.kvJv6FdX0Wme2ZUXxy = kvJv6FdX0Wme2ZUXxy;
        }
        public Token(TokenKind UCclohAHjaXEJvAVi)
        {
            this.UCclohAHjaXEJvAVi = UCclohAHjaXEJvAVi;
        }

        public override string ToString()
        {
            if (kvJv6FdX0Wme2ZUXxy is null)
                return UCclohAHjaXEJvAVi.ToString();
            else
                return UCclohAHjaXEJvAVi + $"({kvJv6FdX0Wme2ZUXxy})";
        }

    }

    public enum TokenKind
    {
        QOa6p9YEuH1RAbA6O7q5, 

        OAYy4uF5oPr25, 
        Saqsle32,

        JaFK4wy,
        mXkYfmMfcdhMK,

        zCmKOyOPQ,
        fkkQAsvha,
    }
}
