using Best_language.src.back.Instructions;
using Best_language.src.front;
using Best_language.src.middle;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
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
                case Operation.Minus:
                    Emit(new(X64Opcode.neg, X64Operand.Reg(X64Register.rax)));
                    break;
                case Operation.BitNot:
                    Emit(new(X64Opcode.not, X64Operand.Reg(X64Register.rax)));
                    break;
                case Operation.Not:
                    Emit(new(X64Opcode.test,
                         X64Operand.Reg(X64Register.rax),
                         X64Operand.Reg(X64Register.rax)));

                    Emit(new(X64Opcode.sete, X64Operand.Reg(X64Register.al)));

                    Emit(new(X64Opcode.movzx,
                         X64Operand.Reg(X64Register.rax),
                         X64Operand.Reg(X64Register.al)));
                    break;
                case Operation.PlusPlusRight:
                case Operation.PlusPlusLeft:
                    Emit(new(X64Opcode.add,
                         X64Operand.Reg(X64Register.rax),
                         X64Operand.Imm(X64Size.word, 1)));
                    break;
                case Operation.MinusMinusRight:
                case Operation.MinusMinusLeft:
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
                case Operation.Minus:
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
                case Operation.Not:
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
                case Operation.PlusPlusRight:
                case Operation.PlusPlusLeft:
                    increment = true;
                    goto case Operation.MinusMinusRight;

                case Operation.MinusMinusRight:
                case Operation.MinusMinusLeft:
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
        private void OperationUnaryPointerGen(IrUnary unary)
        {
            switch (unary.Op)
            {
                case Operation.ReferenceOf:
                    (var offset, _) = GetOrCreateSlot(unary.Right);

                    Emit(new(X64Opcode.lea,
                        X64Operand.Reg(X64Register.rax),
                        X64Operand.Mem(X64Register.rbp, -offset, X64Size.qword)));
                    break;
                case Operation.Dereference:
                    LoadValue(unary.Right, X64Register.rax);

                    int res = GetSlotSize(unary.Result);
                    X64Size size = GetX64Size(unary.Result.Type);

                    if (unary.Result.Type is FloatTypeSymbol)
                        Emit(new(res == 4 ? X64Opcode.movss : X64Opcode.movsd,
                            X64Operand.Reg(X64Register.xmm0),
                            X64Operand.Mem(X64Register.rax, -0, size)));
                    else
                        Emit(new(X64Opcode.mov, X64Operand.Reg(GetRegisterSame(res)),
                              X64Operand.Mem(X64Register.rax, -0, size)));
                    break;
                case Operation.Not:
                    LoadValue(unary.Right, X64Register.rax);

                    Emit(new(X64Opcode.test, X64Operand.Reg(X64Register.rax), X64Operand.Reg(X64Register.rax)));

                    Emit(new(X64Opcode.sete, X64Operand.Reg(X64Register.al)));

                    Emit(new(X64Opcode.movzx, X64Operand.Reg(X64Register.rax), X64Operand.Reg(X64Register.al)));
                    break;
                case Operation.MinusMinusLeft:
                case Operation.PlusPlusLeft:
                case Operation.MinusMinusRight:
                case Operation.PlusPlusRight:
                    var pointer = (PointerTypeSymbol)unary.Right.Type;

                    int step = GetTypeSize(pointer.ElementType);

                    LoadValue(unary.Right, X64Register.rax);

                    bool increment = unary.Op is Operation.PlusPlusRight or Operation.PlusPlusLeft;

                    Emit(new(increment ? X64Opcode.add : X64Opcode.sub,
                        X64Operand.Reg(X64Register.rax),
                        X64Operand.Imm(X64Size.dword, step)));

                    break;
                default:
                    throw new NotImplementedException($"Unsupported unary operation: {unary.Op}");
            }
        }

        private X64Opcode GetOpcodeFromOperationInt(Operation op) => op switch
        {
            Operation.PlusEquals or Operation.Plus => X64Opcode.add,
            Operation.MinusEquals or Operation.Minus => X64Opcode.sub,
            Operation.MultiplyEquals or Operation.Multiply => X64Opcode.imul,
            Operation.BitAndEquals or Operation.BitAnd => X64Opcode.and,
            Operation.BitOrEquals or Operation.BitOr => X64Opcode.or,
            Operation.BitXorEquals or Operation.BitXor => X64Opcode.xor,

            Operation.GreaterThan => X64Opcode.setg,
            Operation.GreaterThanOrEqual => X64Opcode.setge,
            Operation.LessThan => X64Opcode.setl,
            Operation.LessThanOrEqual => X64Opcode.setle,
            Operation.NotEquals => X64Opcode.setne,
            Operation.EqualsEquals => X64Opcode.sete,

            _ => throw new NotImplementedException()
        };
        private X64Opcode GetOpcodeFromOperationFloat(Operation op, bool isFloat) => op switch
        {
            Operation.PlusEquals or Operation.Plus => isFloat ? X64Opcode.addss : X64Opcode.addsd,
            Operation.MinusEquals or Operation.Minus => isFloat ? X64Opcode.subss : X64Opcode.subsd,
            Operation.MultiplyEquals or Operation.Multiply => isFloat ? X64Opcode.mulss : X64Opcode.mulsd,
            Operation.DivideEquals or Operation.Divide => isFloat ? X64Opcode.divss : X64Opcode.divsd,

            Operation.GreaterThan => X64Opcode.seta,
            Operation.GreaterThanOrEqual => X64Opcode.setae,
            Operation.LessThan => X64Opcode.setb,
            Operation.LessThanOrEqual => X64Opcode.setbe,
            Operation.NotEquals => X64Opcode.setne,
            Operation.EqualsEquals => X64Opcode.sete,

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

        private void Compare(X64Opcode op, X64Register l, X64Register r, X64Size resSize)
        {
            Emit(new(X64Opcode.cmp,
                X64Operand.Reg(l),
                X64Operand.Reg(r)));

            Emit(new(op, X64Operand.Reg(X64Register.al)));

            if (resSize != X64Size.@byte)
            {
                Emit(new(X64Opcode.movzx,
                    X64Operand.Reg(X64Register.eax),
                    X64Operand.Reg(X64Register.al)));
            }
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

            _localOffset += size;
            _locals.Add(value, (_localOffset, x64Size));
            return (_localOffset, x64Size);
        }
        private (int, X64Size) GetSlot(IrValue value)
        {
            if (_locals.TryGetValue(value, out (int, X64Size) val))
                return val;

            throw new ArgumentException($"Unknown slot fot {value}");
        }

        private int GetSlotSize(IrValue value) => value.Type switch
        {
            BoolTypeSymbol => 1,
            IntegerTypeSymbol type => (int)type.Size,
            FloatTypeSymbol type => (int)type.Size,
            PointerTypeSymbol => 8,
            StructTypeSymbol st => st.Size,
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
            _ => X64Size.custom,
        };
        private int GetTypeSize(TypeSymbol type) => type switch
        {
            BoolTypeSymbol => 1,
            FloatTypeSymbol @float => (int)@float.Size,
            IntegerTypeSymbol integer => (int)integer.Size,
            PointerTypeSymbol => 8,

            _ => throw new NotImplementedException(),
        };

        private X64Register GetRegisterSame(int size) => size switch
        {
            1 => X64Register.al,
            2 => X64Register.ax,
            4 => X64Register.eax,
            8 => X64Register.rax,
            _ => throw new ArgumentException()
        };
        private X64Register GetRegisterSame(TypeSymbol type) => type switch
        {
            BoolTypeSymbol => X64Register.al,
            IntegerTypeSymbol tmp => GetRegisterSame((int)tmp.Size),
            FloatTypeSymbol tmp => GetRegisterSame((int)tmp.Size),
            PointerTypeSymbol => X64Register.rax,
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

        private X64Register GetFloatParamRegister(int idx) => idx switch
        {
            1 => X64Register.xmm0,
            2 => X64Register.xmm1,
            3 => X64Register.xmm2,
            4 => X64Register.xmm3,
            _ => throw new ArgumentException("Expected _idx < 4"),
        };
        private X64Register GetIntegerParamRegister(int idx, X64Size size)
        {
            ReadOnlySpan<X64Register> registers = size switch
            {
                X64Size.@byte => [X64Register.al, X64Register.dl, X64Register.r8b, X64Register.r9b],
                X64Size.word => [X64Register.cx, X64Register.dx, X64Register.r8w, X64Register.r9w],
                X64Size.dword => [X64Register.ecx, X64Register.edx, X64Register.r8d, X64Register.r9d],
                X64Size.qword => [X64Register.rcx, X64Register.rdx, X64Register.r8, X64Register.r9],
                _ => throw new ArgumentException("Unknown size"),
            };

            return registers[idx]; 
        }

        private void Emit(X64Instruction instruction)
        {
            _instructions.Add(instruction);
        }

        private int AlignUp(int val, int alignment) => checked(val + alignment - 1) & ~(alignment - 1);
        private void AppendStartFunc()
        {
            Emit(new(X64Opcode.label, X64Operand.Label("start")));
            Emit(new(X64Opcode.sub, X64Operand.Reg(X64Register.rsp), X64Operand.Imm(X64Size.dword, 40)));

            Emit(new(X64Opcode.call, X64Operand.Label("main")));

            Emit(new(X64Opcode.mov, X64Operand.Reg(X64Register.ecx), X64Operand.Reg(X64Register.eax)));

            Emit(new(X64Opcode.call, X64Operand.Symbol("ExitProcess")));
        }

        private void FuncEpilogue()
        {
            int offset = AlignUp(_localOffset, 16);

            _instructions[_stackFixup].Operands[1] = X64Operand.Imm(X64Size.@byte, offset);

            Emit(new(X64Opcode.label, X64Operand.Label(_funcEpilogue)));

            if (offset > 0)
                Emit(new(X64Opcode.add,
                  X64Operand.Reg(X64Register.rsp),
                  X64Operand.Imm(X64Size.dword, offset)));

            Emit(new(X64Opcode.pop, X64Operand.Reg(X64Register.rbp)));

            Emit(new(X64Opcode.ret));
        }
    }
}
