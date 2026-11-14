using Best_language.src.back.Instructions;
using Best_language.src.front;
using Best_language.src.middle;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Best_language.src.back.Generators
{
    class X64CodeGenerator(IrProgram irProgram)
    {
        private readonly IrProgram _irProgram = irProgram;

        private readonly Dictionary<IrValue, int> _locals = new();

        private readonly Dictionary<IrValue, long> _constants = new();

        private int offset;

        private List<IrInstruction>? _irInstructions;
        private List<X64Instruction> _instructions = new();

        private int idx;

        public List<X64Instruction> GenerateInstructions()
        {
            idx = 0;
            _instructions = new();
            _irInstructions = _irProgram.Instructions;

            AppendStartFunc();

            while (idx < _irInstructions.Count)
            {
                switch(_irInstructions[idx++])
                {
                    case IrBinary binary:
                        {
                            OperationGen(binary);

                            int slot = GetOrCreateSlot(binary.Left);
                            _constants.Remove(binary.Left);

                            _instructions.Add(new(X64Opcode.mov,
                                new X64MemoryOperand(X64Register.rbp, slot),
                                new X64RegisterOperand(X64Register.rax)));
                            break;
                        }
                    case IrUnary unary:
                        {
                            if (unary.Op is Operations.Plus) break;
                            OperationUnaryGen(unary);

                            int slot = GetOrCreateSlot(unary.Right);
                            _constants.Remove(unary.Right);

                            _instructions.Add(new(X64Opcode.mov,
                                new X64MemoryOperand(X64Register.rbp, slot),
                                new X64RegisterOperand(X64Register.rax)));
                            break;
                        }
                    case IrVariable variable:
                        {
                            LoadValue(variable.Right, X64Register.rax);

                            int slot = GetOrCreateSlot(variable.Left);
                            _constants.Remove(variable.Left);

                            _instructions.Add(new(X64Opcode.mov,
                                new X64MemoryOperand(X64Register.rbp, slot),
                                new X64RegisterOperand(X64Register.rax)));
                            break;
                        }
                    case IrConstant constant:
                        {
                            _constants.Add(constant.Left, constant.Right);
                            break;
                        }
                    case IrLabel label:
                        {
                            _instructions.Add(new(X64Opcode.label, new X64LabelOperand($"{label.Name}")));
                            break;
                        }
                    case IrJump jump:
                        {
                            _instructions.Add(new(X64Opcode.jmp, new X64LabelOperand($"{jump.Target}")));
                            break;
                        }
                    case IrJumpIfTrue jump:
                        {
                            LoadValue(jump.Condition, X64Register.rax);
                            _instructions.Add(new(X64Opcode.test,
                                new X64RegisterOperand(X64Register.rax),
                                new X64RegisterOperand(X64Register.rax)));
                            _instructions.Add(new(X64Opcode.jnz, new X64LabelOperand($"{jump.Target}")));
                            break;
                        }
                }
            }

            AppendEndFunc();

            PutStackSize((offset + 15) & ~15);

            return _instructions;
        }
        private void PutStackSize(int stackSize)
        {
            for(int i = 0; i < _instructions.Count; i++)
            {
                if (_instructions[i].Opcode is not X64Opcode.sub) continue;
                if (_instructions[i].Right is not null) continue;

                _instructions[i].Right = new X64IntOperand(stackSize);
                return;
            }
        }

        private void OperationGen(IrBinary binary)
        {
            LoadValue(binary.Left, X64Register.rax);
            LoadValue(binary.Right, X64Register.rcx);

            switch (binary.Op)
            {
                case Operations.PlusEquals:
                case Operations.Plus:
                    _instructions.Add(new(X64Opcode.add,
                               new X64RegisterOperand(X64Register.rax),
                               new X64RegisterOperand(X64Register.rcx)));
                    break;

                case Operations.MinusEquals:
                case Operations.Minus:
                    _instructions.Add(new(X64Opcode.sub,
                               new X64RegisterOperand(X64Register.rax),
                               new X64RegisterOperand(X64Register.rcx)));
                    break;

                case Operations.MultiplyEquals:
                case Operations.Multiply:
                    _instructions.Add(new(X64Opcode.imul,
                               new X64RegisterOperand(X64Register.rax),
                               new X64RegisterOperand(X64Register.rcx)));
                    break;

                case Operations.DivideEquals:
                case Operations.Divide:
                    _instructions.Add(new(X64Opcode.cqo));
                    _instructions.Add(new(X64Opcode.idiv, new X64RegisterOperand(X64Register.rcx)));
                    break;

                case Operations.DivisionRemEquals:
                case Operations.DivisionRem:
                    _instructions.Add(new(X64Opcode.cqo));
                    _instructions.Add(new(X64Opcode.idiv, new X64RegisterOperand(X64Register.rcx)));
                    _instructions.Add(new(X64Opcode.mov,
                               new X64RegisterOperand(X64Register.rax),
                               new X64RegisterOperand(X64Register.rdx)));
                    break;

                case Operations.GreaterThan:
                    Compare(X64Opcode.setg);
                    break;

                case Operations.GreaterThanOrEqual:
                    Compare(X64Opcode.setge);
                    break;

                case Operations.LessThan:
                    Compare(X64Opcode.setl);
                    break;

                case Operations.LessThanOrEqual:
                    Compare(X64Opcode.setle);
                    break;

                case Operations.EqualsEquals:
                    Compare(X64Opcode.sete);
                    break;

                case Operations.NotEquals:
                    Compare(X64Opcode.setne);
                    break;

                case Operations.And:
                    _instructions.Add(new(X64Opcode.sub,
                              new X64RegisterOperand(X64Register.rax),
                              new X64RegisterOperand(X64Register.rax)));

                    _instructions.Add(new(X64Opcode.setne, new X64RegisterOperand(X64Register.al)));

                    _instructions.Add(new(X64Opcode.sub,
                             new X64RegisterOperand(X64Register.rcx),
                             new X64RegisterOperand(X64Register.rcx)));

                    _instructions.Add(new(X64Opcode.setne, new X64RegisterOperand(X64Register.cl)));
                    _instructions.Add(new(X64Opcode.and,
                          new X64RegisterOperand(X64Register.al),
                          new X64RegisterOperand(X64Register.cl)));

                    _instructions.Add(new(X64Opcode.movzx,
                          new X64RegisterOperand(X64Register.rax),
                          new X64RegisterOperand(X64Register.al)));
                    break;

                case Operations.Or:
                    _instructions.Add(new(X64Opcode.or,
                             new X64RegisterOperand(X64Register.rax),
                             new X64RegisterOperand(X64Register.rcx)));

                    _instructions.Add(new(X64Opcode.setne, new X64RegisterOperand(X64Register.al))); 

                    _instructions.Add(new(X64Opcode.movzx,
                        new X64RegisterOperand(X64Register.rax),
                        new X64RegisterOperand(X64Register.al)));
                    break;

                case Operations.BitAndEquals:
                case Operations.BitAnd:
                    _instructions.Add(new(X64Opcode.and,
                             new X64RegisterOperand(X64Register.rax),
                             new X64RegisterOperand(X64Register.rcx)));
                    break;

                case Operations.BitOrEquals:
                case Operations.BitOr:
                    _instructions.Add(new(X64Opcode.or,
                             new X64RegisterOperand(X64Register.rax),
                             new X64RegisterOperand(X64Register.rcx)));
                    break;

                case Operations.BitXorEquals:
                case Operations.BitXor:
                    _instructions.Add(new(X64Opcode.xor,
                             new X64RegisterOperand(X64Register.rax),
                             new X64RegisterOperand(X64Register.rcx)));
                    break;

                case Operations.LeftShiftEquals:
                case Operations.LeftShift:
                    _instructions.Add(new(X64Opcode.shl,
                             new X64RegisterOperand(X64Register.rax),
                             new X64RegisterOperand(X64Register.cl)));
                    break;

                case Operations.RightShiftEquals:
                case Operations.RightShift:
                    _instructions.Add(new(X64Opcode.sar,
                             new X64RegisterOperand(X64Register.rax),
                             new X64RegisterOperand(X64Register.cl)));
                    break;

                default:
                    throw new NotImplementedException($"Unsupported binary operation: {binary.Op}");
            }
        }
        private void OperationUnaryGen(IrUnary binary)
        {
            LoadValue(binary.Right, X64Register.rax);

            switch (binary.Op)
            {
                case Operations.Minus:
                    _instructions.Add(new(X64Opcode.neg, new X64RegisterOperand(X64Register.rax)));
                    break;
                case Operations.BitNot:
                    _instructions.Add(new(X64Opcode.not, new X64RegisterOperand(X64Register.rax)));
                    break;
                case Operations.Not:
                    _instructions.Add(new(X64Opcode.test,
                         new X64RegisterOperand(X64Register.rax),
                         new X64RegisterOperand(X64Register.rax)));

                    _instructions.Add(new(X64Opcode.sete, new X64RegisterOperand(X64Register.al)));

                    _instructions.Add(new(X64Opcode.movzx,
                         new X64RegisterOperand(X64Register.rax),
                         new X64RegisterOperand(X64Register.al)));
                    break;
                case Operations.PlusPlusRight:
                case Operations.PlusPlusLeft:
                    _instructions.Add(new(X64Opcode.add,
                         new X64RegisterOperand(X64Register.rax),
                         new X64IntOperand(1)));
                    break;
                case Operations.MinusMinusRight:
                case Operations.MinusMinusLeft:
                    _instructions.Add(new(X64Opcode.sub,
                         new X64RegisterOperand(X64Register.rax),
                         new X64IntOperand(1)));
                    break;
                default:
                    throw new NotImplementedException($"Unsupported binary operation: {binary.Op}");
            }
        }

        private void Compare(X64Opcode op)
        {
            _instructions.Add(new(X64Opcode.cmp,
                        new X64RegisterOperand(X64Register.rax),
                        new X64RegisterOperand(X64Register.rcx)));

            _instructions.Add(new(op, new X64RegisterOperand(X64Register.al)));

            _instructions.Add(new(X64Opcode.movzx,
                       new X64RegisterOperand(X64Register.rax),
                       new X64RegisterOperand(X64Register.al)));
        }

        private void LoadValue(IrValue value, X64Register register)
        {
            if (_locals.TryGetValue(value, out int slot))
                _instructions.Add(new(X64Opcode.mov,
                       new X64RegisterOperand(register),
                       new X64MemoryOperand(X64Register.rbp, slot)));
            else if (_constants.TryGetValue(value, out long constant))
                _instructions.Add(new(X64Opcode.mov,
                       new X64RegisterOperand(register),
                       new X64LongOperand(constant)));
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

        public void AppendStartFunc()
        {
            _instructions.Add(new(X64Opcode.label, new X64LabelOperand("start")));
            _instructions.Add(new(X64Opcode.push, new X64RegisterOperand(X64Register.rbp)));
            _instructions.Add(new(X64Opcode.mov, new X64RegisterOperand(X64Register.rbp), new X64RegisterOperand(X64Register.rsp)));
            _instructions.Add(new(X64Opcode.sub, new X64RegisterOperand(X64Register.rsp))); // Later should be added stack size by program
        }

        public void AppendEndFunc()
        {
            _instructions.Add(new(X64Opcode.xor, new X64RegisterOperand(X64Register.ecx), new X64RegisterOperand(X64Register.ecx)));
            _instructions.Add(new(X64Opcode.sub, new X64RegisterOperand(X64Register.rsp), new X64IntOperand(32)));
            _instructions.Add(new(X64Opcode.call, new X64SymbolOperand("ExitProcess")));
        }
    }
}
