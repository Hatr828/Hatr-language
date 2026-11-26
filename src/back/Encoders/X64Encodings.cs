using Best_language.src.back.Instructions;
using System.Buffers.Binary;

namespace Best_language.src.back.Encoders
{
    public partial class X64Encoder
    {
        private List<byte> _code = [];
        private List<PeImport> _importFixups = [];
        private readonly Dictionary<string, string> _dlls = new()
        {
            ["GetStdHandle"] = "kernel32.dll",
            ["ExitProcess"] = "kernel32.dll",
        };
        private List<X64Label> _labelFixups = [];
        private Dictionary<string, int> _labels = [];

        public (byte[] bytes, List<PeImport> importFixups) Encode(List<X64Instruction> instructions)
        {
            _code = [];
            _importFixups = [];
            _labelFixups = [];
            _labels = [];

        outer: foreach (var instruction in instructions)
            {
                if (instruction.Opcode is X64Opcode.label) EncodeLabel(instruction.Operands[0]);
                else if (instruction.Opcode is X64Opcode.call && instruction.Operands[0].Kind is X64ValueKind.Symbol) EncodeImportCall(instruction.Operands[0]);
                else
                {
                    foreach (var form in X64InstructionTable.GetForms(instruction.Opcode))
                    {
                        if (!TryMatch(form, instruction.Operands, out int size)) continue;

                        EncodeInstruction(form, instruction.Operands, size);

                        continue outer;
                    }

                    throw new NotSupportedException($"Unknown encoding for {instruction.Opcode}");
                }
            }

            var res = _code.ToArray();

            foreach (var fixup in _labelFixups)
            {
                if (!_labels.TryGetValue(fixup.Name, out int target)) throw new ArgumentException("");

                int displacement = checked(target - fixup.NextInstruction);

                BinaryPrimitives.WriteInt32LittleEndian(res.AsSpan(fixup.Offset, 4), displacement);
            }

            return (res, _importFixups);
        }
        public void EncodeLabel(X64Operand operand)
        {
            string name = operand.Name!;

            _labels[name] = _code.Count;
        }
        public bool TryMatch(X64Form form, X64Operand[] operands, out int size)
        {
            size = 0;

            if (operands.Length != form.Operands.Length) return false;

            for (int i = 0; i < operands.Length; i++)
            {
                var spec = form.Operands[i];
                var operand = operands[i];

                bool isXmm = operand.Kind is X64ValueKind.Register && operand.Register is >= X64Register.xmm0 and <= X64Register.xmm15;

                bool matches = spec.Kind switch
                {
                    X64OperandKind.Register => operand.Kind is X64ValueKind.Register && !isXmm,
                    X64OperandKind.Memory => operand.Kind is X64ValueKind.Memory,
                    X64OperandKind.RegisterOrMemory => (operand.Kind is X64ValueKind.Register && !isXmm) || operand.Kind is X64ValueKind.Memory,
                    X64OperandKind.Immediate => operand.Kind is X64ValueKind.Immediate,
                    X64OperandKind.Relative => operand.Kind is X64ValueKind.Label,

                    X64OperandKind.XmmRegister => isXmm,
                    X64OperandKind.XmmRegisterOrMemory => isXmm || operand.Kind == X64ValueKind.Memory,

                    _ => false,
                };

                if (!matches) return false;

                if (spec.FixedRegister is X64Register register && (operand.Kind != X64ValueKind.Register || operand.Register != register)) return false;

                if (spec.FixedImmediate is long value && (operand.Kind != X64ValueKind.Immediate || operand.Value != value)) return false;

                if (operand.Kind is not X64ValueKind.Register and not X64ValueKind.Memory) continue;

                int actualSize = GetSize(operand.Size, 0);

                if (operand.Kind is X64ValueKind.Register && (operand.Register is null || operand.Size != X64Operand.GetSize(operand.Register.Value))) return false;

                if (isXmm) continue;
                
                if (spec.Size == X64Size.operand)
                {
                    if (actualSize is 0) continue;

                    if (size != 0 && size != actualSize) return false;

                    size = actualSize;
                }
                else
                {
                    int expectedSize = GetSize(spec.Size, 0);

                    if (actualSize != 0 && actualSize != expectedSize) return false;
                }
            }

            switch (form.SizeMode)
            {
                case X64OperandSizeMode.None:
                    break;

                case X64OperandSizeMode.Byte:
                    if (size != 0 && size != 1) return false;

                    size = 1;
                    break;

                case X64OperandSizeMode.Standard:
                    if (size != 2 && size != 4 && size != 8) return false;
                    break;

                case X64OperandSizeMode.Default64:
                    if (size is 0) size = 8;

                    if (size != 2 && size != 8) return false;

                    break;

                case X64OperandSizeMode.Fixed64:
                    if (size != 0 && size != 8) return false;

                    size = 8;
                    break;

                case X64OperandSizeMode.Integer32Or64:
                    if (size != 4 && size != 8) return false;
                    break;

                default: return false;
            }

            for (int i = 0; i < operands.Length; i++)
            {
                var spec = form.Operands[i];
                var operand = operands[i];

                if (spec.SmallerThanOperandSize && GetSize(spec.Size, size) >= size) return false;

                if (spec.Role is X64OperandRole.Immediate)
                {
                    int width = GetSize(spec.Size, size);

                    bool signed = spec.ImmediateRule == X64ImmediateRule.SignExtended || (spec.ImmediateRule is X64ImmediateRule.SignExtendedWhenOperand64 && size == 8);

                    if (!FitsImmediate(operand.Value, width, signed)) return false;
                }
                else if (spec.Role is X64OperandRole.Relative)
                {
                    if (GetSize(spec.Size, size) != 4 || operand.Name is null) return false;
                }
            }

            return true;
        }
        private void EncodeInstruction(X64Form form, X64Operand[] operands, int size)
        {
            List<byte> bytes = new();

            (int Offset, int Size, string Name)? fixup = null;

            bool prefix = (form.Prefixes & X64Prefixes.OperandSize) != 0 || (size == 2 &&
                (form.SizeMode is X64OperandSizeMode.Standard || form.SizeMode is X64OperandSizeMode.Default64));

            if (prefix) bytes.Add(0x66);
            if ((form.Prefixes & X64Prefixes.Repne) != 0) bytes.Add(0xF2);
            if ((form.Prefixes & X64Prefixes.Rep) != 0) bytes.Add(0xF3);

            byte rex = 0x40;

            if(form.SizeMode is X64OperandSizeMode.Fixed64 || (form.SizeMode is X64OperandSizeMode.Standard or X64OperandSizeMode.Integer32Or64 && size == 8))
            {
                rex |= 0x08;
            }

            for(int i = 0; i < operands.Length; i++)
            {
                var spec = form.Operands[i];
                var operand = operands[i];

                if (operand.Kind is not X64ValueKind.Register and not X64ValueKind.Memory) continue;

                var register = operand.Register!.Value;
                int code = GetRegisterCode(register);

                switch (spec.Role)
                {
                    case X64OperandRole.ModRmReg:
                        if (code >= 8)
                            rex |= 0x04;
                        break;
                    case X64OperandRole.ModRmRm:
                    case X64OperandRole.OpcodeRegister:
                        if (code >= 8)
                            rex |= 0x01;
                        break;
                }
            }

            if (rex != 0x40) bytes.Add(rex);

            switch (form.Map)
            {
                case X64OpcodeMap.Primary:
                    break;

                case X64OpcodeMap.Map0F:
                    bytes.Add(0x0F);
                    break;

                default: throw new NotSupportedException();
            }

            int opcode = form.Opcode;

            if (form.OpcodePlusRegister)
            {
                var operand = GetOperand(form, operands, X64OperandRole.OpcodeRegister);

                opcode += GetRegisterCode(operand.Register!.Value) & 7;
            }

            bytes.Add(checked((byte)opcode));

            if (form.FixedModRm >= 0)
            {
                bytes.Add(checked((byte)form.FixedModRm));
            }
            else if (form.HasModRm)
            {
                int regField = form.Extension;

                if (regField < 0)
                {
                    var register = GetOperand(form, operands, X64OperandRole.ModRmReg);

                    regField = GetRegisterCode(register.Register!.Value);
                }

                var rm = GetOperand(form, operands, X64OperandRole.ModRmRm);

                EncodeModRm(bytes, regField, rm);
            }

            for (int i = 0; i < operands.Length; i++)
            {
                var spec = form.Operands[i];
                var operand = operands[i];

                if (spec.Role is X64OperandRole.Immediate) EncodeInteger(bytes, operand.Value, GetSize(spec.Size, size));
                else if (spec.Role is X64OperandRole.Relative)
                {
                    int width = GetSize(spec.Size, size);

                    if (fixup is not null) throw new ArgumentException();

                    fixup = (bytes.Count, width, operand.Name!);
                    EncodeInteger(bytes, 0, width);
                }
            }

            int start = _code.Count;
            int nextInstruction = checked(start + bytes.Count);

            _code.AddRange(bytes);

            if (fixup is not null)
                _labelFixups.Add(new(fixup.Value.Size, start + fixup.Value.Offset, nextInstruction, fixup.Value.Name));
        }
        public void EncodeModRm(List<byte> bytes, int regField, X64Operand operand)
        {
            regField &= 7;

            if (operand.Kind == X64ValueKind.Register)
            {
                int rm = GetRegisterCode(operand.Register!.Value);

                bytes.Add((byte)(0xC0 | (regField << 3) | rm));
                return;
            }
            X64Register baseRegister = operand.Register!.Value;

            if (X64Operand.GetSize(baseRegister) != X64Size.qword) throw new ArgumentException();

            int baseCode = GetRegisterCode(baseRegister);
            int offset = operand.Offset;
            int mod;

            if (offset == 0 && baseCode != 5) mod = 0;
            else if (offset >= -128 && offset <= 127) mod = 1;
            else mod = 2;

            bytes.Add((byte)((mod << 6) | (regField << 3) | baseCode));

            if (baseCode is 4) bytes.Add(0x24);

            if (mod is 1) EncodeInteger(bytes, offset, 1);
            else if (mod is 2) EncodeInteger(bytes, offset, 4);
        }
        public void EncodeImportCall(X64Operand operand)
        {
            if (!_dlls.TryGetValue(operand.Name!, out string? dll)) throw new ArgumentException($"unknown import: {operand.Name!}");

            _code.Add(0xFF);
            _code.Add(0x15);

            _importFixups.Add(new(_code.Count, dll, operand.Name!));

            EncodeInteger(_code, 0, 4);
        }
        private X64Operand GetOperand(X64Form form, X64Operand[] operands, X64OperandRole role)
        {
            for (int i = 0; i < operands.Length; i++)
            {
                if (form.Operands[i].Role == role)
                    return operands[i];
            }
            throw new InvalidOperationException($"Missing operand with role: {role}");
        }
        private int GetSize(X64Size size, int operandSize) => size switch
        {
            X64Size.none => 0,
            X64Size.@byte => 1,
            X64Size.word => 2,
            X64Size.dword => 4,
            X64Size.qword => 8,

            X64Size.oword => 16,

            X64Size.operand => operandSize,
            X64Size.immediate => operandSize is 2 ? 2 : 4,

            _ => throw new ArgumentOutOfRangeException(nameof(size))
        };
        private bool FitsImmediate(long value, int size, bool signed) => size switch
        {
            1 => signed ? value >= sbyte.MinValue && value <= sbyte.MaxValue : value >= sbyte.MinValue && value <= byte.MaxValue,
            2 => signed ? value >= short.MinValue && value <= short.MaxValue : value >= short.MinValue && value <= ushort.MaxValue,
            4 => signed ? value >= int.MinValue && value <= int.MaxValue : value >= int.MinValue && value <= uint.MaxValue,
            8 => true,

            _ => false
        };
        private void EncodeInteger(List<byte> bytes, long value, int size)
        {
            Span<byte> buffer = stackalloc byte[8];

            BinaryPrimitives.WriteInt64LittleEndian(buffer, value);

            bytes.AddRange(buffer[..size]);
        }
        private byte GetRegisterCode(X64Register reg) => reg switch
        {
            X64Register.rax or X64Register.eax or X64Register.ax or X64Register.al => 0,
            X64Register.rcx or X64Register.ecx or X64Register.cx or X64Register.cl => 1,
            X64Register.rdx or X64Register.edx or X64Register.dx or X64Register.dl => 2,
            X64Register.rbx or X64Register.ebx or X64Register.bx or X64Register.bl => 3,
            X64Register.rsp or X64Register.esp or X64Register.sp or X64Register.spl => 4,
            X64Register.rbp or X64Register.ebp or X64Register.bp or X64Register.bpl => 5,
            X64Register.rsi or X64Register.esi or X64Register.si or X64Register.sil => 6,
            X64Register.rdi or X64Register.edi or X64Register.di or X64Register.dil => 7,
            X64Register.r8 or X64Register.r8d or X64Register.r8w or X64Register.r8b => 8,
            X64Register.r9 or X64Register.r9d or X64Register.r9w or X64Register.r9b => 9,
            X64Register.r10 or X64Register.r10d or X64Register.r10w or X64Register.r10b => 10,
            X64Register.r11 or X64Register.r11d or X64Register.r11w or X64Register.r11b => 11,
            X64Register.r12 or X64Register.r12d or X64Register.r12w or X64Register.r12b => 12,
            X64Register.r13 or X64Register.r13d or X64Register.r13w or X64Register.r13b => 13,
            X64Register.r14 or X64Register.r14d or X64Register.r14w or X64Register.r14b => 14,
            X64Register.r15 or X64Register.r15d or X64Register.r15w or X64Register.r15b => 15,

            >= X64Register.xmm0 and <= X64Register.xmm15 => (byte)((int)reg - (int)X64Register.xmm0),

           _ => throw new ArgumentException($"Unknow register: {reg}")
        };
    }
}
