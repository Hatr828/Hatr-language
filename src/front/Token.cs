using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.front
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
        msEw5C9X6eO,
        R5E_HH,
        nwAN5fKImy9,
        VJ9YVMa9I,

        OAYy4uF5oPr25, 
        Saqsle32,

        px4P2OwC_TCS,
        VpNNk81hH01O3w,
        XvjP9ZInqh1c6ky6nK_,
        HVwX8t15uMw86Zjrr,

        dFZBYFCUJWiZYreG5,
        hJQNxCsE8,
        Tieags2HzPi132WNHai,
        wY8aKmHjGo,
        i3e0fh1jq,
        X0mFuD,
        EVNnxDS7mt,

        JaFK4wy,
        mXkYfmMfcdhMK,
        xQOQMX7lMt2mn, 
        XmL8EZAP1m8r4dI9tNJ,

        zCmKOyOPQ,
        fkkQAsvha,
    }
}
