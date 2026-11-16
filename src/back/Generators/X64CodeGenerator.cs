using Best_language.src.back.Instructions;
using Best_language.src.front;
using Best_language.src.middle;

namespace Best_language.src.back.Generators
{
    class X64CodeGenerator(IrProgram irProgram)
    {
        private readonly IrProgram _irProgram = irProgram;

        private readonly Dictionary<IrValue, (int, X64Size)> _locals = new();

        private readonly Dictionary<IrValue, (long, VariableType)> _constants = new();

        private int offset;

        private List<IrInstruction>? _irInstructions;
        private List<X64Instruction> _instructions = new();

        private int idx;
        private int StackIdx;

        public List<X64Instruction> GenerateInstructions()
        {
            idx = 0;
            _instructions = new();
            _irInstructions = _irProgram.Instructions;

            AppendStartFunc();

            while (idx < _irInstructions.Count)
            {
                switch (_irInstructions[idx++])
                {
                    case IrBinary binary:
                        {
                            OperationGen(binary);

                            (int slot, X64Size size) = GetOrCreateSlot(binary.Left);
                            _constants.Remove(binary.Left);

                            _instructions.Add(new(X64Opcode.mov,
                                X64Operand.Mem(X64Register.rbp, slot, size),
                                X64Operand.Reg(GetRegister(size))));
                            break;
                        }
                    case IrUnary unary:
                        {
                            if (unary.Op is Operations.Plus) break;
                            OperationUnaryGen(unary);

                            (int slot, X64Size size) = GetOrCreateSlot(unary.Right);
                            _constants.Remove(unary.Right);

                            _instructions.Add(new(X64Opcode.mov,
                                X64Operand.Mem(X64Register.rbp, slot, size),
                                X64Operand.Reg(GetRegister(size))));
                            break;
                        }
                    case IrVariable variable:
                        {
                            LoadValue(variable.Right, GetRegister(variable.Right.Type));

                            (int slot, X64Size size) = GetOrCreateSlot(variable.Left);
                            _constants.Remove(variable.Left);

                            _instructions.Add(new(X64Opcode.mov,
                                X64Operand.Mem(X64Register.rbp, slot, size),
                                X64Operand.Reg(GetRegister(size))));
                            break;
                        }
                    case IrConstant constant:
                        {
                            _constants.Add(constant.Left, (constant.Right, constant.TypeRight));
                            break;
                        }
                    case IrLabel label:
                        {
                            _instructions.Add(new(X64Opcode.label, X64Operand.Label($"{label.Name}")));
                            break;
                        }
                    case IrJump jump:
                        {
                            _instructions.Add(new(X64Opcode.jmp, X64Operand.Label($"{jump.Target}")));
                            break;
                        }
                    case IrJumpIfTrue jump:
                        {
                            LoadValue(jump.Condition, X64Register.rax);
                            _instructions.Add(new(X64Opcode.test,
                                X64Operand.Reg(X64Register.rax),
                                X64Operand.Reg(X64Register.rax)));
                            _instructions.Add(new(X64Opcode.jnz, X64Operand.Label($"{jump.Target}")));
                            break;
                        }
                    case IrJumpIfNotTrue jump:
                        {
                            LoadValue(jump.Condition, X64Register.rax);
                            _instructions.Add(new(X64Opcode.test,
                                X64Operand.Reg(X64Register.rax),
                                X64Operand.Reg(X64Register.rax)));
                            _instructions.Add(new(X64Opcode.jz, X64Operand.Label($"{jump.Target}")));
                            break;
                        }
                    default:
                        throw new ArgumentException("Cannot generate instruction for: " + _irInstructions[idx - 1]);
                }
            }

            AppendEndFunc();

            _instructions[StackIdx].Operands[1] = X64Operand.Imm(X64Size.dword, ((offset + 15) & ~15));

            return _instructions;
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
                               X64Operand.Reg(X64Register.rax),
                               X64Operand.Reg(X64Register.rcx)));
                    break;

                case Operations.MinusEquals:
                case Operations.Minus:
                    _instructions.Add(new(X64Opcode.sub,
                               X64Operand.Reg(X64Register.rax),
                               X64Operand.Reg(X64Register.rcx)));
                    break;

