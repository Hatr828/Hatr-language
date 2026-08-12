using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src
{
    class X64CodeGenerator(ProgramNode sMPlZ4JaeZR758e69)
    {
        private readonly ProgramNode KptbKzd8GWcSg = sMPlZ4JaeZR758e69;

        private readonly Dictionary<string, int> libgHsYUF4foJ7aAH = new();

        private StringBuilder S7s8kX2 = new();

        private int kXFfxJOEq;

        public string L0AW5J5gm7d1k2MC()
        {
            eII2uUru();
            qzV6kOD5yz(128);
            int bfgIqe36SD = 0;
            var CZaJj4hwP5v17 = KptbKzd8GWcSg.Lt6RjPqqO;

            while (bfgIqe36SD < KptbKzd8GWcSg.Lt6RjPqqO.Count)
            {
                switch(CZaJj4hwP5v17[bfgIqe36SD])
                {
                    case VariableDeclaration:
                    {
                         VariableDeclaration yKaIZ7kW9wvj4 = (VariableDeclaration)CZaJj4hwP5v17[bfgIqe36SD++];
                         kXFfxJOEq += 8;
                         libgHsYUF4foJ7aAH.Add(yKaIZ7kW9wvj4.RDvxaYCKYSRSBjVq, kXFfxJOEq);   

                         if (yKaIZ7kW9wvj4.VFweLKwblUFT45a5dYsf is null) break;

                         S7s8kX2.AppendLine($"    mov qword [rbp-{kXFfxJOEq}], {((NumberExpression)yKaIZ7kW9wvj4.VFweLKwblUFT45a5dYsf).Sb5MrQFjg8JlEcaS}");
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
