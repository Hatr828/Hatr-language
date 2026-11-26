namespace Best_language.src.back.Instructions
{
    public enum X64Register : byte
    {
        rax, rcx, rdx, rbx, rbs, rsp, rbp, rsi, rdi,
        r8, r9, r10, r11, r12, r13, r14, r15,

        eax, ecx, edx, ebx, esp, ebp, esi, edi,
        r8d, r9d, r10d, r11d, r12d, r13d, r14d, r15d,

        ax, cx, dx, bx, sp, bp, si, di,
        r8w, r9w, r10w, r11w, r12w, r13w, r14w, r15w,

        al, cl, dl, bl, spl, bpl, sil, dil,
        r8b, r9b, r10b, r11b, r12b, r13b, r14b, r15b,

        xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7, xmm8, xmm9, xmm10, xmm11, xmm12, xmm13, xmm14, xmm15,
    }

    public enum X64Opcode : short
    {
        // 0 operands
        cqo, ret,
        // 1 operands
        push, pop,
        not, neg,
        idiv,
        jnz, jz, jmp,
        call, 
        label,

        setg, setge,
        setl, setle,
        sete, setne,
        seta, setae,
        setb, setbe,
        setp, setnp,

        // 2 operands
        mov, movzx, movsxd, movsx,
        lea,
        or, and, xor,
        shl, sar,
        cmp, test,
        sub, add, shr,

        // varies
        imul,

        // float/double 2 operands

        movss, movsd,
        addss, addsd,
        subss, subsd,
        mulss, mulsd,
        divss, divsd,
        ucomiss, ucomisd,
        cvtsi2ss, cvtsi2sd,
        cvtsd2ss, cvtss2sd,
        cvttss2si, cvttsd2si,
        xorps, xorpd,
        movd, movq,
    }

    public enum X64Size : byte
    {
        none,
        @byte = 1,  word = 2, dword = 4, qword = 8, oword = 16,  // Do not change numbers

        custom,

        operand,
        immediate
    }

    public enum X64OperandSizeMode : byte
    {
        None,
        Byte,
        Standard,
        Default64,
        Integer32Or64,
        Fixed64
    }

    public enum X64OperandKind : byte
    {
        Register,
        RegisterOrMemory,
        Memory,
        Immediate,
        Relative,

        XmmRegisterOrMemory,
        XmmRegister,
    }

    public enum X64OperandRole : byte
    {
        ModRmReg,
        ModRmRm,
        OpcodeRegister,
        Immediate,
        Relative,
        Fixed
    }
    public enum X64ImmediateRule
    {
        BitPattern,
        SignExtended,
        SignExtendedWhenOperand64,
    }
    public enum X64OpcodeMap : byte
    {
        Primary,
        Map0F
    }
    [Flags]
    public enum X64Prefixes : byte
    {
        None = 0,
        OperandSize = 1,
        Repne = 2, // REPeat REPeat while Not Equal
        Rep = 4 // REPeat same opcode as for REPE  REPeat while Equal (0xF3)
    }
    public struct PeImport(int offset, string dll, string name)
    {
        public int Offset = offset;
        public string DLL = dll;
        public string Name = name;
    }
    public struct X64Label(int size, int offset, int nextInstruction, string name)
    {
        public int Size { get; } = size;
        public int Offset { get; } = offset;
        public int NextInstruction { get; } = nextInstruction;
        public string Name { get; } = name;
    }
    public class X64OperandSpec
    {
        public X64OperandKind Kind { get; init; }
        public X64OperandRole Role { get; init; }
        public X64Size Size { get; init; }
        public X64ImmediateRule ImmediateRule { get; init; }

        public X64Register? FixedRegister { get; init; }
        public long? FixedImmediate { get; init; }
        public bool SmallerThanOperandSize { get; init; }
    }

    public class X64Form
    {
        public X64Opcode Kind { get; init; }
        public X64OpcodeMap Map { get; init; }
        public byte Opcode { get; init; }

        public X64Prefixes Prefixes { get; init; }
        public X64OperandSizeMode SizeMode { get; init; }

        public int Extension { get; init; } = -1;
        public int FixedModRm { get; init; } = -1;

        public bool OpcodePlusRegister { get; init; }
        public bool OpcodePlusCondition { get; init; }
        public bool HasModRm
        {
            get
            {
                if (Extension >= 0 || FixedModRm >= 0) return true;

                foreach (var operand in Operands)
                {
                    if (operand.Role is X64OperandRole.ModRmReg or X64OperandRole.ModRmRm) return true;
                }

                return false;
            }
        }

        public X64OperandSpec[] Operands { get; init; } = [];
    }

    public enum X64ValueKind
    {
        Register, Memory, Immediate, Label, Symbol
    }
    public struct X64Operand(X64ValueKind kind, X64Size size, X64Register register = 0,
        int offset = 0, long value = 0, string? name = null)
    {
        public X64ValueKind Kind { get; init; } = kind;
        public X64Size Size { get; init; } = size;

        public X64Register? Register { get; } = register;
        public int Offset { get; } = offset;

        public long Value { get; } = value;
        public string? Name { get; } = name;

        public static X64Operand Reg(X64Register register) => new(X64ValueKind.Register, GetSize(register), register);
        public static X64Operand Mem(X64Register register, int offset, X64Size size) => new(X64ValueKind.Memory, size, register, offset);
        public static X64Operand Imm(X64Size size, long value) => new(X64ValueKind.Immediate, size, value: value);
        public static X64Operand Label(string name) => new(X64ValueKind.Label, 0, name: name);
        public static X64Operand Symbol(string name) => new(X64ValueKind.Symbol, 0, name: name);

        public static X64Size GetSize(X64Register reg) => reg switch
        {
            >= X64Register.rax and <= X64Register.r15 => X64Size.qword,
            >= X64Register.eax and <= X64Register.r15d => X64Size.dword,
            >= X64Register.ax and <= X64Register.r15w => X64Size.word,
            >= X64Register.al and <= X64Register.r15b => X64Size.@byte,

            >= X64Register.xmm0 and <= X64Register.xmm15 => X64Size.oword,

            _ => throw new NotSupportedException()
        };
    }
    public class X64Instruction(X64Opcode opcode, params X64Operand[] operands)
    {
        public X64Opcode Opcode { get; } = opcode;
        public X64Operand[] Operands { get; } = operands;
    }
}
