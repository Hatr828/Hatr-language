using Best_language.src.back.Instructions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.back.Printers
{
    public static class X64Printer
    {
        public static string GenerateAsm(List<X64Instruction> instructions)
        {
            StringBuilder _sb = new();

            _sb.AppendLine("global  start");
            _sb.AppendLine("extern ExitProcess");
            _sb.AppendLine("section .text");

            foreach (var instruction in instructions)
            {
                switch(instruction.Opcode)
                {
                    case X64Opcode.sub:
                    case X64Opcode.mov:
                    case X64Opcode.movzx:
                    case X64Opcode.add:
                    case X64Opcode.or:
                    case X64Opcode.xor:
                    case X64Opcode.and:
                    case X64Opcode.sar:
                    case X64Opcode.test:
                    case X64Opcode.shl:
                        _sb.AppendLine($"    {instruction.Opcode} " +
                            $"{GetStringFromOperand(instruction.Operands[0])}," +
                            $" {GetStringFromOperand(instruction.Operands[1])}");
                        break;

                    case X64Opcode.call:
                    case X64Opcode.not:
                    case X64Opcode.neg:
                    case X64Opcode.sete:
                    case X64Opcode.setl:
                    case X64Opcode.setle:
                    case X64Opcode.setg:
                    case X64Opcode.setge:
                    case X64Opcode.idiv:
                    case X64Opcode.imul:
                    case X64Opcode.jmp:
                    case X64Opcode.jnz:
                    case X64Opcode.push:
                        _sb.AppendLine($"    {instruction.Opcode} " +
                            $"{GetStringFromOperand(instruction.Operands[0])}");
                        break;

                    case X64Opcode.cqo:
                    case X64Opcode.ret:
                        _sb.AppendLine($"    {instruction.Opcode} ");
                        break;

                    case X64Opcode.label:
                        _sb.AppendLine($"{GetStringFromOperand(instruction.Operands[0])}:");
                        break;

                    default:
                        throw new ArgumentException($"Unknown opcode: {instruction.Opcode}");
                }
            }

            return _sb.ToString();
        }

        private static string GetStringFromOperand(X64Operand operand)
        {
            switch (operand.Kind)
            {
                case X64ValueKind.Register:
                    return operand.Register.ToString()!;

                case X64ValueKind.Immediate:
                    return operand.Value.ToString();

                case X64ValueKind.Memory:
                    if (operand.Size != X64Size.@byte)
                        return $"{operand.Size} [{operand.Register}-{operand.Offset}]";
                    else
                        throw new NotSupportedException();
                case X64ValueKind.Label:
                    return $"{operand.Name}";

                case X64ValueKind.Symbol:
                    return $"{operand.Name}";

                default:
                    throw new ArgumentException($"Unknow operand: {operand}");
            }
        }
    }
}
