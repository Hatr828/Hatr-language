namespace Best_language.src.back.Instructions
{
    public enum X64Register : byte
    {
        rax, rcx, rdx, rbx, rbs, rsp, rbp, rsi, rdi, r8,
        edx, ecx, r8d,
        dx, cx, r8w,
        al, cl, r8b
    }

    public enum X64Opcode
    {
        mov,
        movzx,
        or,
        and,
        xor,
        shl,
        sar,
        neg,
        not,
        cmp,
        cqo,
        push,
        sub,
        add,
        imul,
        idiv,
        label,
        test,
        jnz,
        jz,
        setg,
        setge,
        setl,
        setle,
        sete,
        setne,
        jmp,
        call,
        ret
    }

    public enum X64Size
    {
        none,
        @byte,
        word,
        dword,
        qword,

        operand,
        immediate
    }

    public enum X64OperandSizeMode
    {
        None,
        Byte,
        Standard,
        Default64,
        Fixed64
    }

    public enum X64OperandKind
    {
        Register,
        RegisterOrMemory,
        Memory,
        Immediate,
        Relative,
    }

    public enum X64OperandRole
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
    public enum X64OpcodeMap
    {
        Primary,
        Map0F
    }
    [Flags]
    public enum X64Prefixes
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
            >= X64Register.rax and <= X64Register.rdi => X64Size.qword,
            X64Register.ecx or X64Register.edx => X64Size.dword,
            X64Register.dx or X64Register.cx => X64Size.word,
            X64Register.al or X64Register.cl or X64Register.r8b => X64Size.@byte,

            _ => throw new NotSupportedException()
        };
    }
    public class X64Instruction(X64Opcode opcode, params X64Operand[] operands)
    {
        public X64Opcode Opcode { get; } = opcode;
        public X64Operand[] Operands { get; } = operands;
    }
}
