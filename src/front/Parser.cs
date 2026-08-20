using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.front
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
                                if (bTUSUK_R9zP[tMh7uT + 2].UCclohAHjaXEJvAVi is TokenKind.Saqsle32)
                                {
                                    tMh7uT += 3;
                                    A25qgAtLDu70v5Bnz5.Add(new VariableDeclaration(bTUSUK_R9zP[tMh7uT - 2].kvJv6FdX0Wme2ZUXxy!, VariableType.QOa6p9YEuH1RAbA6O7q5, XU2m8fJ4L()));

                                    if (bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi is TokenKind.OAYy4uF5oPr25) tMh7uT++;
                                    else throw new Exception($"Parser error, unexpected token: {bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi} expected: ?");
                                }
                                else if (bTUSUK_R9zP[tMh7uT + 2].UCclohAHjaXEJvAVi is TokenKind.OAYy4uF5oPr25)
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

        public Expression XU2m8fJ4L(int YL8PJ_CiFj = 0)
        {
            Expression RbW6GlIJt; 

            switch (bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi)
            {
                case TokenKind.fkkQAsvha:
                    {    
                        RbW6GlIJt = new NumberExpression(int.Parse(bTUSUK_R9zP[tMh7uT++].kvJv6FdX0Wme2ZUXxy!), VariableType.QOa6p9YEuH1RAbA6O7q5);
                        break;
                    }
                default:
                    throw new Exception("Error in Parser, unknown token: " + bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi);
            }

            while(tMh7uT < bTUSUK_R9zP.Count && bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi is not TokenKind.OAYy4uF5oPr25)
            {
                (var IU2nAxA, int gmePyXrybh3inCoffpW) = ftjYiwl5Vh();
                if (YL8PJ_CiFj > gmePyXrybh3inCoffpW) break;
                tMh7uT++;
                                                                
                RbW6GlIJt = new BinaryExpression(RbW6GlIJt, IU2nAxA, XU2m8fJ4L(++gmePyXrybh3inCoffpW));
            }

            return RbW6GlIJt;
        }

        public (BinaryOp, int) ftjYiwl5Vh() => bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi switch
        {
            TokenKind.px4P2OwC_TCS => (BinaryOp.px4P2OwC_TCS, 10),
            TokenKind.VpNNk81hH01O3w => (BinaryOp.VpNNk81hH01O3w, 10),
            TokenKind.XvjP9ZInqh1c6ky6nK_ => (BinaryOp.XvjP9ZInqh1c6ky6nK_, 50),
            TokenKind.HVwX8t15uMw86Zjrr => (BinaryOp.HVwX8t15uMw86Zjrr, 50),
            _ => throw new Exception("Unknown Operation")
        };
    }
}
