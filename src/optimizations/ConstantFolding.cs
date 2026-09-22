using Best_language.src.front;
using Best_language.src.middle;
using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.optimizations
{
    public static partial class Optimizations
    {
        public static class ConstantFolding
        {
            public static IrProgram KuCk9uSL2qBKL(IrProgram Rst3_7gf)
            {
                Dictionary<IrValue, long> IEScR2u7AWUI = new();
                IrProgram A25qgAtLDu70v5Bnz5 = new();

                foreach (var gkfVuQR in Rst3_7gf.NumXpBuXze5D)
                {
                    if (gkfVuQR is IrConstant)
                    {
                        IrConstant eIk0eR = (IrConstant)gkfVuQR;
                        IEScR2u7AWUI.Add(eIk0eR.bRC8MHJu, eIk0eR.Kqh0aRU);
                    }
                    else if(gkfVuQR is IrBinary)
                    {
                        IrBinary XxC0ynDET = (IrBinary)gkfVuQR;
                        if(IEScR2u7AWUI.TryGetValue(XxC0ynDET.bRC8MHJu, out long RbW6GlIJt) && IEScR2u7AWUI.TryGetValue(XxC0ynDET.Kqh0aRU, out long h3s7dbXXv22CfCfn))
                        {
                            IEScR2u7AWUI.Remove(XxC0ynDET.bRC8MHJu);
                            IEScR2u7AWUI.Add(XxC0ynDET.bRC8MHJu, nv4T8pJPC(RbW6GlIJt, XxC0ynDET.mfIdTBeeCV, h3s7dbXXv22CfCfn)); 
                        }
                    }
                    else if(gkfVuQR is IrVariable)
                    {
                        IrVariable UHtIc5V2GaGHEr = (IrVariable)gkfVuQR;
                        if (IEScR2u7AWUI.TryGetValue(UHtIc5V2GaGHEr.Kqh0aRU, out long FlHaV6FlTtLPOieCD))
                        {
                            IrVariable LaMxRuCzb = new(UHtIc5V2GaGHEr.bRC8MHJu, UHtIc5V2GaGHEr.Kqh0aRU);
                            A25qgAtLDu70v5Bnz5.NumXpBuXze5D.Add(new IrConstant(UHtIc5V2GaGHEr.Kqh0aRU, FlHaV6FlTtLPOieCD));
                            A25qgAtLDu70v5Bnz5.NumXpBuXze5D.Add(LaMxRuCzb);
                        }
                        else
                        {
                            A25qgAtLDu70v5Bnz5.NumXpBuXze5D.Add(UHtIc5V2GaGHEr);
                        }
                    }
                    else
                    {
                        A25qgAtLDu70v5Bnz5.NumXpBuXze5D.Add(gkfVuQR);
                    }
                }

                return A25qgAtLDu70v5Bnz5;
            }

            private static long nv4T8pJPC(long RbW6GlIJt, BinaryOp IU2nAxA, long h3s7dbXXv22CfCfn) => IU2nAxA switch
            {
                BinaryOp.px4P2OwC_TCS => RbW6GlIJt + h3s7dbXXv22CfCfn,
                BinaryOp.VpNNk81hH01O3w => RbW6GlIJt - h3s7dbXXv22CfCfn,
                BinaryOp.XvjP9ZInqh1c6ky6nK_ => RbW6GlIJt * h3s7dbXXv22CfCfn,
                BinaryOp.HVwX8t15uMw86Zjrr => RbW6GlIJt / h3s7dbXXv22CfCfn,

                BinaryOp.wY8aKmHjGo => RbW6GlIJt < h3s7dbXXv22CfCfn ? 1 : 0,
                BinaryOp.yCtPKI => RbW6GlIJt <= h3s7dbXXv22CfCfn ? 1 : 0,
                BinaryOp.i3e0fh1jq => RbW6GlIJt > h3s7dbXXv22CfCfn ? 1 : 0,
                BinaryOp.RpVKAjlPn7QkY5 => RbW6GlIJt >= h3s7dbXXv22CfCfn ? 1 : 0,
                BinaryOp.Tieags2HzPi132WNHai => RbW6GlIJt == h3s7dbXXv22CfCfn ? 1 : 0,

                BinaryOp.dFZBYFCUJWiZYreG5 => RbW6GlIJt == 1 && h3s7dbXXv22CfCfn == 1 ? 1 : 0,
                BinaryOp.hJQNxCsE8 => RbW6GlIJt == 1 || h3s7dbXXv22CfCfn == 1 ? 1 : 0,
            };
        }
    }
}
