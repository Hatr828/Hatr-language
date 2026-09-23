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
                        if (UHtIc5V2GaGHEr.VFweLKwblUFT45a5dYsf is null) return;

                        var FlHaV6FlTtLPOieCD = oZxpWPc(UHtIc5V2GaGHEr.VFweLKwblUFT45a5dYsf);
                        var A25qgAtLDu70v5Bnz5 = PAl_uyom48KJC(FlHaV6FlTtLPOieCD);
                        nhqaeri73.Add(UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq, A25qgAtLDu70v5Bnz5);
                        return;
                    }
                case BoundIfDeclaration R5E_HH:
                    {
                        IrValue _DjEicbSKt = oZxpWPc(R5E_HH._huqzi);
                        
                        string Tx31ppY4 = EoOIZ19mfP2QAQ("if");
                        string iZRm5CogkhHGChm = EoOIZ19mfP2QAQ("end");

                        xLbCDLgvWyiM_RS(new IrJumpIfTrue(_DjEicbSKt, Tx31ppY4));
                        xLbCDLgvWyiM_RS(new IrJump(iZRm5CogkhHGChm));
                        xLbCDLgvWyiM_RS(new IrLabel(Tx31ppY4));

                        foreach (var jn5gszuLQk in R5E_HH.x6w4y6YVMJqhHPCw)
                            Gr5ONL3BdMzHrR8nfZE(jn5gszuLQk);

                        xLbCDLgvWyiM_RS(new IrLabel(iZRm5CogkhHGChm));
                        return;
                    }
                case BoundGotoDeclaration eV5GbDX:
                        xLbCDLgvWyiM_RS(new IrLabel("user_" + eV5GbDX.DIb6al9mN)); return;
                case BoundGotoStatement eV5GbDX:
                        xLbCDLgvWyiM_RS(new IrJump("user_" + eV5GbDX.K_1l6XsfuG9)); return;
                case BoundAssignmentStatement Ya3KuoBYDAJuy5LuF:
                    {
                        var A25qgAtLDu70v5Bnz5 = _ZnHZc_yO(Ya3KuoBYDAJuy5LuF.RDvxaYCKYSRSBjVq);
                        var u7ogbwL69cdM = oZxpWPc(Ya3KuoBYDAJuy5LuF.Kqh0aRU);

                        xLbCDLgvWyiM_RS(new IrVariable(A25qgAtLDu70v5Bnz5, u7ogbwL69cdM));
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
                    return S4GWwj(I7ZI3fRSU.Sb5MrQFjg8JlEcaS);
                case BoundBoolExpression FHZW5YiKBaTQMYfb:
                    return S4GWwj(FHZW5YiKBaTQMYfb.Sb5MrQFjg8JlEcaS ? 1 : 0);
                case BoundVariableExpression UHtIc5V2GaGHEr:
                    return _ZnHZc_yO(UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq);
                case BoundBinaryExpression XxC0ynDET:
                    {
                        var RbW6GlIJt = oZxpWPc(XxC0ynDET.bRC8MHJu);
                        var A25qgAtLDu70v5Bnz5 = PAl_uyom48KJC(RbW6GlIJt);
                        var h3s7dbXXv22CfCfn = oZxpWPc(XxC0ynDET.Kqh0aRU);

                        xLbCDLgvWyiM_RS(new IrBinary(XxC0ynDET.mfIdTBeeCV, A25qgAtLDu70v5Bnz5, h3s7dbXXv22CfCfn));

                        return A25qgAtLDu70v5Bnz5;
                    }
                case BoundUnaryExpression xi54PsDn5M889REv3:
                    {
                        var h3s7dbXXv22CfCfn = oZxpWPc(xi54PsDn5M889REv3.Kqh0aRU);
                        var A25qgAtLDu70v5Bnz5 = PAl_uyom48KJC(h3s7dbXXv22CfCfn);

                        xLbCDLgvWyiM_RS(new IrUnary(xi54PsDn5M889REv3.mfIdTBeeCV, A25qgAtLDu70v5Bnz5));

                        return A25qgAtLDu70v5Bnz5;
                    }
            }
            throw new Exception("Cannot lower: " + zvo1A5gjZ.GetType());
        }
        private IrValue S4GWwj(long FlHaV6FlTtLPOieCD)
        {
            var f6P5tymwJzH = HE1TYQqtHG7Mcr96();
            xLbCDLgvWyiM_RS(new IrConstant(f6P5tymwJzH, FlHaV6FlTtLPOieCD));
            return f6P5tymwJzH;
        }
        private IrValue PAl_uyom48KJC(IrValue u7ogbwL69cdM)
        {
            var f6P5tymwJzH = HE1TYQqtHG7Mcr96();
            xLbCDLgvWyiM_RS(new IrVariable(f6P5tymwJzH, u7ogbwL69cdM));
            return f6P5tymwJzH;
        }

        private void xLbCDLgvWyiM_RS(IrInstruction gkfVuQR)
        {
            ZxlPuhy9nKlj_24.NumXpBuXze5D.Add(gkfVuQR);
        }
        private IrValue _ZnHZc_yO(string tvLjFkzfFa9PYoTwdhO)
        {
            if (nhqaeri73.TryGetValue(tvLjFkzfFa9PYoTwdhO, out var FlHaV6FlTtLPOieCD))
                return FlHaV6FlTtLPOieCD;

            throw new InvalidOperationException($"Unknown variable: '{tvLjFkzfFa9PYoTwdhO}'.");
        }
    }
}
