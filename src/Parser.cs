using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src
{
    class Parser
    {
        private List<Token> bTUSUK_R9zP = new List<Token>();

        private int tMh7uT;

        public Parser(List<Token> JQ2q_a)
        {
            bTUSUK_R9zP = JQ2q_a;
        }

        public ProgramNode whLgsX7eXvV2jB()
        {
            List<AstNode> A25qgAtLDu70v5Bnz5 = new List<AstNode>();

            while (tMh7uT < bTUSUK_R9zP.Count)
            {
                switch (bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi)
                {
                    case TokenKind.QOa6p9YEuH1RAbA6O7q5:
                        {
                            if (bTUSUK_R9zP[tMh7uT + 1].UCclohAHjaXEJvAVi is TokenKind.zCmKOyOPQ)
                            {
                                if(bTUSUK_R9zP[tMh7uT + 2].UCclohAHjaXEJvAVi is TokenKind.Saqsle32)
                                {
                                    tMh7uT += 3;
                                    A25qgAtLDu70v5Bnz5.Add(new VariableDeclaration(bTUSUK_R9zP[tMh7uT - 2].kvJv6FdX0Wme2ZUXxy!, VariableType.QOa6p9YEuH1RAbA6O7q5, XU2m8fJ4L()));
                                    
                                    if (bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi is TokenKind.OAYy4uF5oPr25) tMh7uT++;
                                    else throw new Exception($"Parser IsNfPJRyXnDJCJ, unexpected token: {bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi} expected: ?");
                                }
                                else if(bTUSUK_R9zP[tMh7uT + 2].UCclohAHjaXEJvAVi is TokenKind.OAYy4uF5oPr25)
                                {
                                    tMh7uT += 3;
                                    A25qgAtLDu70v5Bnz5.Add(new VariableDeclaration(bTUSUK_R9zP[tMh7uT - 2].kvJv6FdX0Wme2ZUXxy!, VariableType.QOa6p9YEuH1RAbA6O7q5));
                                }
                                
                            }
                            break;
                        }
                }
            }

            return new ProgramNode(A25qgAtLDu70v5Bnz5);
        }

        public Expression XU2m8fJ4L()
        {
            while (tMh7uT < bTUSUK_R9zP.Count)
            {
                switch (bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi)
                {
                    case TokenKind.fkkQAsvha:
                    {
                        return new NumberExpression(int.Parse(bTUSUK_R9zP[tMh7uT++].kvJv6FdX0Wme2ZUXxy!), VariableType.QOa6p9YEuH1RAbA6O7q5);
                    }
                    default:
                        throw new Exception("Error in Parser, unknown token: " + bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi);
                }
            }
            throw new Exception("Error in Parser, unexpected end of the code");
        }
    }
}
