using Best_language.src.front;
using Best_language.src.middle;
using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.back
{
    class X64CodeGenerator(IrProgram irProgram)
    {
        private readonly IrProgram _irProgram = irProgram;

        private readonly Dictionary<IrValue, int> _locals = new();

        private readonly Dictionary<IrValue, long> _constants = new();

        private StringBuilder _sb = new();

        private int offset;

        private List<IrInstruction>? instructions;

        private int idx;

        public string GenerateAsm()
        {
            AppendHeader();
            AppendStartFunc();
            idx = 0;
            instructions = _irProgram.Instructions;

            while (idx < instructions.Count)
            {
                switch(instructions[idx++])
                {
                    case IrBinary binary:
                        {
                            OperationGen(binary);

                            int slot = GetOrCreateSlot(binary.Left);
                            _constants.Remove(binary.Left);

                            _sb.AppendLine($"    mov qword [rbp-{slot}], rax");
                            break;
                        }
                    case IrUnary unary:
                        {
                            if (unary.Op is Operations.Plus) break;
                            OperationUnaryGen(unary);

                            int slot = GetOrCreateSlot(unary.Right);
                            _constants.Remove(unary.Right);

                            _sb.AppendLine($"    mov qword [rbp-{slot}], rax");
                            break;
                        }
                    case IrVariable variable:
                        {
                            LoadValue(variable.Right, "rax");

                            int slot = GetOrCreateSlot(variable.Left);
                            _constants.Remove(variable.Left);

                            _sb.AppendLine($"    mov qword [rbp-{slot}], rax");
                            break;
                        }
                    case IrConstant constant:
                        {
                            _constants.Add(constant.Left, constant.Right);
                            break;
                        }
                    case IrLabel label:
                        {
                            _sb.AppendLine($"{label.Name}:");
                            break;
                        }
                    case IrJump jump:
                        {
                            _sb.AppendLine($"    jmp {jump.Target}");
                            break;
                        }
                    case IrJumpIfTrue jump:
                        {
                            LoadValue(jump.Condition, "rax");
                            _sb.AppendLine("    test rax, rax");
                            _sb.AppendLine($"    jnz {jump.Target}");
                            break;
                        }
                }
            }

            AppendEndFunc();

            int stackSize = (offset + 15) & ~15;
            _sb.Replace("STACK_SIZE", stackSize.ToString());

            return _sb.ToString();
        }
        private void OperationGen(IrBinary binary)
        {
            LoadValue(binary.Left, "rax");
            LoadValue(binary.Right, "rcx");

            switch (binary.Op)
            {
                case Operations.PlusEquals:
                case Operations.Plus:
                    _sb.AppendLine("    add rax, rcx");
                    break;

                case Operations.MinusEquals:
                case Operations.Minus:
                    _sb.AppendLine("    sub rax, rcx");
                    break;

                case Operations.MultiplyEquals:
                case Operations.Multiply:
                    _sb.AppendLine("    imul rax, rcx");
                    break;

                case Operations.DivideEquals:
                case Operations.Divide:
                    _sb.AppendLine("    cqo");
                    _sb.AppendLine("    idiv rcx");
                    break;

                case Operations.DivisionRemEquals:
                case Operations.DivisionRem:
                    _sb.AppendLine("    cqo");
                    _sb.AppendLine("    idiv rcx");
                    _sb.AppendLine("    mov rax, rdx");
                    break;

                case Operations.GreaterThan:
                    Compare("setg");
                    break;

                case Operations.GreaterThanOrEqual:
                    Compare("setge");
                    break;

                case Operations.LessThan:
                    Compare("setl");
                    break;

                case Operations.LessThanOrEqual:
                    Compare("setle");
                    break;

                case Operations.EqualsEquals:
                    Compare("sete");
                    break;

                case Operations.NotEquals:
                    Compare("setne");
                    break;

                case Operations.And:
                    _sb.AppendLine("    test rax, rax");
                    _sb.AppendLine("    setne al");
                    _sb.AppendLine("    test rcx, rcx");
                    _sb.AppendLine("    setne cl");
                    _sb.AppendLine("    and al, cl");
                    _sb.AppendLine("    movzx rax, al");
                    break;

                case Operations.Or:
                    _sb.AppendLine("    or rax, rcx");
                    _sb.AppendLine("    setne al");
                    _sb.AppendLine("    movzx rax, al");
                    break;

                case Operations.BitAndEquals:
                case Operations.BitAnd:
                    _sb.AppendLine("    and rax, rcx");
                    break;

                case Operations.BitOrEquals:
                case Operations.BitOr:
                    _sb.AppendLine("    or rax, rcx");
                    break;

                case Operations.BitXorEquals:
                case Operations.BitXor:
                    _sb.AppendLine("    xor rax, rcx");
                    break;

                case Operations.LeftShiftEquals:
                case Operations.LeftShift:
                    _sb.AppendLine("    shl rax, cl");
                    break;

                case Operations.RightShiftEquals:
                case Operations.RightShift:
                    _sb.AppendLine("    sar rax, cl");
                    break;

                default:
                    throw new NotImplementedException($"Unsupported binary operation: {binary.Op}");
            }
        }
        private void OperationUnaryGen(IrUnary binary)
        {
            LoadValue(binary.Right, "rax");

            switch (binary.Op)
            {
                case Operations.Minus:
                    _sb.AppendLine("    neg rax");
                    break;
                case Operations.BitNot:
                    _sb.AppendLine("    not rax");
                    break;
                case Operations.Not:
                    _sb.AppendLine("    test rax, rax");
                    _sb.AppendLine("    sete al");
                    _sb.AppendLine("    movzx rax, al");
                    break;
                case Operations.PlusPlusRight:
                case Operations.PlusPlusLeft:
                    _sb.AppendLine("    add rax, 1");
                    break;
                case Operations.MinusMinusRight:
                case Operations.MinusMinusLeft:
                    _sb.AppendLine("    sub rax, 1");
                    break;
                default:
                    throw new NotImplementedException($"Unsupported binary operation: {binary.Op}");
            }
        }

        private void Compare(string instruction)
        {
            _sb.AppendLine("    cmp rax, rcx");
            _sb.AppendLine($"    {instruction} al");
            _sb.AppendLine("    movzx rax, al");
        }

        private void LoadValue(IrValue value, string register)
        {
            if (_locals.TryGetValue(value, out int slot))
                _sb.AppendLine($"    mov {register}, qword [rbp-{slot}]");
            else if (_constants.TryGetValue(value, out long constant))
                _sb.AppendLine($"    mov {register}, {constant}");
            else
                throw new InvalidOperationException($"Unknown IR value: {value}");
        }

        private int GetOrCreateSlot(IrValue value)
        {
            if (_locals.TryGetValue(value, out int slot))
                return slot;

            offset += 8;
            _locals.Add(value, offset);
            return offset;
        }

        public void AppendHeader()
        {
            _sb.AppendLine("global  start");
            _sb.AppendLine("extern ExitProcess");
            _sb.AppendLine("section .text");
        }

        public void AppendStartFunc()
        {
            _sb.AppendLine("start: ");
            _sb.AppendLine("    push rbp");
            _sb.AppendLine("    mov rbp, rsp");
            _sb.AppendLine($"    sub rsp, STACK_SIZE");
        }

        public void AppendEndFunc()
        {
            _sb.AppendLine("    xor ecx, ecx");
            _sb.AppendLine("    sub rsp, 32");
            _sb.AppendLine("    call ExitProcess");

        }
    }
}
