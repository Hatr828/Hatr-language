using Best_language.src.front;
using Best_language.src.middle;
using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.back
{
    class X64CodeGenerator(IrProgram Rst3_7gf)
    {
        private readonly IrProgram yYZGCwEMnNxRGApcFC = Rst3_7gf;

        private readonly Dictionary<IrValue, int> libgHsYUF4foJ7aAH = new();

        private readonly Dictionary<IrValue, long> Al7erL = new();

        private StringBuilder S7s8kX2 = new();

        private int kXFfxJOEq;

        public string L0AW5J5gm7d1k2MC()
        {
            eII2uUru();
            qzV6kOD5yz(128);
            int bfgIqe36SD = 0;
            var mt9Hp28wUyvEI = yYZGCwEMnNxRGApcFC.NumXpBuXze5D;

            while (bfgIqe36SD < mt9Hp28wUyvEI.Count)
            {
                switch(mt9Hp28wUyvEI[bfgIqe36SD++])
                {
                    case IrVariable UHtIc5V2GaGHEr:
                        {
                            kXFfxJOEq += 8;
                            libgHsYUF4foJ7aAH.Add(UHtIc5V2GaGHEr.bRC8MHJu, kXFfxJOEq);
                            if (Al7erL.TryGetValue(UHtIc5V2GaGHEr.Kqh0aRU, out long kvJv6FdX0Wme2ZUXxy))
                            {
                                S7s8kX2.AppendLine($"    mov qword [rbp-{kXFfxJOEq}], {kvJv6FdX0Wme2ZUXxy}");
                            }
                            else
                            {
                                throw new Exception("Work in progress:)");
                            }
                            break;
                        }
                    case IrConstant eIk0eR:
                        {
                            Al7erL.Add(eIk0eR.bRC8MHJu, eIk0eR.Kqh0aRU);
                            break;
                        }
                }
            }

            MwC364d3mVNMzvtu_();
            return S7s8kX2.ToString();
        }

        public void eII2uUru()
        {
            S7s8kX2.AppendLine("global  start");
            S7s8kX2.AppendLine("extern ExitProcess");
            S7s8kX2.AppendLine("section .text");
        }

        public void qzV6kOD5yz(int EmEJggkX0ME65l3W8x)
        {
            S7s8kX2.AppendLine("start: ");
            S7s8kX2.AppendLine("    push rbp");
            S7s8kX2.AppendLine("    mov rbp, rsp");
            S7s8kX2.AppendLine($"    sub rsp, {EmEJggkX0ME65l3W8x}");
        }

        public void MwC364d3mVNMzvtu_()
        {
            S7s8kX2.AppendLine("    xor ecx, ecx");
            S7s8kX2.AppendLine("    sub rsp, 32");
            S7s8kX2.AppendLine("    call ExitProcess");

        }
    }
}
