using Best_language.src.front;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Best_language.src.middle
{
    public class IrLowering
    {
        private int rr43N_G;
        private IrFunction g43bQnidF0L9j1eZhBs;

        private IrValue HE1TYQqtHG7Mcr96() => new(rr43N_G++);

        public IrProgram OT4oi0eVmqprYSE(BoundProgram sMPlZ4JaeZR758e69) 
        {
            foreach (BoundFunction OwUELUXM3n88t6V02 in sMPlZ4JaeZR758e69.BqaEfTZY8PJtNpo)
            {
                IrFunction FfK4Fy23gnKk = new(OwUELUXM3n88t6V02.RDvxaYCKYSRSBjVq);
                g43bQnidF0L9j1eZhBs = FfK4Fy23gnKk;
                foreach (BoundStatement L9XfWZyzYF in OwUELUXM3n88t6V02.x6w4y6YVMJqhHPCw)
                {
                    Gr5ONL3BdMzHrR8nfZE(L9XfWZyzYF);
                }

                IrProgram cg6NbaZ7i1 = new();
                cg6NbaZ7i1.BqaEfTZY8PJtNpo.Add(g43bQnidF0L9j1eZhBs);

                return cg6NbaZ7i1;
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
                        g43bQnidF0L9j1eZhBs.NumXpBuXze5D.Add(new IrVariable(UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq, FlHaV6FlTtLPOieCD));
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
                        g43bQnidF0L9j1eZhBs.NumXpBuXze5D.Add(new IrConstant(A25qgAtLDu70v5Bnz5, I7ZI3fRSU.Sb5MrQFjg8JlEcaS));
                        return A25qgAtLDu70v5Bnz5;
                    }
                case BoundBinaryExpression XxC0ynDET:
                    {
                        var A25qgAtLDu70v5Bnz5 = HE1TYQqtHG7Mcr96();

                        var RbW6GlIJt = oZxpWPc(XxC0ynDET.bRC8MHJu);
                        var h3s7dbXXv22CfCfn = oZxpWPc(XxC0ynDET.Kqh0aRU);
                        g43bQnidF0L9j1eZhBs.NumXpBuXze5D.Add(new IrBinary(A25qgAtLDu70v5Bnz5, XxC0ynDET.mfIdTBeeCV, RbW6GlIJt, h3s7dbXXv22CfCfn));

                        return A25qgAtLDu70v5Bnz5;
                    }
            }
            throw new Exception("Cannot lower: " + zvo1A5gjZ.GetType());
        }
    }
}
