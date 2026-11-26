using Best_language.src.back.Instructions;
using Best_language.src.front;
using Best_language.src.middle;
using Microsoft.Extensions.FileProviders;
using Microsoft.Win32;
using System.Drawing;
using System.Numerics;
using System.Numerics.Colors;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Best_language.src.back.Generators
{
    public partial class X64CodeGenerator(IrProgram irProgram)
    {
        private readonly IrProgram _irProgram = irProgram;

        private readonly Dictionary<IrValue, (int, X64Size)> _locals = new();

        private readonly Dictionary<IrValue, (ulong, TypeSymbol)> _constants = new();
        private readonly Dictionary<IrValue, (double, TypeSymbol)> _constantsFP = new();
        private int _stackFixup;
        private string _funcEpilogue;
        private int _localOffset;

        private List<IrInstruction>? _irInstructions;
        private List<X64Instruction> _instructions = new();

        private int _idx;

        public List<X64Instruction> GenerateInstructions()
        {
            _idx = 0;
            _instructions = new();
            _irInstructions = _irProgram.Instructions;

            AppendStartFunc();

            while (_idx < _irInstructions.Count)
            {
                switch (_irInstructions[_idx++])
                {
                    case IrBinary binary:
                        {
                            bool isFloat = binary.Right.Type is FloatTypeSymbol;
                            bool resultFloat = binary.Result.Type is FloatTypeSymbol;

                            if (isFloat) OperationBinaryFloatGen(binary);
                            else OperationBinaryGen(binary);

                            (int slot, X64Size size) = GetOrCreateSlot(binary.Result);

                            if(resultFloat)
                                Emit(new(size == X64Size.dword ? X64Opcode.movss : X64Opcode.movsd,
                                    X64Operand.Mem(X64Register.rbp, -slot, size),
                                    X64Operand.Reg(X64Register.xmm0)));
                            else
                                Emit(new(X64Opcode.mov,
                                    X64Operand.Mem(X64Register.rbp, -slot, size),
                                    X64Operand.Reg(GetRegisterSame(binary.Result))));

                            break;
                        }
                    case IrUnary unary:
                        {
                            bool rightFloat = unary.Right.Type is FloatTypeSymbol;
                            bool resultFloat = unary.Result.Type is FloatTypeSymbol;

                            if (unary.Op is Operation.Plus)
                            {
                                LoadValue(unary.Right, rightFloat ? X64Register.xmm0 : GetRegisterSame(unary.Right));
                            }
                            else if (unary.Right.Type is PointerTypeSymbol || unary.Result.Type is PointerTypeSymbol)
                                OperationUnaryPointerGen(unary);
                            else if (rightFloat) OperationUnaryFloatGen(unary);
                            else OperationUnaryGen(unary);

                            (int slot, X64Size size) = GetOrCreateSlot(unary.Result);

                            if (resultFloat)
                            {
                                Emit(new(size == X64Size.dword ? X64Opcode.movss : X64Opcode.movsd,
                                    X64Operand.Mem(X64Register.rbp, -slot, size),
                                    X64Operand.Reg(X64Register.xmm0)));
                            }
                            else
                                Emit(new(X64Opcode.mov,
                                    X64Operand.Mem(X64Register.rbp, -slot, size),
                                    X64Operand.Reg(GetRegisterSame(size))));

                            break;
                        }
                    case IrRet ret:
                        {
                            if (ret.Res is IrValue res)
                            {
                                var reg = res.Type is FloatTypeSymbol ? X64Register.xmm0 : GetRegisterSame(res.Type);

                                LoadValue(res, reg);
                            }

                            Emit(new(X64Opcode.jmp, X64Operand.Label(_funcEpilogue!)));
                            break;
                        }
                    case IrFunc func:
                        {
                            if (_funcEpilogue is not null)
                            {
                                FuncEpilogue();
                            }

                            _localOffset = 0;
                            _locals.Clear();
                            _funcEpilogue = func.Name + "_epilogue";

                            Emit(new(X64Opcode.label, X64Operand.Label(func.Name)));

                            Emit(new(X64Opcode.push, X64Operand.Reg(X64Register.rbp)));

                            Emit(new(X64Opcode.mov,
                                X64Operand.Reg(X64Register.rbp),
                                X64Operand.Reg(X64Register.rsp)));


                            _stackFixup = _instructions.Count;

                            Emit(new(X64Opcode.sub,
                                X64Operand.Reg(X64Register.rsp),
                                X64Operand.Imm(X64Size.dword, 0)));

                            break;
                        }
                    case IrCall call:
                        {
                            int count = call.Params.Count;
                            int stackSize = AlignUp(32 + Math.Max(0, count - 4) * 8, 16);

                            Emit(new(X64Opcode.sub,
                                X64Operand.Reg(X64Register.rsp),
                                X64Operand.Imm(X64Size.dword, stackSize)));

                            for (int i = 4; i < count; i++)
                            {
                                var arg = call.Params[i];
                                var size = GetX64Size(arg.Type);

                                bool isFloat = arg.Type is FloatTypeSymbol;
                                var reg = isFloat ? X64Register.xmm4 : GetRegisterSame(size);

                                var opcode = isFloat ? (size == X64Size.dword ? X64Opcode.movss : X64Opcode.movsd) : X64Opcode.mov;

                                LoadValue(arg, reg);

                                Emit(new(opcode, X64Operand.Mem(X64Register.rsp, 32 + (i - 4) * 8, size),
                                    X64Operand.Reg(reg)));
                            }

                            for (int i = 0; i < Math.Min(count, 4); i++)
                            {
                                var arg = call.Params[i];
                                var reg = arg.Type is FloatTypeSymbol ? GetFloatParamRegister(i) : GetIntegerParamRegister(i, GetX64Size(arg.Type));

                                LoadValue(arg, reg);
                            }

                            Emit(new(X64Opcode.call, X64Operand.Label(call.Name)));

                            Emit(new(X64Opcode.add,
                                X64Operand.Reg(X64Register.rsp),
                                X64Operand.Imm(X64Size.dword, stackSize)));

                            if (call.Res.Type is not VoidTypeSymbol)
                            {
                                var (slot, size) = GetOrCreateSlot(call.Res);
                                bool isFloat = call.Res.Type is FloatTypeSymbol;
                                var reg = isFloat ? X64Register.xmm0 : GetRegisterSame(size);
                                var opcode = isFloat ? (size == X64Size.dword ? X64Opcode.movss : X64Opcode.movsd) : X64Opcode.mov;

                                Emit(new(opcode, X64Operand.Mem(X64Register.rbp, -slot, size),
                                    X64Operand.Reg(reg)));
                            }
                            break;
                        }
                    case IrParam param:
                        {
                            var (slot, size) = GetOrCreateSlot(param.Left);
                            bool isFloat = param.Left.Type is FloatTypeSymbol;

                            var opcode = isFloat ? (size == X64Size.dword ? X64Opcode.movss : X64Opcode.movsd) : X64Opcode.mov;

                            X64Register left;

                            if (param.Idx < 4)
                            {
                                left = isFloat ? GetFloatParamRegister(param.Idx) : GetIntegerParamRegister(param.Idx, size); 
                            }
                            else
                            {
                                int offset = 48 + (param.Idx - 4) * 8;

                                left = isFloat ? X64Register.xmm4 : GetRegisterSame(size);

                                Emit(new(opcode,
                                    X64Operand.Reg(left),
                                    X64Operand.Mem(X64Register.rbp, offset, size)));
                            }
                            Emit(new(opcode,
                                X64Operand.Mem(X64Register.rbp, -slot, size),
                                X64Operand.Reg(left)));

                            break;
                        }
                    case IrVariable variable:
                        {
                            var type = variable.Right.Type;
                            var size1 = GetX64Size(type);
                            bool isFloat = type is FloatTypeSymbol;

                            var reg = isFloat ? X64Register.xmm0 : GetRegister(size1);
                            var opcode = isFloat ? (size1 == X64Size.dword ? X64Opcode.movss : X64Opcode.movsd) : X64Opcode.mov;

                            LoadValue(variable.Right, reg);

                            (int slot, X64Size size) = GetOrCreateSlot(variable.Left);

                            _constants.Remove(variable.Right);
                            _constantsFP.Remove(variable.Right);

                            Emit(new(opcode,
                                X64Operand.Mem(X64Register.rbp, -slot, size),
                                X64Operand.Reg(reg)));
                            break;
                        }
                    case IrLoad address:
                        {
                            (int offset, _) = GetSlot(address.Right);

                            (int slot, X64Size size) = GetOrCreateSlot(address.Left);

                            LoadValue(address.Right, X64Register.rcx);

                            Emit(new(X64Opcode.mov,
                                X64Operand.Reg(GetRegister(size)),
                                X64Operand.Mem(X64Register.rcx, address.Offset, size)));

                            Emit(new(X64Opcode.mov,
                                X64Operand.Mem(X64Register.rbp, -slot, size),
                                X64Operand.Reg(GetRegister(size))));
                            break;
                        }
                    case IrIndexedAddress address:
                        {
                            LoadValue(address.BaseAddress, X64Register.rax);
                            LoadValue(address.Index, X64Register.rcx);

                            Emit(new(X64Opcode.mov,
                                X64Operand.Reg(X64Register.edx),
                                X64Operand.Imm(X64Size.dword, address.Step)));

                            Emit(new(X64Opcode.imul,
                                X64Operand.Reg(X64Register.rcx),
                                X64Operand.Reg(X64Register.rdx)));

                            Emit(new(X64Opcode.add,
                                X64Operand.Reg(X64Register.rax),
                                X64Operand.Reg(X64Register.rcx)));

                            (int offset, _) = GetOrCreateSlot(address.Left);

                            Emit(new(X64Opcode.mov,
                                X64Operand.Mem(X64Register.rbp, -offset, X64Size.qword),
                                X64Operand.Reg(X64Register.rax)));

                            break;
                        }
                    case IrStore store:
                        {
                            var type = store.Right.Type;
                            var size = GetX64Size(type);
                            bool isFloat = type is FloatTypeSymbol;

                            var reg = isFloat ? X64Register.xmm0 : GetRegister(size);
                            var opcode = isFloat ? (size == X64Size.dword ? X64Opcode.movss : X64Opcode.movsd) : X64Opcode.mov;

                            LoadValue(store.Right, reg);
                            LoadValue(store.Left, X64Register.rcx);

                            Emit(new(opcode,
                                X64Operand.Mem(X64Register.rcx, store.Offset, size), 
                                X64Operand.Reg(reg)));
                            break;
                        }
                    case IrConstant constant:
                        {
                            _constants.Add(constant.Left, (constant.Right, constant.TypeRight));
                            break;
                        }
                    case IrArrayConstant constant:
                        {
                            if (constant.TypeRight is StructTypeSymbol st)
                                _locals.Add(constant.Left, (st.Size, X64Size.custom));
                            else if (constant.TypeRight is ArrayTypeSymbol arrType)
                                _locals.Add(constant.Left, ((int)arrType.Size, X64Size.custom));
                            else throw new NotImplementedException();
                            break;
                        }
                    case IrFloatConstant constant:
                        {
                            _constantsFP.Add(constant.Left, (constant.Right, constant.TypeRight));
                            break;
                        }
                    case IrLabel label:
                        {
                            Emit(new(X64Opcode.label, X64Operand.Label($"{label.Name}")));
                            break;
                        }
                    case IrJump jump:
                        {
                            Emit(new(X64Opcode.jmp, X64Operand.Label($"{jump.Target}")));
                            break;
                        }
                    case IrJumpIfTrue jump:
                        {
                            X64Register register = GetRegisterSame((jump.Condition)); 

                            LoadValue(jump.Condition, register);
                            Emit(new(X64Opcode.test, X64Operand.Reg(register), X64Operand.Reg(register)));
                            Emit(new(X64Opcode.jnz, X64Operand.Label($"{jump.Target}")));
                            break;
                        }
                    case IrJumpIfNotTrue jump:
                        {
                            X64Register register = GetRegisterSame((jump.Condition));

                            LoadValue(jump.Condition, register);
                            Emit(new(X64Opcode.test, X64Operand.Reg(register), X64Operand.Reg(register)));
                            Emit(new(X64Opcode.jz, X64Operand.Label($"{jump.Target}")));
                            break;
                        }
                    case IrConversion conversion:
                        {
                            ConversionGen(conversion);
                            break;
                        }
                    default:
                        throw new ArgumentException("Cannot generate instruction for: " + _irInstructions[_idx - 1]);
                }
            }

            if (_funcEpilogue is not null)
            {
                FuncEpilogue();
            }

            return _instructions;
        }

        private void ConversionGen(IrConversion conversion)
        {
            int rightSize = GetSlotSize(conversion.Right);
            int leftSize = GetSlotSize(conversion.Left);

            bool rightFloat = conversion.Right.Type is FloatTypeSymbol;
            bool leftFloat = conversion.Left.Type is FloatTypeSymbol;

            bool rightSigned = conversion.Right.Type is IntegerTypeSymbol integer && integer.IsSigned;
            bool leftSigned = conversion.Left.Type is IntegerTypeSymbol integer2 && integer2.IsSigned;

            if (leftFloat && !rightFloat && rightSize == 8 && !rightSigned)
            {
                ULongToFloatConversion(conversion, rightSize, leftSize);
                return;
            }

            if (rightFloat && conversion.Left.Type is IntegerTypeSymbol && leftSize == 8 && !leftSigned)
            {
                FloatToUlongConversion(conversion, rightSize, leftSize);
                return;
            }

            X64Register rightRegister = rightFloat ? X64Register.xmm0 : GetRegisterSame(rightSize);

            LoadValue(conversion.Right, rightRegister);

            (int leftOffset, X64Size leftSizeX64) = GetOrCreateSlot(conversion.Left);

            if (conversion.Left.Type is BoolTypeSymbol)
            {
                if(rightFloat)
                {
                    Emit(new(X64Opcode.xorps, X64Operand.Reg(X64Register.xmm1), X64Operand.Reg(X64Register.xmm1)));

                    Emit(new(rightSize == 4 ? X64Opcode.ucomiss : X64Opcode.ucomisd, 
                        X64Operand.Reg(X64Register.xmm0), X64Operand.Reg(X64Register.xmm1)));

                    Emit(new(X64Opcode.setne, X64Operand.Reg(X64Register.al)));

                    Emit(new(X64Opcode.setp, X64Operand.Reg(X64Register.cl)));

                    Emit(new(X64Opcode.or, X64Operand.Reg(X64Register.al), X64Operand.Reg(X64Register.cl)));
                }
                else
                {
                    Emit(new(X64Opcode.test,
                        X64Operand.Reg(rightRegister),
                        X64Operand.Reg(rightRegister)));

                    Emit(new(X64Opcode.setne, X64Operand.Reg(X64Register.al)));
                }
                Emit(new(X64Opcode.mov,
                    X64Operand.Mem(X64Register.rbp, -leftOffset, X64Size.@byte),
                    X64Operand.Reg(X64Register.al)));
            }
            else if (leftFloat)
            {
                if(rightFloat)
                {
                    if (rightSize != leftSize)
                        Emit(new(leftSize == 8 ? X64Opcode.cvtss2sd : X64Opcode.cvtsd2ss, 
                            X64Operand.Reg(X64Register.xmm0), X64Operand.Reg(X64Register.xmm0)));
                }
                else
                {
                    X64Register sourceRegister;

                    if (rightSize < 4)
                    {
                        Emit(new(rightSigned ? X64Opcode.movsx : X64Opcode.movzx, 
                            X64Operand.Reg(X64Register.eax), X64Operand.Reg(rightRegister)));

                        sourceRegister = X64Register.eax;
                    }
                    else if (rightSize == 4 && rightSigned) sourceRegister = X64Register.eax;
                    else sourceRegister = X64Register.rax;

                    Emit(new(leftSize == 4 ? X64Opcode.cvtsi2ss : X64Opcode.cvtsi2sd, 
                        X64Operand.Reg(X64Register.xmm0), X64Operand.Reg(sourceRegister)));
                }

                Emit(new(leftSize == 4 ? X64Opcode.movss : X64Opcode.movsd,
                       X64Operand.Mem(X64Register.rbp, -leftOffset, leftSizeX64), X64Operand.Reg(X64Register.xmm0)));
            }
            else if (rightFloat)
            {
                X64Register resultRegister = leftSize == 8 || (leftSize == 4 && !leftSigned) ? X64Register.rax : X64Register.eax;

                Emit(new(rightSize == 4 ? X64Opcode.cvttss2si : X64Opcode.cvttsd2si,
                        X64Operand.Reg(resultRegister), X64Operand.Reg(X64Register.xmm0)));

                Emit(new(X64Opcode.mov,
                       X64Operand.Mem(X64Register.rbp, -leftOffset, leftSizeX64), X64Operand.Reg(GetRegisterSame(leftSize))));
            }
            else
            {
                X64Register leftRegister = GetRegisterSame(leftSize);

                if (leftSize > rightSize)
                {
                    if (rightSigned)
                        Emit(new(rightSize == 4 ? X64Opcode.movsxd : X64Opcode.movsx,
                        X64Operand.Reg(leftRegister), X64Operand.Reg(rightRegister)));
                    else if (rightSize != 4)
                        Emit(new(X64Opcode.movzx,
                        X64Operand.Reg(leftRegister), X64Operand.Reg(rightRegister)));
                }

                Emit(new(X64Opcode.mov,
                       X64Operand.Mem(X64Register.rbp, -leftOffset, leftSizeX64), X64Operand.Reg(leftRegister)));
            }

            _constants.Remove(conversion.Left);
            _constantsFP.Remove(conversion.Left);
        }

        private void OperationBinaryFloatGen(IrBinary binary)
        {
            LoadValue(binary.Left, X64Register.xmm0);
            LoadValue(binary.Right, X64Register.xmm1);

            int size = GetSlotSize(binary.Left);

            bool isFloat = size == 4;

            switch (binary.Op)
            {
                case >= Operation.PlusEquals and <= Operation.Divide:
                    Emit(new(GetOpcodeFromOperationFloat(binary.Op, isFloat), X64Operand.Reg(X64Register.xmm0), X64Operand.Reg(X64Register.xmm1)));
                    break;

                case >= Operation.LessThan and <= Operation.NotEquals:
                    CompareFloat(GetOpcodeFromOperationFloat(binary.Op, isFloat), isFloat);
                    break;

                default:
                    throw new NotImplementedException($"Unsupported unary operation: {binary.Op}");
            }
        }
        private void OperationBinaryGen(IrBinary binary)
        {
            X64Register l = GetRegisterSame(binary.Left.Type);
            X64Register r = GetRegisterSame(binary.Left.Type) + 1;

            LoadValue(binary.Left, l);
            LoadValue(binary.Right, r);

            switch (binary.Op)
            {
                case >= Operation.PlusEquals and <= Operation.BitOr:
                    Emit(new(GetOpcodeFromOperationInt(binary.Op), X64Operand.Reg(l), X64Operand.Reg(r)));
                    break;

                case >= Operation.LessThan and <= Operation.NotEquals:
                    Compare(GetOpcodeFromOperationInt(binary.Op), l, r, GetX64Size(binary.Result.Type));
                    break;

                case Operation.Divide:
                    Emit(new(X64Opcode.cqo));
                    Emit(new(X64Opcode.idiv, X64Operand.Reg(r)));
                    break;

                case Operation.DivisionRem:
                    Emit(new(X64Opcode.cqo));
                    Emit(new(X64Opcode.idiv, X64Operand.Reg(r)));
                    Emit(new(X64Opcode.mov, X64Operand.Reg(l), X64Operand.Reg(X64Register.rdx)));
                    break;

                case Operation.And:
                    Emit(new(X64Opcode.sub,
                              X64Operand.Reg(l),
                              X64Operand.Reg(l)));

                    Emit(new(X64Opcode.setne, X64Operand.Reg(X64Register.al)));

                    Emit(new(X64Opcode.sub,
                             X64Operand.Reg(r),
                             X64Operand.Reg(r)));

                    Emit(new(X64Opcode.setne, X64Operand.Reg(X64Register.cl)));
                    Emit(new(X64Opcode.and,
                          X64Operand.Reg(X64Register.al),
                          X64Operand.Reg(X64Register.cl)));

                    Emit(new(X64Opcode.movzx,
                          X64Operand.Reg(l),
                          X64Operand.Reg(X64Register.al)));
                    break;

                case Operation.Or:
                    Emit(new(X64Opcode.or,
                             X64Operand.Reg(l),
                             X64Operand.Reg(r)));

                    Emit(new(X64Opcode.setne, X64Operand.Reg(X64Register.al)));

                    Emit(new(X64Opcode.movzx,
                        X64Operand.Reg(l),
                        X64Operand.Reg(X64Register.al)));
                    break;
                case Operation.LeftShift:
                    Emit(new(X64Opcode.shl,
                             X64Operand.Reg(l),
                             X64Operand.Reg(X64Register.cl)));
                    break;

                case Operation.RightShift:
                    Emit(new(X64Opcode.sar,
                             X64Operand.Reg(l),
                             X64Operand.Reg(X64Register.cl)));
                    break;

                default:
                    throw new NotImplementedException($"Unsupported unary operation: {binary.Op}");
            }
        }

    }
}
