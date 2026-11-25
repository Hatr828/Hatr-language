using Best_language.src.back.Instructions;
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
                switch (instruction.Opcode)
                {
                    case X64Opcode.cqo:
                    case X64Opcode.ret:
                        _sb.AppendLine($"    {instruction.Opcode} ");
                        break;

                    case X64Opcode.label:
                        _sb.AppendLine($"{GetStringFromOperand(instruction.Operands[0])}:");
                        break;

                    case >= X64Opcode.mov and <= X64Opcode.add:
                    case >= X64Opcode.movss and <= X64Opcode.movq:
                    case X64Opcode.imul:
                        _sb.AppendLine($"    {instruction.Opcode} " +
                            $"{GetStringFromOperand(instruction.Operands[0])}," +
                            $" {GetStringFromOperand(instruction.Operands[1])}");
                        break;

                    case >= X64Opcode.push and <= X64Opcode.setnp:
                        _sb.AppendLine($"    {instruction.Opcode} " +
                            $"{GetStringFromOperand(instruction.Operands[0])}");
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
                    if(operand.Offset == 0)
                    {
                        if (operand.Size != X64Size.@byte)
                            return $"{operand.Size} [{operand.Register}-{operand.Offset}]";
                        else
                            return $"byte [{operand.Register}-{operand.Offset}]";
                    }
                    else if (operand.Offset > 0)
                        if (operand.Size != X64Size.@byte)
                            return $"{operand.Size} [{operand.Register}+{operand.Offset}]";
                        else
                            return $"byte [{operand.Register}+{operand.Offset}]";
                    else
                        if (operand.Size != X64Size.@byte)
                            return $"{operand.Size} [{operand.Register}{operand.Offset}]";
                        else
                            return $"byte [{operand.Register}{operand.Offset}]";
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
