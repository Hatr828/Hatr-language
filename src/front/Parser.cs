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

             while(tMh7uT < bTUSUK_R9zP.Count)
             {
                 A25qgAtLDu70v5Bnz5.Add(_8CvQhVY3OxAzmhbj());
             }

            return new(A25qgAtLDu70v5Bnz5);
        }
        public AstNode _8CvQhVY3OxAzmhbj()
        {

            while (tMh7uT < bTUSUK_R9zP.Count)
            {
                switch (bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi)
                {
                    case TokenKind.msEw5C9X6eO:
                    case TokenKind.QOa6p9YEuH1RAbA6O7q5:
                        {
                            var FubV605Z9qF5D9z3X = bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi;
                            if (bTUSUK_R9zP[tMh7uT + 1].UCclohAHjaXEJvAVi is TokenKind.zCmKOyOPQ)
                            {
                                if (bTUSUK_R9zP[tMh7uT + 2].UCclohAHjaXEJvAVi is TokenKind.Saqsle32)
                                {
                                    tMh7uT += 3;
                                    var zBgs8nftvUtORz5x7k = new VariableDeclaration(bTUSUK_R9zP[tMh7uT - 2].kvJv6FdX0Wme2ZUXxy!, QKG8TsMd(FubV605Z9qF5D9z3X), XU2m8fJ4L());

                                    if (bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi is not TokenKind.OAYy4uF5oPr25) throw new Exception($"Parser error, unexpected token: {bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi} expected: ?");

                                    tMh7uT++;

                                    return zBgs8nftvUtORz5x7k;
                                }
                                else if (bTUSUK_R9zP[tMh7uT + 2].UCclohAHjaXEJvAVi is TokenKind.OAYy4uF5oPr25)
                                {
                                    tMh7uT += 3;
                                    return new VariableDeclaration(bTUSUK_R9zP[tMh7uT - 2].kvJv6FdX0Wme2ZUXxy!, VariableType.QOa6p9YEuH1RAbA6O7q5);
                                }
                            }
                            throw new Exception("Error in Parser, expected: Indent");
                        }
                    case TokenKind.R5E_HH:
                        {
                            if (bTUSUK_R9zP[tMh7uT + 1].UCclohAHjaXEJvAVi is not TokenKind.JaFK4wy) throw new Exception($"Parser error, unexpected token: {bTUSUK_R9zP[tMh7uT + 1].UCclohAHjaXEJvAVi} expected: }}");
                            tMh7uT += 2;

                            Expression FHZW5YiKBaTQMYfb = XU2m8fJ4L();
                            List<AstNode> zk9bMID2GlAo = new();

                            if (bTUSUK_R9zP[tMh7uT++].UCclohAHjaXEJvAVi is not TokenKind.mXkYfmMfcdhMK) throw new Exception($"Parser error, unexpected token: {bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi} expected: (");
                            if (bTUSUK_R9zP[tMh7uT++].UCclohAHjaXEJvAVi is not TokenKind.xQOQMX7lMt2mn) throw new Exception($"Parser error, unexpected token: {bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi} expected: {{");

                            while (bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi is not TokenKind.XmL8EZAP1m8r4dI9tNJ)
                            {
                                zk9bMID2GlAo.Add(_8CvQhVY3OxAzmhbj());
                            }

                            tMh7uT++;
                            return new IfDeclaration(FHZW5YiKBaTQMYfb, zk9bMID2GlAo);
                        }
                    case TokenKind.lgNzh3UAydkfeRDf2Cu:
                        {
                            if (bTUSUK_R9zP[tMh7uT + 1].UCclohAHjaXEJvAVi is not TokenKind.zCmKOyOPQ) throw new Exception($"Parser error, unexpected token: {bTUSUK_R9zP[tMh7uT + 1].UCclohAHjaXEJvAVi} expected: Indent");
                            if (bTUSUK_R9zP[tMh7uT + 2].UCclohAHjaXEJvAVi is not TokenKind.OAYy4uF5oPr25) throw new Exception($"Parser error, unexpected token: {bTUSUK_R9zP[tMh7uT + 1].UCclohAHjaXEJvAVi} expected: {TokenKind.OAYy4uF5oPr25}");
                            tMh7uT += 3;
                            return new GotoStatement(bTUSUK_R9zP[tMh7uT - 2].kvJv6FdX0Wme2ZUXxy!);
                        }
                    case TokenKind.zCmKOyOPQ when bTUSUK_R9zP[tMh7uT + 1].UCclohAHjaXEJvAVi is TokenKind.VkPSg0:
                        {
                             tMh7uT += 2;
                            return new GotoDeclaration(bTUSUK_R9zP[tMh7uT - 2].kvJv6FdX0Wme2ZUXxy!);
                        }
                    case TokenKind.zCmKOyOPQ:
                        {
                            string tvLjFkzfFa9PYoTwdhO = bTUSUK_R9zP[tMh7uT].kvJv6FdX0Wme2ZUXxy!;

                            if (bTUSUK_R9zP[tMh7uT + 1].UCclohAHjaXEJvAVi is TokenKind.Saqsle32)
                            {
                                tMh7uT += 2;
                                var zBgs8nftvUtORz5x7k = new AssignmentStatement(tvLjFkzfFa9PYoTwdhO, XU2m8fJ4L());

                                if (bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi is not TokenKind.OAYy4uF5oPr25) throw new Exception($"Parser error, unexpected token: {bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi} expected: ?");
                                tMh7uT++;

                                return zBgs8nftvUtORz5x7k;
                            }

                            throw new Exception("Error in Parser, expected: =");
                        }

                    default: throw new Exception("Error in Parser, unknown token: " + bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi);
                }
            }

            throw new Exception("Parser error, unknown token");
        }

        public Expression XU2m8fJ4L(int YL8PJ_CiFj = 0)
        {
            Expression RbW6GlIJt;

            switch (bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi)
            {
                case TokenKind.px4P2OwC_TCS:
                case TokenKind.VpNNk81hH01O3w:
                    {
                        BinaryOp IU2nAxA = bTUSUK_R9zP[tMh7uT++].UCclohAHjaXEJvAVi switch
                        {
                            TokenKind.px4P2OwC_TCS => BinaryOp.px4P2OwC_TCS,
                            TokenKind.VpNNk81hH01O3w => BinaryOp.VpNNk81hH01O3w,
                            _ => throw new Exception("Unknown unary operator.")
                        };

                        RbW6GlIJt = new UnaryExpression(IU2nAxA, XU2m8fJ4L(100));
                        break;
                    }
                case TokenKind.JaFK4wy:
                    {
                        tMh7uT++; 
                        RbW6GlIJt = XU2m8fJ4L();

                        if (tMh7uT >= bTUSUK_R9zP.Count || bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi != TokenKind.mXkYfmMfcdhMK)
                        {
                            throw new Exception("Parser error expected )");
                        }

                        tMh7uT++;
                        break;
                    }
                case TokenKind.fkkQAsvha:
                    {    
                        RbW6GlIJt = new NumberExpression(int.Parse(bTUSUK_R9zP[tMh7uT++].kvJv6FdX0Wme2ZUXxy!), VariableType.QOa6p9YEuH1RAbA6O7q5);
                        break;
                    }
                case TokenKind.zCmKOyOPQ:
                    {
                        RbW6GlIJt = new VariableExpression(bTUSUK_R9zP[tMh7uT++].kvJv6FdX0Wme2ZUXxy!);
                        break;
                    }
                case TokenKind.VJ9YVMa9I:
                    {
                        RbW6GlIJt = new BoolExpression(false);
                        tMh7uT++;
                        break;
                    }
                case TokenKind.nwAN5fKImy9:
                    {
                        RbW6GlIJt = new BoolExpression(true);
                        tMh7uT++;
                        break;
                    }
                default:
                    throw new Exception("Error in Parser, unknown token: " + bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi);
            }

            while(tMh7uT < bTUSUK_R9zP.Count && bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi is not TokenKind.OAYy4uF5oPr25 and not TokenKind.mXkYfmMfcdhMK)
            {
                (var IU2nAxA, int gmePyXrybh3inCoffpW) = ftjYiwl5Vh();
                if (YL8PJ_CiFj > gmePyXrybh3inCoffpW) break;
                tMh7uT++;
                                                                
                RbW6GlIJt = new BinaryExpression(RbW6GlIJt, IU2nAxA, XU2m8fJ4L(++gmePyXrybh3inCoffpW));
            }

            return RbW6GlIJt;
        }

        private void iFxJgdC(TokenKind UCclohAHjaXEJvAVi)
        {
            if (bTUSUK_R9zP[++tMh7uT].UCclohAHjaXEJvAVi != UCclohAHjaXEJvAVi) throw new Exception($"Requires {UCclohAHjaXEJvAVi}");
        }
        private (BinaryOp, int) ftjYiwl5Vh() => bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi switch
        {
            TokenKind.px4P2OwC_TCS => (BinaryOp.px4P2OwC_TCS, 10),
            TokenKind.VpNNk81hH01O3w => (BinaryOp.VpNNk81hH01O3w, 10),
            TokenKind.XvjP9ZInqh1c6ky6nK_ => (BinaryOp.XvjP9ZInqh1c6ky6nK_, 50),
            TokenKind.HVwX8t15uMw86Zjrr => (BinaryOp.HVwX8t15uMw86Zjrr, 50),
            TokenKind.Dcm_6_V8BembLSz3Xvd => (BinaryOp.Dcm_6_V8BembLSz3Xvd, 50),

            TokenKind.i3e0fh1jq => (BinaryOp.i3e0fh1jq, 5),
            TokenKind.X0mFuD => (BinaryOp.RpVKAjlPn7QkY5, 5),
            TokenKind.wY8aKmHjGo => (BinaryOp.wY8aKmHjGo, 5),
            TokenKind.EVNnxDS7mt => (BinaryOp.yCtPKI, 5),
            TokenKind.Tieags2HzPi132WNHai => (BinaryOp.Tieags2HzPi132WNHai, 5),

            TokenKind.dFZBYFCUJWiZYreG5 => (BinaryOp.dFZBYFCUJWiZYreG5, 2),
            TokenKind.hJQNxCsE8 => (BinaryOp.hJQNxCsE8, 1),
            _ => throw new Exception("Unknown Operation")
        };
        public VariableType QKG8TsMd(TokenKind UCclohAHjaXEJvAVi) => UCclohAHjaXEJvAVi switch { 
            TokenKind.msEw5C9X6eO => VariableType.msEw5C9X6eO,
            TokenKind.QOa6p9YEuH1RAbA6O7q5 => VariableType.QOa6p9YEuH1RAbA6O7q5,
            _ => throw new Exception("Unknown type")
        };
    }
}
