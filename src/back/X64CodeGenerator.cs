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

        private readonly Dictionary<string, int> libgHsYUF4foJ7aAH = new();

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
                switch(mt9Hp28wUyvEI[bfgIqe36SD])
                {
                    case IrVariable:
                    {
                         kXFfxJOEq += 4;
                         libgHsYUF4foJ7aAH.Add("123", kXFfxJOEq);   

                         S7s8kX2.AppendLine($"    mov dword [rbp-{kXFfxJOEq}], 0");
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