                case Operations.MultiplyEquals:
                case Operations.Multiply:
                    _instructions.Add(new(X64Opcode.imul,
                               X64Operand.Reg(X64Register.rax),
                               X64Operand.Reg(X64Register.rcx)));
                    break;

                case Operations.DivideEquals:
                case Operations.Divide:
                    _instructions.Add(new(X64Opcode.cqo));
                    _instructions.Add(new(X64Opcode.idiv, X64Operand.Reg(X64Register.rcx)));
                    break;

                case Operations.DivisionRemEquals:
                case Operations.DivisionRem:
                    _instructions.Add(new(X64Opcode.cqo));
                    _instructions.Add(new(X64Opcode.idiv, X64Operand.Reg(X64Register.rcx)));
                    _instructions.Add(new(X64Opcode.mov,
                               X64Operand.Reg(X64Register.rax),
                               X64Operand.Reg(X64Register.rdx)));
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
                              X64Operand.Reg(X64Register.rax),
                              X64Operand.Reg(X64Register.rax)));

                    _instructions.Add(new(X64Opcode.setne, X64Operand.Reg(X64Register.al)));

                    _instructions.Add(new(X64Opcode.sub,
                             X64Operand.Reg(X64Register.rcx),
                             X64Operand.Reg(X64Register.rcx)));

                    _instructions.Add(new(X64Opcode.setne, X64Operand.Reg(X64Register.cl)));
                    _instructions.Add(new(X64Opcode.and,
                          X64Operand.Reg(X64Register.al),
                          X64Operand.Reg(X64Register.cl)));

                    _instructions.Add(new(X64Opcode.movzx,
                          X64Operand.Reg(X64Register.rax),
                          X64Operand.Reg(X64Register.al)));
                    break;

                case Operations.Or:
                    _instructions.Add(new(X64Opcode.or,
                             X64Operand.Reg(X64Register.rax),
                             X64Operand.Reg(X64Register.rcx)));

                    _instructions.Add(new(X64Opcode.setne, X64Operand.Reg(X64Register.al)));

                    _instructions.Add(new(X64Opcode.movzx,
                        X64Operand.Reg(X64Register.rax),
                        X64Operand.Reg(X64Register.al)));
                    break;

                case Operations.BitAndEquals:
                case Operations.BitAnd:
                    _instructions.Add(new(X64Opcode.and,
                             X64Operand.Reg(X64Register.rax),
                             X64Operand.Reg(X64Register.rcx)));
                    break;

                case Operations.BitOrEquals:
                case Operations.BitOr:
                    _instructions.Add(new(X64Opcode.or,
                             X64Operand.Reg(X64Register.rax),
                             X64Operand.Reg(X64Register.rcx)));
                    break;

                case Operations.BitXorEquals:
                case Operations.BitXor:
                    _instructions.Add(new(X64Opcode.xor,
                             X64Operand.Reg(X64Register.rax),
                             X64Operand.Reg(X64Register.rcx)));
                    break;

                case Operations.LeftShiftEquals:
                case Operations.LeftShift:
                    _instructions.Add(new(X64Opcode.shl,
                             X64Operand.Reg(X64Register.rax),
                             X64Operand.Reg(X64Register.cl)));
                    break;

                case Operations.RightShiftEquals:
                case Operations.RightShift:
                    _instructions.Add(new(X64Opcode.sar,
                             X64Operand.Reg(X64Register.rax),
                             X64Operand.Reg(X64Register.cl)));
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
                    _instructions.Add(new(X64Opcode.neg, X64Operand.Reg(X64Register.rax)));
                    break;
                case Operations.BitNot:
                    _instructions.Add(new(X64Opcode.not, X64Operand.Reg(X64Register.rax)));
                    break;
                case Operations.Not:
                    _instructions.Add(new(X64Opcode.test,
                         X64Operand.Reg(X64Register.rax),
                         X64Operand.Reg(X64Register.rax)));

                    _instructions.Add(new(X64Opcode.sete, X64Operand.Reg(X64Register.al)));

                    _instructions.Add(new(X64Opcode.movzx,
                         X64Operand.Reg(X64Register.rax),
                         X64Operand.Reg(X64Register.al)));
                    break;
                case Operations.PlusPlusRight:
                case Operations.PlusPlusLeft:
                    _instructions.Add(new(X64Opcode.add,
                         X64Operand.Reg(X64Register.rax),
                         X64Operand.Imm(X64Size.word, 1)));
                    break;
                case Operations.MinusMinusRight:
                case Operations.MinusMinusLeft:
                    _instructions.Add(new(X64Opcode.sub,
                         X64Operand.Reg(X64Register.rax),
                         X64Operand.Imm(X64Size.word, 1)));
                    break;
                default:
                    throw new NotImplementedException($"Unsupported binary operation: {binary.Op}");
            }
        }

        private void Compare(X64Opcode op)
        {
            _instructions.Add(new(X64Opcode.cmp,
                        X64Operand.Reg(X64Register.rax),
                        X64Operand.Reg(X64Register.rcx)));

            _instructions.Add(new(op, X64Operand.Reg(X64Register.al)));

            _instructions.Add(new(X64Opcode.movzx,
                       X64Operand.Reg(X64Register.rax),
                       X64Operand.Reg(X64Register.al)));
        }

        private void LoadValue(IrValue value, X64Register register)
        {
            if (_locals.TryGetValue(value, out (int slot, X64Size size) val))
                _instructions.Add(new(X64Opcode.mov,
                       X64Operand.Reg(register),
                       X64Operand.Mem(X64Register.rbp, val.slot, val.size)));
            else if (_constants.TryGetValue(value, out (long val, VariableType type) constant))
                _instructions.Add(new(X64Opcode.mov,
                       X64Operand.Reg(register),
                       X64Operand.Imm(GetX64Size(constant.type), constant.val)));
            else
                throw new InvalidOperationException($"Unknown IR value: {value}");
        }

        private (int, X64Size) GetOrCreateSlot(IrValue value)
        {
            if (_locals.TryGetValue(value, out (int, X64Size) val))
                return val;

            int size = GetSlotSize(value);
            X64Size x64Size = GetX64Size(size);

            offset += size;
            _locals.Add(value, (offset, x64Size));
            return (offset, x64Size);
        }

        private int GetSlotSize(IrValue value) => value.Type switch
        {
            VariableType.Bool => 1,
            VariableType.Short => 2,
            VariableType.Int => 4,
            VariableType.Long => 8,
            _ => throw new ArgumentException()
        };
        private X64Size GetX64Size(int slot) => slot switch
        {
            1 => X64Size.@byte,
            2 => X64Size.word,
            4 => X64Size.dword,
            8 => X64Size.qword,
            _ => throw new ArgumentException()
        };
        private X64Register GetRegister(X64Size size) => size switch
        {
            X64Size.@byte => X64Register.r8b,
            X64Size.word => X64Register.cx,
            X64Size.dword => X64Register.edx,
            X64Size.qword => X64Register.rax,
            _ => throw new ArgumentException()
        };
        private X64Size GetX64Size(VariableType type) => type switch
        {
            VariableType.Bool => X64Size.@byte,
            VariableType.Short => X64Size.word,
            VariableType.Int => X64Size.dword,
            VariableType.Long => X64Size.qword,
            _ => throw new ArgumentException()
        };
        private X64Register GetRegister(VariableType type) => type switch
        {
            VariableType.Bool => X64Register.r8b,
            VariableType.Short => X64Register.cx,
            VariableType.Int => X64Register.edx,
            VariableType.Long => X64Register.rax,
            _ => throw new ArgumentException()
        };

        public void AppendStartFunc()
        {
            _instructions.Add(new(X64Opcode.label, X64Operand.Label("start")));
            _instructions.Add(new(X64Opcode.push, X64Operand.Reg(X64Register.rbp)));
            _instructions.Add(new(X64Opcode.mov, X64Operand.Reg(X64Register.rbp), X64Operand.Reg(X64Register.rsp)));
            _instructions.Add(new(X64Opcode.sub, X64Operand.Reg(X64Register.rsp), X64Operand.Imm(X64Size.dword, -1))); // Later should be added stack size by program
            StackIdx = _instructions.Count - 1;
        }

        public void AppendEndFunc()
        {
            _instructions.Add(new(X64Opcode.xor, X64Operand.Reg(X64Register.ecx), X64Operand.Reg(X64Register.ecx)));
            _instructions.Add(new(X64Opcode.sub, X64Operand.Reg(X64Register.rsp), X64Operand.Imm(X64Size.@byte, 32)));
            _instructions.Add(new(X64Opcode.call, X64Operand.Symbol("ExitProcess")));
        }
    }
}
