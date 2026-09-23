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
                            var FubV605Z9qF5D9z3X = bTUSUK_R9zP[tMh7uT++].UCclohAHjaXEJvAVi;
                            iFxJgdC(TokenKind.zCmKOyOPQ);
                            if (bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi is TokenKind.Saqsle32)
                            {
                                tMh7uT++;
                                var zBgs8nftvUtORz5x7k = new VariableDeclaration(bTUSUK_R9zP[tMh7uT - 2].kvJv6FdX0Wme2ZUXxy!, QKG8TsMd(FubV605Z9qF5D9z3X), XU2m8fJ4L());

                                iFxJgdC(TokenKind.OAYy4uF5oPr25);

                                return zBgs8nftvUtORz5x7k;
                            }
                            else if (bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi is TokenKind.OAYy4uF5oPr25)
                            {
                                tMh7uT++;
                                return new VariableDeclaration(bTUSUK_R9zP[tMh7uT - 2].kvJv6FdX0Wme2ZUXxy!, VariableType.QOa6p9YEuH1RAbA6O7q5);
                            }
                            break;
                        }
                    case TokenKind.R5E_HH:
                        {
                            tMh7uT++;
                            iFxJgdC(TokenKind.JaFK4wy);

                            Expression FHZW5YiKBaTQMYfb = XU2m8fJ4L();
                            List<AstNode> zk9bMID2GlAo = new();

                            iFxJgdC(TokenKind.mXkYfmMfcdhMK);
                            iFxJgdC(TokenKind.xQOQMX7lMt2mn);

                            while (bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi is not TokenKind.XmL8EZAP1m8r4dI9tNJ)
                            {
                                zk9bMID2GlAo.Add(_8CvQhVY3OxAzmhbj());
                            }

                            tMh7uT++;
                            return new IfDeclaration(FHZW5YiKBaTQMYfb, zk9bMID2GlAo);
                        }
                    case TokenKind.lgNzh3UAydkfeRDf2Cu:
                        {
                            tMh7uT++;
                            iFxJgdC(TokenKind.zCmKOyOPQ);
                            iFxJgdC(TokenKind.OAYy4uF5oPr25);
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

                            if (bTUSUK_R9zP[++tMh7uT].UCclohAHjaXEJvAVi is TokenKind.Saqsle32)
                            {
                                tMh7uT++;
                                var zBgs8nftvUtORz5x7k = new AssignmentStatement(tvLjFkzfFa9PYoTwdhO, null, XU2m8fJ4L());

                                iFxJgdC(TokenKind.OAYy4uF5oPr25);

                                return zBgs8nftvUtORz5x7k;
                            }
                            var IU2nAxA = _jlUqzof(); tMh7uT++;
                            var znqK8sSSbcNNHX98uY1 = new AssignmentStatement(tvLjFkzfFa9PYoTwdhO, IU2nAxA, XU2m8fJ4L());

                            iFxJgdC(TokenKind.OAYy4uF5oPr25);
                            return znqK8sSSbcNNHX98uY1;
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
                case TokenKind.AXJEUF8:
                case TokenKind.V0oF1_Ia:
                case TokenKind.LCF6mrBw1SEACqszM0:
                case TokenKind.FTFUgi4mZsX8Vyt:
                {
                        Operations IU2nAxA = bTUSUK_R9zP[tMh7uT++].UCclohAHjaXEJvAVi switch
                        {
                            TokenKind.px4P2OwC_TCS => Operations.px4P2OwC_TCS,
                            TokenKind.VpNNk81hH01O3w => Operations.VpNNk81hH01O3w,
                            TokenKind.FTFUgi4mZsX8Vyt => Operations.o1eEDaDB1HpqMTgNI,
                            TokenKind.LCF6mrBw1SEACqszM0 => Operations.tmDJZxxjN8YO6gXg,
                            TokenKind.V0oF1_Ia => Operations.V0oF1_Ia,
                            TokenKind.AXJEUF8 => Operations.AXJEUF8,
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
                if (bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi is TokenKind.LCF6mrBw1SEACqszM0 or TokenKind.FTFUgi4mZsX8Vyt)
                {
                    var ZOPdtklYsstgjwAX = bTUSUK_R9zP[tMh7uT++].UCclohAHjaXEJvAVi switch
                    {
                        TokenKind.LCF6mrBw1SEACqszM0 => Operations.jeuvS6KLpYeMA43v9,
                        TokenKind.FTFUgi4mZsX8Vyt => Operations.SaZ45v4KVaHn,
                        _ => throw new Exception()
                    };

                    RbW6GlIJt = new UnaryExpression(ZOPdtklYsstgjwAX, RbW6GlIJt);
                    continue;
                }

                (var IU2nAxA, int gmePyXrybh3inCoffpW) = ftjYiwl5Vh();
                if (YL8PJ_CiFj > gmePyXrybh3inCoffpW) break;
                tMh7uT++;
                                                                
                RbW6GlIJt = new BinaryExpression(RbW6GlIJt, IU2nAxA, XU2m8fJ4L(++gmePyXrybh3inCoffpW));
            }

            return RbW6GlIJt;
        }

        private void iFxJgdC(TokenKind UCclohAHjaXEJvAVi)
        {
            if (bTUSUK_R9zP[tMh7uT++].UCclohAHjaXEJvAVi != UCclohAHjaXEJvAVi) throw new Exception($"Requires {UCclohAHjaXEJvAVi}");
        }
        private (Operations, int) ftjYiwl5Vh() => bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi switch
        {
            TokenKind.XvjP9ZInqh1c6ky6nK_ => (Operations.XvjP9ZInqh1c6ky6nK_, 70),
            TokenKind.HVwX8t15uMw86Zjrr => (Operations.HVwX8t15uMw86Zjrr, 70),
            TokenKind.Dcm_6_V8BembLSz3Xvd => (Operations.Dcm_6_V8BembLSz3Xvd, 70),

            TokenKind.px4P2OwC_TCS => (Operations.px4P2OwC_TCS, 60),
            TokenKind.VpNNk81hH01O3w => (Operations.VpNNk81hH01O3w, 60),

            TokenKind.glXuoibfOZtI7SBkfT => (Operations.glXuoibfOZtI7SBkfT, 50),
            TokenKind.FsWeBIKdwX => (Operations.FsWeBIKdwX, 50),

            TokenKind.i3e0fh1jq => (Operations.i3e0fh1jq, 40),
            TokenKind.X0mFuD => (Operations.RpVKAjlPn7QkY5, 40),
            TokenKind.wY8aKmHjGo => (Operations.wY8aKmHjGo, 40),
            TokenKind.EVNnxDS7mt => (Operations.yCtPKI, 40),

            TokenKind.Tieags2HzPi132WNHai => (Operations.Tieags2HzPi132WNHai, 30),
            TokenKind.ggA0QEoXbT2g42XV => (Operations.ggA0QEoXbT2g42XV, 30),

            TokenKind.xyQlFLPMo74jLnujD => (Operations.xyQlFLPMo74jLnujD, 25),
            TokenKind.tOhkYK403y => (Operations.tOhkYK403y, 20),
            TokenKind.LoG1nbZ0fIkPU => (Operations.LoG1nbZ0fIkPU, 15),

            TokenKind.dFZBYFCUJWiZYreG5 => (Operations.dFZBYFCUJWiZYreG5, 10),
            TokenKind.hJQNxCsE8 => (Operations.hJQNxCsE8, 5),

            _ => throw new Exception("Unknown Operation")
        };

        private Operations _jlUqzof() => bTUSUK_R9zP[tMh7uT].UCclohAHjaXEJvAVi switch
        {
            TokenKind.yzkCqYShh => Operations.yzkCqYShh,
            TokenKind.cJcVwmWsoiUI => Operations.cJcVwmWsoiUI,
            TokenKind.pe3_jLc => Operations.pe3_jLc,
            TokenKind.bEbHtL6r_nWiA4 => Operations.bEbHtL6r_nWiA4,
            TokenKind.SKz7ydBz => Operations.SKz7ydBz,

            TokenKind.JYW1ZlxYZddYZFQz => Operations.JYW1ZlxYZddYZFQz,
            TokenKind.Jl_g4z_tM6N7fZ6 => Operations.Jl_g4z_tM6N7fZ6,
            TokenKind.BiHitvb7n3O1 => Operations.BiHitvb7n3O1,

            TokenKind.Y51ndI1fZSk8I => Operations.Y51ndI1fZSk8I,
            TokenKind.w2UzTMSp9o0T => Operations.w2UzTMSp9o0T,

            _ => throw new Exception("Unknown Operation")
        };
        public VariableType QKG8TsMd(TokenKind UCclohAHjaXEJvAVi) => UCclohAHjaXEJvAVi switch { 
            TokenKind.msEw5C9X6eO => VariableType.msEw5C9X6eO,
            TokenKind.QOa6p9YEuH1RAbA6O7q5 => VariableType.QOa6p9YEuH1RAbA6O7q5,
            _ => throw new Exception("Unknown type")
        };
    }
}
