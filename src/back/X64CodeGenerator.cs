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

        private List<IrInstruction>? mt9Hp28wUyvEI;

        private int bfgIqe36SD;

        public string L0AW5J5gm7d1k2MC()
        {
            eII2uUru();
            qzV6kOD5yz();
            bfgIqe36SD = 0;
            mt9Hp28wUyvEI = yYZGCwEMnNxRGApcFC.NumXpBuXze5D;

            while (bfgIqe36SD < mt9Hp28wUyvEI.Count)
            {
                switch(mt9Hp28wUyvEI[bfgIqe36SD++])
                {
                    case IrBinary XxC0ynDET:
                        {
                            fK2NV8z6FbtMILjRf(XxC0ynDET);

                            int S7FJ7HAuO2 = J32YhIfpRYQ(XxC0ynDET.bRC8MHJu);
                            Al7erL.Remove(XxC0ynDET.bRC8MHJu);

                            S7s8kX2.AppendLine($"    mov qword [rbp-{S7FJ7HAuO2}], rax");
                            break;
                        }
                    case IrUnary xi54PsDn5M889REv3:
                        {
                            if (xi54PsDn5M889REv3.mfIdTBeeCV is Operations.px4P2OwC_TCS) break;
                            PouZtZpLKPKu8(xi54PsDn5M889REv3);

                            int S7FJ7HAuO2 = J32YhIfpRYQ(xi54PsDn5M889REv3.Kqh0aRU);
                            Al7erL.Remove(xi54PsDn5M889REv3.Kqh0aRU);

                            S7s8kX2.AppendLine($"    mov qword [rbp-{S7FJ7HAuO2}], rax");
                            break;
                        }
                    case IrVariable UHtIc5V2GaGHEr:
                        {
                            wBv0lHH3Pgrpf(UHtIc5V2GaGHEr.Kqh0aRU, "rax");

                            int S7FJ7HAuO2 = J32YhIfpRYQ(UHtIc5V2GaGHEr.bRC8MHJu);
                            Al7erL.Remove(UHtIc5V2GaGHEr.bRC8MHJu);

                            S7s8kX2.AppendLine($"    mov qword [rbp-{S7FJ7HAuO2}], rax");
                            break;
                        }
                    case IrConstant eIk0eR:
                        {
                            Al7erL.Add(eIk0eR.bRC8MHJu, eIk0eR.Kqh0aRU);
                            break;
                        }
                    case IrLabel Tx31ppY4:
                        {
                            S7s8kX2.AppendLine($"{Tx31ppY4.RDvxaYCKYSRSBjVq}:");
                            break;
                        }
                    case IrJump KjirbwqrRKk6IgTqZH:
                        {
                            S7s8kX2.AppendLine($"    jmp {KjirbwqrRKk6IgTqZH.Bbopqy}");
                            break;
                        }
                    case IrJumpIfTrue KjirbwqrRKk6IgTqZH:
                        {
                            wBv0lHH3Pgrpf(KjirbwqrRKk6IgTqZH.u6xbke, "rax");
                            S7s8kX2.AppendLine("    test rax, rax");
                            S7s8kX2.AppendLine($"    jnz {KjirbwqrRKk6IgTqZH.Bbopqy}");
                            break;
                        }
                }
            }

            MwC364d3mVNMzvtu_();

            int EmEJggkX0ME65l3W8x = (kXFfxJOEq + 15) & ~15;
            S7s8kX2.Replace("STACK_SIZE", EmEJggkX0ME65l3W8x.ToString());

            return S7s8kX2.ToString();
        }
        private void fK2NV8z6FbtMILjRf(IrBinary XxC0ynDET)
        {
            wBv0lHH3Pgrpf(XxC0ynDET.bRC8MHJu, "rax");
            wBv0lHH3Pgrpf(XxC0ynDET.Kqh0aRU, "rcx");

            switch (XxC0ynDET.mfIdTBeeCV)
            {
                case Operations.yzkCqYShh:
                case Operations.px4P2OwC_TCS:
                    S7s8kX2.AppendLine("    add rax, rcx");
                    break;

                case Operations.cJcVwmWsoiUI:
                case Operations.VpNNk81hH01O3w:
                    S7s8kX2.AppendLine("    sub rax, rcx");
                    break;

                case Operations.pe3_jLc:
                case Operations.XvjP9ZInqh1c6ky6nK_:
                    S7s8kX2.AppendLine("    imul rax, rcx");
                    break;

                case Operations.bEbHtL6r_nWiA4:
                case Operations.HVwX8t15uMw86Zjrr:
                    S7s8kX2.AppendLine("    cqo");
                    S7s8kX2.AppendLine("    idiv rcx");
                    break;

                case Operations.SKz7ydBz:
                case Operations.Dcm_6_V8BembLSz3Xvd:
                    S7s8kX2.AppendLine("    cqo");
                    S7s8kX2.AppendLine("    idiv rcx");
                    S7s8kX2.AppendLine("    mov rax, rdx");
                    break;

                case Operations.i3e0fh1jq:
                    xY0C58HW("setg");
                    break;

                case Operations.RpVKAjlPn7QkY5:
                    xY0C58HW("setge");
                    break;

                case Operations.wY8aKmHjGo:
                    xY0C58HW("setl");
                    break;

                case Operations.yCtPKI:
                    xY0C58HW("setle");
                    break;

                case Operations.Tieags2HzPi132WNHai:
                    xY0C58HW("sete");
                    break;

                case Operations.ggA0QEoXbT2g42XV:
                    xY0C58HW("setne");
                    break;

                case Operations.dFZBYFCUJWiZYreG5:
                    S7s8kX2.AppendLine("    test rax, rax");
                    S7s8kX2.AppendLine("    setne al");
                    S7s8kX2.AppendLine("    test rcx, rcx");
                    S7s8kX2.AppendLine("    setne cl");
                    S7s8kX2.AppendLine("    and al, cl");
                    S7s8kX2.AppendLine("    movzx rax, al");
                    break;

                case Operations.hJQNxCsE8:
                    S7s8kX2.AppendLine("    or rax, rcx");
                    S7s8kX2.AppendLine("    setne al");
                    S7s8kX2.AppendLine("    movzx rax, al");
                    break;

                case Operations.JYW1ZlxYZddYZFQz:
                case Operations.xyQlFLPMo74jLnujD:
                    S7s8kX2.AppendLine("    and rax, rcx");
                    break;

                case Operations.Jl_g4z_tM6N7fZ6:
                case Operations.LoG1nbZ0fIkPU:
                    S7s8kX2.AppendLine("    or rax, rcx");
                    break;

                case Operations.BiHitvb7n3O1:
                case Operations.tOhkYK403y:
                    S7s8kX2.AppendLine("    xor rax, rcx");
                    break;

                case Operations.w2UzTMSp9o0T:
                case Operations.glXuoibfOZtI7SBkfT:
                    S7s8kX2.AppendLine("    shl rax, cl");
                    break;

                case Operations.Y51ndI1fZSk8I:
                case Operations.FsWeBIKdwX:
                    S7s8kX2.AppendLine("    sar rax, cl");
                    break;

                default:
                    throw new NotImplementedException($"Unsupported binary operation: {XxC0ynDET.mfIdTBeeCV}");
            }
        }
        private void PouZtZpLKPKu8(IrUnary XxC0ynDET)
        {
            wBv0lHH3Pgrpf(XxC0ynDET.Kqh0aRU, "rax");

            switch (XxC0ynDET.mfIdTBeeCV)
            {
                case Operations.VpNNk81hH01O3w:
                    S7s8kX2.AppendLine("    neg rax");
                    break;
                case Operations.V0oF1_Ia:
                    S7s8kX2.AppendLine("    not rax");
                    break;
                case Operations.AXJEUF8:
                    S7s8kX2.AppendLine("    test rax, rax");
                    S7s8kX2.AppendLine("    sete al");
                    S7s8kX2.AppendLine("    movzx rax, al");
                    break;
                case Operations.jeuvS6KLpYeMA43v9:
                case Operations.tmDJZxxjN8YO6gXg:
                    S7s8kX2.AppendLine("    add rax, 1");
                    break;
                case Operations.SaZ45v4KVaHn:
                case Operations.o1eEDaDB1HpqMTgNI:
                    S7s8kX2.AppendLine("    sub rax, 1");
                    break;
                default:
                    throw new NotImplementedException($"Unsupported binary operation: {XxC0ynDET.mfIdTBeeCV}");
            }
        }

        private void xY0C58HW(string gkfVuQR)
        {
            S7s8kX2.AppendLine("    cmp rax, rcx");
            S7s8kX2.AppendLine($"    {gkfVuQR} al");
            S7s8kX2.AppendLine("    movzx rax, al");
        }

        private void wBv0lHH3Pgrpf(IrValue FlHaV6FlTtLPOieCD, string PpMPwZP)
        {
            if (libgHsYUF4foJ7aAH.TryGetValue(FlHaV6FlTtLPOieCD, out int S7FJ7HAuO2))
                S7s8kX2.AppendLine($"    mov {PpMPwZP}, qword [rbp-{S7FJ7HAuO2}]");
            else if (Al7erL.TryGetValue(FlHaV6FlTtLPOieCD, out long eIk0eR))
                S7s8kX2.AppendLine($"    mov {PpMPwZP}, {eIk0eR}");
            else
                throw new InvalidOperationException($"Unknown IR value: {FlHaV6FlTtLPOieCD}");
        }

        private int J32YhIfpRYQ(IrValue FlHaV6FlTtLPOieCD)
        {
            if (libgHsYUF4foJ7aAH.TryGetValue(FlHaV6FlTtLPOieCD, out int S7FJ7HAuO2))
                return S7FJ7HAuO2;

            kXFfxJOEq += 8;
            libgHsYUF4foJ7aAH.Add(FlHaV6FlTtLPOieCD, kXFfxJOEq);
            return kXFfxJOEq;
        }

        public void eII2uUru()
        {
            S7s8kX2.AppendLine("global  start");
            S7s8kX2.AppendLine("extern ExitProcess");
            S7s8kX2.AppendLine("section .text");
        }

        public void qzV6kOD5yz()
        {
            S7s8kX2.AppendLine("start: ");
            S7s8kX2.AppendLine("    push rbp");
            S7s8kX2.AppendLine("    mov rbp, rsp");
            S7s8kX2.AppendLine($"    sub rsp, STACK_SIZE");
        }

        public void MwC364d3mVNMzvtu_()
        {
            S7s8kX2.AppendLine("    xor ecx, ecx");
            S7s8kX2.AppendLine("    sub rsp, 32");
            S7s8kX2.AppendLine("    call ExitProcess");

        }
    }
}
