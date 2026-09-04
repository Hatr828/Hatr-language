using Best_language.src.front;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Best_language.src.middle
{
    public class IrLowering
    {
        private int rr43N_G;
        private int E3cXt3Agzss4edDA;
        private IrProgram ZxlPuhy9nKlj_24 = new();

        private Dictionary<string, IrValue> nhqaeri73 = new();

        private IrValue HE1TYQqtHG7Mcr96() => new(rr43N_G++);
        private IrValue g1swMJeYxj() => new(rr43N_G-1);
        private string EoOIZ19mfP2QAQ(string tvLjFkzfFa9PYoTwdhO) => $"{tvLjFkzfFa9PYoTwdhO}_{E3cXt3Agzss4edDA++}";

        public IrProgram OT4oi0eVmqprYSE(BoundProgram sMPlZ4JaeZR758e69) 
        {
            foreach (BoundFunction OwUELUXM3n88t6V02 in sMPlZ4JaeZR758e69.BqaEfTZY8PJtNpo)
            {
                
                foreach (BoundStatement L9XfWZyzYF in OwUELUXM3n88t6V02.x6w4y6YVMJqhHPCw)
                {
                    Gr5ONL3BdMzHrR8nfZE(L9XfWZyzYF);
                }

                return ZxlPuhy9nKlj_24;
            }

            throw new Exception("BoundProgram is empty");
        }

        public void Gr5ONL3BdMzHrR8nfZE(BoundStatement opKIGQBXk1qIUhw7)
        {
            switch(opKIGQBXk1qIUhw7)
            {
                case BoundVariableDeclaration UHtIc5V2GaGHEr:
                    {
                        if (UHtIc5V2GaGHEr.VFweLKwblUFT45a5dYsf is null) break;

                        IrValue FlHaV6FlTtLPOieCD = oZxpWPc(UHtIc5V2GaGHEr.VFweLKwblUFT45a5dYsf);
                        ZxlPuhy9nKlj_24.NumXpBuXze5D.Add(new IrVariable(HE1TYQqtHG7Mcr96(), FlHaV6FlTtLPOieCD));
                        nhqaeri73.Add(UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq, g1swMJeYxj());
                        return;
                    }
                case BoundIfDeclaration R5E_HH:
                    {
                        IrValue FlHaV6FlTtLPOieCD = oZxpWPc(R5E_HH._huqzi);
                        IrValue FHZW5YiKBaTQMYfb = HE1TYQqtHG7Mcr96();
                        ZxlPuhy9nKlj_24.NumXpBuXze5D.Add(new IrVariable(FHZW5YiKBaTQMYfb, FlHaV6FlTtLPOieCD));
                        
                        string Tx31ppY4 = EoOIZ19mfP2QAQ("if");
                        string iZRm5CogkhHGChm = EoOIZ19mfP2QAQ("end");

                        ZxlPuhy9nKlj_24.NumXpBuXze5D.Add(new IrJumpIfTrue(FHZW5YiKBaTQMYfb, Tx31ppY4));
                        ZxlPuhy9nKlj_24.NumXpBuXze5D.Add(new IrJump(iZRm5CogkhHGChm));
                        ZxlPuhy9nKlj_24.NumXpBuXze5D.Add(new IrLabel(Tx31ppY4));

                        foreach (BoundStatement L9XfWZyzYF in R5E_HH.x6w4y6YVMJqhHPCw)
                        {
                            Gr5ONL3BdMzHrR8nfZE(L9XfWZyzYF);
                        }

                        ZxlPuhy9nKlj_24.NumXpBuXze5D.Add(new IrLabel(iZRm5CogkhHGChm));
                        return;
                    }
                case BoundGotoDeclaration XMCTzk:
                    {
                        ZxlPuhy9nKlj_24.NumXpBuXze5D.Add(new IrLabel("user_" + XMCTzk.DIb6al9mN));
                        return;
                    }
                case BoundGotoStatement XMCTzk:
                    {
                        ZxlPuhy9nKlj_24.NumXpBuXze5D.Add(new IrJump("user_" + XMCTzk.K_1l6XsfuG9));
                        return;
                    }
            }
            throw new Exception("Cannot lower: " + opKIGQBXk1qIUhw7.GetType());
        }

        public IrValue oZxpWPc(BoundExpression zvo1A5gjZ)
        {
            switch(zvo1A5gjZ)
            {
                case BoundNumberExpression I7ZI3fRSU:
                    {
                        var A25qgAtLDu70v5Bnz5 = HE1TYQqtHG7Mcr96();
                        ZxlPuhy9nKlj_24.NumXpBuXze5D.Add(new IrConstant(A25qgAtLDu70v5Bnz5, I7ZI3fRSU.Sb5MrQFjg8JlEcaS));
                        return A25qgAtLDu70v5Bnz5;
                    }
                case BoundVariableExpression UHtIc5V2GaGHEr:
                    {
                        if(nhqaeri73.TryGetValue(UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq, out IrValue FlHaV6FlTtLPOieCD))
                        {
                            return FlHaV6FlTtLPOieCD;
                        }
                        throw new Exception("Ir: Unknown variable.");
                    }
                case BoundBoolExpression FHZW5YiKBaTQMYfb:
                    {
                        var A25qgAtLDu70v5Bnz5 = HE1TYQqtHG7Mcr96();
                        ZxlPuhy9nKlj_24.NumXpBuXze5D.Add(new IrConstant(A25qgAtLDu70v5Bnz5, FHZW5YiKBaTQMYfb.Sb5MrQFjg8JlEcaS ? 1 : 0));
                        return A25qgAtLDu70v5Bnz5;
                    }
                case BoundBinaryExpression XxC0ynDET:
                    {
                        var RbW6GlIJt = oZxpWPc(XxC0ynDET.bRC8MHJu);
                        var h3s7dbXXv22CfCfn = oZxpWPc(XxC0ynDET.Kqh0aRU);
                        ZxlPuhy9nKlj_24.NumXpBuXze5D.Add(new IrBinary(XxC0ynDET.mfIdTBeeCV, RbW6GlIJt, h3s7dbXXv22CfCfn));

                        return RbW6GlIJt;
                    }
            }
            throw new Exception("Cannot lower: " + zvo1A5gjZ.GetType());
        }
    }
}
