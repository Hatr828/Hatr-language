using Best_language.src.back.Instructions;
using System.Text;

namespace Best_language.src.back.Printers
{
    public static class X64Printer
    {
        public static string GenerateAsm(List<X64Instruction> instructions, List<byte> globalData, Dictionary<string, int> dataOffset)
        {
            StringBuilder sb = new();

            sb.AppendLine("global  start");
            sb.AppendLine("extern ExitProcess");
            sb.AppendLine("extern GetStdHandle");

            AppendDataSection(sb, globalData, dataOffset);
            sb.AppendLine("section .text");

            foreach (var instruction in instructions)
            {
                switch (instruction.Opcode)
                {
                    case >= X64Opcode.cqo and <= X64Opcode.ret:
                        sb.AppendLine($"    {instruction.Opcode} ");
                        break;

                    case X64Opcode.label:
                        sb.AppendLine($"{GetStringFromOperand(instruction.Operands[0])}:");
                        break;

                    case >= X64Opcode.mov and <= X64Opcode.add:
                    case >= X64Opcode.movss and <= X64Opcode.movq:
                    case X64Opcode.imul:
                        sb.AppendLine($"    {instruction.Opcode} " +
                            $"{GetStringFromOperand(instruction.Operands[0])}," +
                            $" {GetStringFromOperand(instruction.Operands[1])}");
                        break;

                    case >= X64Opcode.push and <= X64Opcode.setnp:
                        sb.AppendLine($"    {instruction.Opcode} " +
                            $"{GetStringFromOperand(instruction.Operands[0])}");
                        break;

                    default:
                        throw new ArgumentException($"Unknown opcode: {instruction.Opcode}");
                }
            }

            return sb.ToString();
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
                    string address = operand.Name is { } name ? $"rel {name}" : operand.Register.ToString()!;

                    string offset = operand.Offset >= 0 ? $"+{operand.Offset}" : operand.Offset.ToString()!;

                    return $"{operand.Offset} [{address}{offset}]";
                case X64ValueKind.Label:
                    return $"{operand.Name}";

                case X64ValueKind.Symbol:
                    return $"{operand.Name}";

                default:
                    throw new ArgumentException($"Unknow operand: {operand}");
            }
        }

        private static void AppendDataSection(StringBuilder sb, List<byte> globalData, Dictionary<string, int> dataOffset)
        {
            if (globalData.Count == 0 || dataOffset.Count == 0) return;

            sb.AppendLine("section .data");

            int pos = 0;

            foreach (var (name, offset) in dataOffset.OrderBy(x => x.Value))
            {
                AppendDataBytes(sb, globalData, pos, offset);
                sb.Append(name).AppendLine(":");
                pos = offset;
            }

            AppendDataBytes(sb, globalData, pos, globalData.Count);
            sb.AppendLine();
        }

        private static void AppendDataBytes(StringBuilder sb, List<byte> data, int start, int end)
        {
            if (start == end) return;

            sb.Append("    db ");

            for (int i = start; i < end; i++)
            {
                if (i > start) sb.Append(", ");

                sb.Append($"0x{data[i]:X2}");
            }

            sb.AppendLine();
        }
    }
}
