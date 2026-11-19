using Best_language.src.back.Instructions;
using Best_language.src.front;
using Best_language.src.middle;
using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.back.Generators
{
    public partial class X64CodeGenerator
    {
        private void OperationUnaryGen(IrUnary unary)
        {
            LoadValue(unary.Right, X64Register.rax);

            switch (unary.Op)
            {
                case Operations.Minus:
                    Emit(new(X64Opcode.neg, X64Operand.Reg(X64Register.rax)));
                    break;
                case Operations.BitNot:
                    Emit(new(X64Opcode.not, X64Operand.Reg(X64Register.rax)));
                    break;
                case Operations.Not:
                    Emit(new(X64Opcode.test,
                         X64Operand.Reg(X64Register.rax),
                         X64Operand.Reg(X64Register.rax)));

                    Emit(new(X64Opcode.sete, X64Operand.Reg(X64Register.al)));

                    Emit(new(X64Opcode.movzx,
                         X64Operand.Reg(X64Register.rax),
                         X64Operand.Reg(X64Register.al)));
                    break;
                case Operations.PlusPlusRight:
                case Operations.PlusPlusLeft:
                    Emit(new(X64Opcode.add,
                         X64Operand.Reg(X64Register.rax),
                         X64Operand.Imm(X64Size.word, 1)));
                    break;
                case Operations.MinusMinusRight:
                case Operations.MinusMinusLeft:
                    Emit(new(X64Opcode.sub,
                         X64Operand.Reg(X64Register.rax),
                         X64Operand.Imm(X64Size.word, 1)));
                    break;
                default:
                    throw new NotImplementedException($"Unsupported unary operation: {unary.Op}");
            }
        }
        private void OperationUnaryFloatGen(IrUnary unary)
        {
            LoadValue(unary.Right, X64Register.xmm0);

            int size = GetSlotSize(unary.Right);
            bool isFloat = size == 4;
            bool increment = false;

            switch (unary.Op)
            {
                case Operations.Minus:
                    long mask = isFloat ? 0x80000000L : long.MinValue;

                    Emit(new(X64Opcode.mov,
                        X64Operand.Reg(X64Register.rax),
                        X64Operand.Imm(X64Size.qword, mask)));

                    Emit(new(isFloat ? X64Opcode.movd : X64Opcode.movq,
                        X64Operand.Reg(X64Register.xmm1),
                        X64Operand.Reg(isFloat ? X64Register.eax : X64Register.rax)));

                    Emit(new(isFloat ? X64Opcode.xorps : X64Opcode.xorpd,
                        X64Operand.Reg(X64Register.xmm0),
                        X64Operand.Reg(X64Register.xmm1)));

                    break;
                case Operations.Not:
                    Emit(new(X64Opcode.xorps,
                       X64Operand.Reg(X64Register.xmm1),
                       X64Operand.Reg(X64Register.xmm1)));

                    Emit(new(isFloat ? X64Opcode.ucomiss : X64Opcode.ucomisd,
                       X64Operand.Reg(X64Register.xmm0),
                       X64Operand.Reg(X64Register.xmm1)));

                    Emit(new(X64Opcode.sete, X64Operand.Reg(X64Register.al)));

                    Emit(new(X64Opcode.setnp, X64Operand.Reg(X64Register.cl)));

                    Emit(new(X64Opcode.and, X64Operand.Reg(X64Register.al), X64Operand.Reg(X64Register.cl)));

                    Emit(new(X64Opcode.movzx, X64Operand.Reg(X64Register.eax), X64Operand.Reg(X64Register.al)));

                    break;
                case Operations.PlusPlusRight:
                case Operations.PlusPlusLeft:
                    increment = true;
                    goto case Operations.MinusMinusRight;
                case Operations.MinusMinusRight:
                case Operations.MinusMinusLeft:
                    long bits = isFloat ? BitConverter.SingleToInt32Bits(1.0f) : BitConverter.DoubleToInt64Bits(1.0);

                    Emit(new(X64Opcode.mov,
                        X64Operand.Reg(X64Register.rax),
                        X64Operand.Imm(X64Size.qword, bits)));

                    Emit(new(isFloat ? X64Opcode.movd : X64Opcode.movq,
                        X64Operand.Reg(X64Register.xmm1),
                        X64Operand.Reg(isFloat ? X64Register.eax : X64Register.rax)));

                    X64Opcode op = increment ? (isFloat ? X64Opcode.addss : X64Opcode.addsd) : (isFloat ? X64Opcode.subss : X64Opcode.subsd);

                    Emit(new(op, X64Operand.Reg(X64Register.xmm0), X64Operand.Reg(X64Register.xmm1)));

                    break;
                default:
                    throw new NotImplementedException($"Unsupported unary operation: {unary.Op}");
            }
        }

        private X64Opcode GetOpcodeFromOperationInt(Operations op) => op switch
        {
            Operations.PlusEquals or Operations.Plus => X64Opcode.add,
            Operations.MinusEquals or Operations.Minus => X64Opcode.sub,
            Operations.MultiplyEquals or Operations.Multiply => X64Opcode.imul,
            Operations.BitAndEquals or Operations.BitAnd => X64Opcode.and,
            Operations.BitOrEquals or Operations.BitOr => X64Opcode.or,
            Operations.BitXorEquals or Operations.BitXor => X64Opcode.xor,

            Operations.GreaterThan => X64Opcode.setg,
            Operations.GreaterThanOrEqual => X64Opcode.setge,
            Operations.LessThan => X64Opcode.setl,
            Operations.LessThanOrEqual => X64Opcode.setle,
            Operations.NotEquals => X64Opcode.setne,

            _ => throw new NotImplementedException()
        };
        private X64Opcode GetOpcodeFromOperationFloat(Operations op, bool isFloat) => op switch
        {
            Operations.PlusEquals or Operations.Plus => isFloat ? X64Opcode.addss : X64Opcode.addsd,
            Operations.MinusEquals or Operations.Minus => isFloat ? X64Opcode.subss : X64Opcode.subsd,
            Operations.MultiplyEquals or Operations.Multiply => isFloat ? X64Opcode.mulss : X64Opcode.mulsd,
            Operations.DivideEquals or Operations.Divide => isFloat ? X64Opcode.divss : X64Opcode.divsd,

            Operations.GreaterThan => X64Opcode.seta,
            Operations.GreaterThanOrEqual => X64Opcode.setae,
            Operations.LessThan => X64Opcode.setb,
            Operations.LessThanOrEqual => X64Opcode.setbe,
            Operations.NotEquals => X64Opcode.setne,
            Operations.EqualsEquals => X64Opcode.sete,

            _ => throw new NotImplementedException()
        };

        private void ULongToFloatConversion(IrConversion conversion, int rightSize, int leftSize)
        {
            string suffix = Guid.NewGuid().ToString("N");
            string smallLabel = $"u64_fp_u64_{suffix}";
            string endLabel = $"u64_to_fp_end_{suffix}";

            var convertOpcode = leftSize == 4 ? X64Opcode.cvtsi2ss : X64Opcode.cvtsi2sd;

            LoadValue(conversion.Right, X64Register.rax);

            Emit(new(X64Opcode.mov, X64Operand.Reg(X64Register.rcx), X64Operand.Reg(X64Register.rax)));

            Emit(new(X64Opcode.shr, X64Operand.Reg(X64Register.rcx), X64Operand.Imm(X64Size.@byte, 63)));

            Emit(new(X64Opcode.test, X64Operand.Reg(X64Register.rcx), X64Operand.Reg(X64Register.rcx)));

            Emit(new(X64Opcode.jz, X64Operand.Label(smallLabel)));

            Emit(new(X64Opcode.mov, X64Operand.Reg(X64Register.rcx), X64Operand.Reg(X64Register.rax)));

            Emit(new(X64Opcode.and, X64Operand.Reg(X64Register.rcx), X64Operand.Imm(X64Size.@byte, 1)));

            Emit(new(X64Opcode.shr, X64Operand.Reg(X64Register.rax), X64Operand.Imm(X64Size.@byte, 1)));

            Emit(new(X64Opcode.or, X64Operand.Reg(X64Register.rax), X64Operand.Reg(X64Register.rcx)));

            Emit(new(convertOpcode, X64Operand.Reg(X64Register.xmm0), X64Operand.Reg(X64Register.rax)));

            Emit(new(leftSize == 4 ? X64Opcode.addss : X64Opcode.addsd,
                X64Operand.Reg(X64Register.xmm0), X64Operand.Reg(X64Register.xmm0)));

            Emit(new(X64Opcode.jmp, X64Operand.Label(endLabel)));

            Emit(new(X64Opcode.label, X64Operand.Label(smallLabel)));

            Emit(new(convertOpcode, X64Operand.Reg(X64Register.xmm0), X64Operand.Reg(X64Register.rax)));

            Emit(new(X64Opcode.label, X64Operand.Label(endLabel)));

            var destination = GetOrCreateSlot(conversion.Left);

            Emit(new(leftSize == 4 ? X64Opcode.movss : X64Opcode.movsd,
                X64Operand.Mem(X64Register.rbp, -destination.Item1, destination.Item2), X64Operand.Reg(X64Register.xmm0)));

            _constants.Remove(conversion.Left);
            _constantsFP.Remove(conversion.Left);
        }
        private void FloatToUlongConversion(IrConversion conversion, int rightSize, int leftSize)
        {
            string suffix = Guid.NewGuid().ToString("N");
            string smallLabel = $"fp_to_u64_{suffix}";
            string endLabel = $"fp_to_u64_end_{suffix}";

            var convertOpcode = rightSize == 4 ? X64Opcode.cvttss2si : X64Opcode.cvttsd2si;

            LoadValue(conversion.Right, X64Register.xmm0);

            if (rightSize == 4)
            {
                Emit(new(X64Opcode.mov, X64Operand.Reg(X64Register.eax), X64Operand.Imm(X64Size.dword, 0x5F000000)));

                Emit(new(X64Opcode.movd, X64Operand.Reg(X64Register.xmm1), X64Operand.Reg(X64Register.eax)));
            }
            else
            {
                Emit(new(X64Opcode.mov, X64Operand.Reg(X64Register.rax), X64Operand.Imm(X64Size.qword, 0x43E0000000000000L)));

                Emit(new(X64Opcode.movq, X64Operand.Reg(X64Register.xmm1), X64Operand.Reg(X64Register.rax)));
            }

            Emit(new(rightSize == 4 ? X64Opcode.ucomiss : X64Opcode.ucomisd,
                X64Operand.Reg(X64Register.xmm0),
                X64Operand.Reg(X64Register.xmm1)));

            Emit(new(X64Opcode.setae, X64Operand.Reg(X64Register.al)));

            Emit(new(X64Opcode.test, X64Operand.Reg(X64Register.al), X64Operand.Reg(X64Register.al)));

            Emit(new(X64Opcode.jz, X64Operand.Label(smallLabel)));

            Emit(new(rightSize == 4 ? X64Opcode.subss : X64Opcode.subsd,
                X64Operand.Reg(X64Register.xmm0),
                X64Operand.Reg(X64Register.xmm1)));

            Emit(new(convertOpcode, X64Operand.Reg(X64Register.rax), X64Operand.Reg(X64Register.xmm0)));

            Emit(new(X64Opcode.mov, X64Operand.Reg(X64Register.rcx), X64Operand.Imm(X64Size.qword, long.MinValue)));

            Emit(new(X64Opcode.or, X64Operand.Reg(X64Register.rax), X64Operand.Reg(X64Register.rcx)));

            Emit(new(X64Opcode.jmp, X64Operand.Label(endLabel)));

            Emit(new(X64Opcode.label, X64Operand.Label(smallLabel)));

            Emit(new(convertOpcode, X64Operand.Reg(X64Register.rax), X64Operand.Reg(X64Register.xmm0)));

            Emit(new(X64Opcode.label, X64Operand.Label(endLabel)));

            var destination = GetOrCreateSlot(conversion.Left);

            Emit(new(X64Opcode.mov, X64Operand.Mem(X64Register.rbp, -destination.Item1, destination.Item2), X64Operand.Reg(X64Register.rax)));

            _constants.Remove(conversion.Left);
            _constantsFP.Remove(conversion.Left);
        }

        private void Compare(X64Opcode op)
        {
            Emit(new(X64Opcode.cmp,
                X64Operand.Reg(X64Register.rax),
                X64Operand.Reg(X64Register.rcx)));

            Emit(new(op, X64Operand.Reg(X64Register.al)));

            Emit(new(X64Opcode.movzx,
                X64Operand.Reg(X64Register.rax),
                X64Operand.Reg(X64Register.al)));
        }
        private void CompareFloat(X64Opcode op, bool isFloat)
        {
            Emit(new(isFloat ? X64Opcode.ucomiss : X64Opcode.ucomisd,
                X64Operand.Reg(X64Register.xmm0),
                X64Operand.Reg(X64Register.xmm1)));

            Emit(new(op, X64Operand.Reg(X64Register.al)));

            if (op is X64Opcode.setne or X64Opcode.setb or X64Opcode.setbe)
            {
                Emit(new(X64Opcode.setnp, X64Operand.Reg(X64Register.cl)));

                Emit(new(X64Opcode.and,
                    X64Operand.Reg(X64Register.al),
                    X64Operand.Reg(X64Register.cl)));
            }
            else if (op is X64Opcode.setne)
            {
                Emit(new(X64Opcode.setp, X64Operand.Reg(X64Register.cl)));

                Emit(new(X64Opcode.or,
                    X64Operand.Reg(X64Register.al),
                    X64Operand.Reg(X64Register.cl)));
            }

            Emit(new(X64Opcode.movzx,
                X64Operand.Reg(X64Register.eax),
                X64Operand.Reg(X64Register.al)));
        }

        private void LoadValue(IrValue value, X64Register register)
        {
            if (_locals.TryGetValue(value, out (int slot, X64Size size) val))
                if (register >= X64Register.xmm0 && register <= X64Register.xmm15)
                    Emit(new(X64Opcode.movsd,
                       X64Operand.Reg(register),
                       X64Operand.Mem(X64Register.rbp, -val.slot, val.size)));
                else
                    Emit(new(X64Opcode.mov,
                        X64Operand.Reg(register),
                        X64Operand.Mem(X64Register.rbp, -val.slot, val.size)));
            else if (_constants.TryGetValue(value, out (ulong val, TypeSymbol type) constant))
            {
                if (constant.type is IntegerTypeSymbol or BoolTypeSymbol)
                {
                    Emit(new(X64Opcode.mov,
                        X64Operand.Reg(register),
                        X64Operand.Imm(GetX64Size(constant.type), (long)constant.val)));
                }
                else throw new ArgumentException();
            }
            else if (_constantsFP.TryGetValue(value, out (double val, TypeSymbol type) constantFP))
            {
                if (constantFP.type is FloatTypeSymbol type)
                {
                    long bits = BitConverter.DoubleToInt64Bits(constantFP.val);

                    Emit(new(X64Opcode.mov,
                        X64Operand.Reg(X64Register.rax),
                        X64Operand.Imm(X64Size.qword, bits)));

                    Emit(new(X64Opcode.movq,
                        X64Operand.Reg(register),
                        X64Operand.Reg(X64Register.rax)));
                }
                else throw new ArgumentException();
            }
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
            BoolTypeSymbol => 1,
            IntegerTypeSymbol type => (int)type.Size,
            FloatTypeSymbol type => (int)type.Size,
            PointerTypeSymbol => 8,
            _ => throw new ArgumentException()
        };

        private X64Size GetX64Size(TypeSymbol type) => type switch
        {
            BoolTypeSymbol => (X64Size)1,
            IntegerTypeSymbol tmp => (X64Size)tmp.Size,
            FloatTypeSymbol tmp => (X64Size)tmp.Size,
            PointerTypeSymbol => (X64Size)8,
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

        private X64Register GetRegisterSame(int size) => size switch
        {
            1 => X64Register.al,
            2 => X64Register.ax,
            4 => X64Register.eax,
            8 => X64Register.rax,
            _ => throw new ArgumentException()
        };
        private X64Register GetRegisterSame(IrValue value) => value.Type switch
        {
            BoolTypeSymbol => X64Register.al,
            IntegerTypeSymbol type => GetRegisterSame((int)type.Size),
            FloatTypeSymbol type => GetRegisterSame((int)type.Size),
            PointerTypeSymbol => X64Register.rax,
            _ => throw new ArgumentException()
        };
        private X64Register GetRegisterSame(X64Size size) => size switch
        {
            X64Size.@byte => X64Register.al,
            X64Size.word => X64Register.ax,
            X64Size.dword => X64Register.eax,
            X64Size.qword => X64Register.rax,
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
        private X64Register GetRegister(TypeSymbol type) => type switch
        {
            BoolTypeSymbol => X64Register.r8b,
            IntegerTypeSymbol tmp => GetRegister((int)tmp.Size),
            FloatTypeSymbol tmp => GetRegister((int)tmp.Size),
            PointerTypeSymbol => X64Register.rax,
            _ => throw new ArgumentException()
        };
        private X64Register GetRegister(int size) => size switch
        {
            1 => X64Register.r8b,
            2 => X64Register.cx,
            4 => X64Register.edx,
            8 => X64Register.rax,

            _ => throw new ArgumentException()
        };

        private void Emit(X64Instruction instruction)
        {
            _instructions.Add(instruction);
        }

        public void AppendStartFunc()
        {
            Emit(new(X64Opcode.label, X64Operand.Label("start")));
            Emit(new(X64Opcode.push, X64Operand.Reg(X64Register.rbp)));
            Emit(new(X64Opcode.mov, X64Operand.Reg(X64Register.rbp), X64Operand.Reg(X64Register.rsp)));
            Emit(new(X64Opcode.sub, X64Operand.Reg(X64Register.rsp), X64Operand.Imm(X64Size.dword, -1))); // Later should be added stack size by program
            StackIdx = _instructions.Count - 1;
        }
        public void AppendEndFunc()
        {
            Emit(new(X64Opcode.xor, X64Operand.Reg(X64Register.ecx), X64Operand.Reg(X64Register.ecx)));
            Emit(new(X64Opcode.sub, X64Operand.Reg(X64Register.rsp), X64Operand.Imm(X64Size.@byte, 32)));
            Emit(new(X64Opcode.call, X64Operand.Symbol("ExitProcess")));
        }
    }
}
