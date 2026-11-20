using Best_language.src.back.Instructions;

namespace Best_language.src.back.Encoders
{
    public static class X64InstructionTable
    {
        private static readonly Dictionary<X64Opcode, List<X64Form>> _forms = new();

        static X64InstructionTable()
        {
            // mov r/m 8, r 8
            Add(new X64Form
            {
                Kind = X64Opcode.mov,
                Opcode = 0x88,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.RegisterOrMemory,
                        Role = X64OperandRole.ModRmRm,
                        Size = X64Size.@byte
                    },
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.ModRmReg,
                        Size = X64Size.@byte
                    }
                ]
            });

            // mov r/m 16/32/64, r 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.mov,
                Opcode = 0x89,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.RegisterOrMemory,
                        Role = X64OperandRole.ModRmRm,
                        Size = X64Size.operand
                    },
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.ModRmReg,
                        Size = X64Size.operand
                    }
                ]
            });

            // mov r 8, r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.mov,
                Opcode = 0x8A,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.ModRmReg,
                        Size = X64Size.@byte
                    },
                    new()
                    {
                        Kind = X64OperandKind.RegisterOrMemory,
                        Role = X64OperandRole.ModRmRm,
                        Size = X64Size.@byte
                    }
                ]
            });

            // mov r 16/32/64, r/m 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.mov,
                Opcode = 0x8B,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.ModRmReg,
                        Size = X64Size.operand
                    },
                    new()
                    {
                        Kind = X64OperandKind.RegisterOrMemory,
                        Role = X64OperandRole.ModRmRm,
                        Size = X64Size.operand
                    }
                ]
            });

            // mov r 8, imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.mov,
                Opcode = 0xB0,
                OpcodePlusRegister = true,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.OpcodeRegister,
                        Size = X64Size.@byte
                    },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.@byte
                    }
                ]
            });

            // mov r/m 8, imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.mov,
                Opcode = 0xC6,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.RegisterOrMemory,
                        Role = X64OperandRole.ModRmRm,
                        Size = X64Size.@byte
                    },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.@byte
                    }
                ]
            });

            // mov r/m 16, imm 16
            // mov r/m 32, imm 32
            // mov r/m 64, sign-extended imm 32
            Add(new X64Form
            {
                Kind = X64Opcode.mov,
                Opcode = 0xC7,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.RegisterOrMemory,
                        Role = X64OperandRole.ModRmRm,
                        Size = X64Size.operand
                    },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.immediate,
                        ImmediateRule = X64ImmediateRule.SignExtendedWhenOperand64
                    }
                ]
            });

            // mov r 16, imm 16
            // mov r 32, imm 32
            // mov r 64, imm 64
            Add(new X64Form
            {
                Kind = X64Opcode.mov,
                Opcode = 0xB8,
                OpcodePlusRegister = true,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.OpcodeRegister,
                        Size = X64Size.operand
                    },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.operand
                    }
                ]
            });

            // movsx r 16/32/64, r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.movsx,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0xBE,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.ModRmReg,
                        Size = X64Size.operand
                    },
                    new()
                    {
                        Kind = X64OperandKind.RegisterOrMemory,
                        Role = X64OperandRole.ModRmRm,
                        Size = X64Size.@byte,
                        SmallerThanOperandSize = true
                    }
                ]
            });

            // movsx r 32/64, r/m 16
            Add(new X64Form
            {
                Kind = X64Opcode.movsx,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0xBF,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.ModRmReg,
                        Size = X64Size.operand
                    },
                    new()
                    {
                        Kind = X64OperandKind.RegisterOrMemory,
                        Role = X64OperandRole.ModRmRm,
                        Size = X64Size.word,
                        SmallerThanOperandSize = true
                    }
                           ]
            });

            // movsxd r 64, r/m 32
            Add(new X64Form
            {
                Kind = X64Opcode.movsxd,
                Map = X64OpcodeMap.Primary,
                Opcode = 0x63,
                SizeMode = X64OperandSizeMode.Fixed64,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.ModRmReg,
                        Size = X64Size.qword
                    },
                    new()
                    {
                        Kind = X64OperandKind.RegisterOrMemory,
                        Role = X64OperandRole.ModRmRm,
                        Size = X64Size.dword
                    }
                ]
            });

            // movzx r 16/32/64, r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.movzx,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0xB6,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.ModRmReg,
                        Size = X64Size.operand
                    },
                    new()
                    {
                        Kind = X64OperandKind.RegisterOrMemory,
                        Role = X64OperandRole.ModRmRm,
                        Size = X64Size.@byte,
                        SmallerThanOperandSize = true
                    }
                ]
            });

            // movzx r 32/64, r/m 16
            Add(new X64Form
            {
                Kind = X64Opcode.movzx,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0xB7,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.ModRmReg,
                        Size = X64Size.operand
                    },
                    new()
                    {
                        Kind = X64OperandKind.RegisterOrMemory,
                        Role = X64OperandRole.ModRmRm,
                        Size = X64Size.word,
                        SmallerThanOperandSize = true
                    }
                ]
            });

            // shr r/m 16/32/64, imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.shr,
                Opcode = 0xC1,
                Extension = 5,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                new()
                {
                    Kind = X64OperandKind.RegisterOrMemory,
                    Role = X64OperandRole.ModRmRm,
                    Size = X64Size.operand
                },
                new()
                {
                    Kind = X64OperandKind.Immediate,
                    Role = X64OperandRole.Immediate,
                    Size = X64Size.@byte
                }]
             });

            // or r/m 8, r 8
            Add(new X64Form
            {
                Kind = X64Opcode.or,
                Opcode = 0x08,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.@byte }
                ]
            });

            // or r/m 16/32/64, r 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.or,
                Opcode = 0x09,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand }
                ]
            });

            // or r 8, r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.or,
                Opcode = 0x0A,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // or r 16/32/64, r/m 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.or,
                Opcode = 0x0B,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand }
                ]
            });

            // or r/m 8, imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.or,
                Opcode = 0x80,
                Extension = 1,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Immediate, Role = X64OperandRole.Immediate, Size = X64Size.@byte }
                ]
            });

            // or r/m 16/32/64, sign-extended imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.or,
                Opcode = 0x83,
                Extension = 1,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.@byte,
                        ImmediateRule = X64ImmediateRule.SignExtended
                    }
                ]
            });

            // or r/m 16, imm 16
            // or r/m 32, imm 32
            // or r/m 64, sign-extended imm 32
            Add(new X64Form
            {
                Kind = X64Opcode.or,
                Opcode = 0x81,
                Extension = 1,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.immediate,
                        ImmediateRule = X64ImmediateRule.SignExtendedWhenOperand64
                    }
                ]
            });

            // and r/m 8, r 8
            Add(new X64Form
            {
                Kind = X64Opcode.and,
                Opcode = 0x20,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.@byte }
                ]
            });

            // and r/m 16/32/64, r 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.and,
                Opcode = 0x21,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand }
                ]
            });

            // and r 8, r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.and,
                Opcode = 0x22,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // and r 16/32/64, r/m 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.and,
                Opcode = 0x23,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand }
                ]
            });

            // and r/m 8, imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.and,
                Opcode = 0x80,
                Extension = 4,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Immediate, Role = X64OperandRole.Immediate, Size = X64Size.@byte }
                ]
            });

            // and r/m 16/32/64, sign-extended imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.and,
                Opcode = 0x83,
                Extension = 4,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.@byte,
                        ImmediateRule = X64ImmediateRule.SignExtended
                    }
                ]
            });

            // and r/m 16, imm 16
            // and r/m 32, imm 32
            // and r/m 64, sign-extended imm 32
            Add(new X64Form
            {
                Kind = X64Opcode.and,
                Opcode = 0x81,
                Extension = 4,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.immediate,
                        ImmediateRule = X64ImmediateRule.SignExtendedWhenOperand64
                    }
                ]
            });

            // xor r/m 8, r 8
            Add(new X64Form
            {
                Kind = X64Opcode.xor,
                Opcode = 0x30,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.@byte }
                ]
            });

            // xor r/m 16/32/64, r 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.xor,
                Opcode = 0x31,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand }
                ]
            });

            // xor r 8, r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.xor,
                Opcode = 0x32,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // xor r 16/32/64, r/m 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.xor,
                Opcode = 0x33,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand }
                ]
            });

            // xor r/m 8, imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.xor,
                Opcode = 0x80,
                Extension = 6,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Immediate, Role = X64OperandRole.Immediate, Size = X64Size.@byte }
                ]
            });

            // xor r/m 16/32/64, sign-extended imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.xor,
                Opcode = 0x83,
                Extension = 6,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.@byte,
                        ImmediateRule = X64ImmediateRule.SignExtended
                    }
                ]
            });

            // xor r/m 16, imm 16
            // xor r/m 32, imm 32
            // xor r/m 64, sign-extended imm 32
            Add(new X64Form
            {
                Kind = X64Opcode.xor,
                Opcode = 0x81,
                Extension = 6,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.immediate,
                        ImmediateRule = X64ImmediateRule.SignExtendedWhenOperand64
                    }
                ]
            });

            // shl r/m 8, 1
            Add(new X64Form
            {
                Kind = X64Opcode.shl,
                Opcode = 0xD0,
                Extension = 4,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Fixed,
                        Size = X64Size.@byte,
                        FixedImmediate = 1
                    }
                ]
            });

            // shl r/m 16/32/64, 1
            Add(new X64Form
            {
                Kind = X64Opcode.shl,
                Opcode = 0xD1,
                Extension = 4,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Fixed,
                        Size = X64Size.@byte,
                        FixedImmediate = 1
                    }
                ]
            });

            // shl r/m 8, cl
            Add(new X64Form
            {
                Kind = X64Opcode.shl,
                Opcode = 0xD2,
                Extension = 4,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.Fixed,
                        Size = X64Size.@byte,
                        FixedRegister = X64Register.cl
                    }
                ]
            });

            // shl r/m 16/32/64, cl
            Add(new X64Form
            {
                Kind = X64Opcode.shl,
                Opcode = 0xD3,
                Extension = 4,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.Fixed,
                        Size = X64Size.@byte,
                        FixedRegister = X64Register.cl
                    }
                ]
            });

            // shl r/m 8, imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.shl,
                Opcode = 0xC0,
                Extension = 4,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Immediate, Role = X64OperandRole.Immediate, Size = X64Size.@byte }
                ]
            });

            // shl r/m 16/32/64, imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.shl,
                Opcode = 0xC1,
                Extension = 4,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.Immediate, Role = X64OperandRole.Immediate, Size = X64Size.@byte }
                ]
            });

            // sar r/m 8, 1
            Add(new X64Form
            {
                Kind = X64Opcode.sar,
                Opcode = 0xD0,
                Extension = 7,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Fixed,
                        Size = X64Size.@byte,
                        FixedImmediate = 1
                    }
                ]
            });

            // sar r/m 16/32/64, 1
            Add(new X64Form
            {
                Kind = X64Opcode.sar,
                Opcode = 0xD1,
                Extension = 7,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Fixed,
                        Size = X64Size.@byte,
                        FixedImmediate = 1
                    }
                ]
            });

            // sar r/m 8, cl
            Add(new X64Form
            {
                Kind = X64Opcode.sar,
                Opcode = 0xD2,
                Extension = 7,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.Fixed,
                        Size = X64Size.@byte,
                        FixedRegister = X64Register.cl
                    }
                ]
            });

            // sar r/m 16/32/64, cl
            Add(new X64Form
            {
                Kind = X64Opcode.sar,
                Opcode = 0xD3,
                Extension = 7,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.Fixed,
                        Size = X64Size.@byte,
                        FixedRegister = X64Register.cl
                    }
                ]
            });

            // sar r/m 8, imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.sar,
                Opcode = 0xC0,
                Extension = 7,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Immediate, Role = X64OperandRole.Immediate, Size = X64Size.@byte }
                ]
            });

            // sar r/m 16/32/64, imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.sar,
                Opcode = 0xC1,
                Extension = 7,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.Immediate, Role = X64OperandRole.Immediate, Size = X64Size.@byte }
                ]
            });

            // neg r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.neg,
                Opcode = 0xF6,
                Extension = 3,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // neg r/m 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.neg,
                Opcode = 0xF7,
                Extension = 3,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand }
                ]
            });

            // not r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.not,
                Opcode = 0xF6,
                Extension = 2,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // not r/m 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.not,
                Opcode = 0xF7,
                Extension = 2,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand }
                ]
            });

            // cmp r/m 8, r 8
            Add(new X64Form
            {
                Kind = X64Opcode.cmp,
                Opcode = 0x38,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.@byte }
                ]
            });

            // cmp r/m 16/32/64, r 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.cmp,
                Opcode = 0x39,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand }
                ]
            });

            // cmp r 8, r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.cmp,
                Opcode = 0x3A,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // cmp r 16/32/64, r/m 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.cmp,
                Opcode = 0x3B,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand }
                ]
            });

            // cmp r/m 8, imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.cmp,
                Opcode = 0x80,
                Extension = 7,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Immediate, Role = X64OperandRole.Immediate, Size = X64Size.@byte }
                ]
            });

            // cmp r/m 16/32/64, sign-extended imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.cmp,
                Opcode = 0x83,
                Extension = 7,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.@byte,
                        ImmediateRule = X64ImmediateRule.SignExtended
                    }
                ]
            });

            // cmp r/m 16, imm 16
            // cmp r/m 32, imm 32
            // cmp r/m 64, sign-extended imm 32
            Add(new X64Form
            {
                Kind = X64Opcode.cmp,
                Opcode = 0x81,
                Extension = 7,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.immediate,
                        ImmediateRule = X64ImmediateRule.SignExtendedWhenOperand64
                    }
                ]
            });

            // cqo
            Add(new X64Form
            {
                Kind = X64Opcode.cqo,
                Opcode = 0x99,
                SizeMode = X64OperandSizeMode.Fixed64
            });

            // push r 16/64
            Add(new X64Form
            {
                Kind = X64Opcode.push,
                Opcode = 0x50,
                OpcodePlusRegister = true,
                SizeMode = X64OperandSizeMode.Default64,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.OpcodeRegister, Size = X64Size.operand }
                ]
            });

            // push r/m 16/64
            Add(new X64Form
            {
                Kind = X64Opcode.push,
                Opcode = 0xFF,
                Extension = 6,
                SizeMode = X64OperandSizeMode.Default64,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand }
                ]
            });

            // push sign-extended imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.push,
                Opcode = 0x6A,
                SizeMode = X64OperandSizeMode.Default64,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.@byte,
                        ImmediateRule = X64ImmediateRule.SignExtended
                    }
                ]
            });

            // push sign-extended imm 32
            Add(new X64Form
            {
                Kind = X64Opcode.push,
                Opcode = 0x68,
                SizeMode = X64OperandSizeMode.Default64,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.dword,
                        ImmediateRule = X64ImmediateRule.SignExtended
                    }
                ]
            });

            // sub r/m 8, r 8
            Add(new X64Form
            {
                Kind = X64Opcode.sub,
                Opcode = 0x28,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.@byte }
                ]
            });

            // sub r/m 16/32/64, r 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.sub,
                Opcode = 0x29,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand }
                ]
            });

            // sub r 8, r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.sub,
                Opcode = 0x2A,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // sub r 16/32/64, r/m 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.sub,
                Opcode = 0x2B,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand }
                ]
            });

            // sub r/m 8, imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.sub,
                Opcode = 0x80,
                Extension = 5,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Immediate, Role = X64OperandRole.Immediate, Size = X64Size.@byte }
                ]
            });

            // sub r/m 16/32/64, sign-extended imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.sub,
                Opcode = 0x83,
                Extension = 5,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.@byte,
                        ImmediateRule = X64ImmediateRule.SignExtended
                    }
                ]
            });

            // sub r/m 16, imm 16
            // sub r/m 32, imm 32
            // sub r/m 64, sign-extended imm 32
            Add(new X64Form
            {
                Kind = X64Opcode.sub,
                Opcode = 0x81,
                Extension = 5,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.immediate,
                        ImmediateRule = X64ImmediateRule.SignExtendedWhenOperand64
                    }
                ]
            });

            // add r/m 8, r 8
            Add(new X64Form
            {
                Kind = X64Opcode.add,
                Opcode = 0x00,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.@byte }
                ]
            });

            // add r/m 16/32/64, r 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.add,
                Opcode = 0x01,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand }
                ]
            });

            // add r 8, r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.add,
                Opcode = 0x02,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // add r 16/32/64, r/m 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.add,
                Opcode = 0x03,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand }
                ]
            });

            // add r/m 8, imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.add,
                Opcode = 0x80,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Immediate, Role = X64OperandRole.Immediate, Size = X64Size.@byte }
                ]
            });

            // add r/m 16/32/64, sign-extended imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.add,
                Opcode = 0x83,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.@byte,
                        ImmediateRule = X64ImmediateRule.SignExtended
                    }
                ]
            });

            // add r/m 16, imm 16
            // add r/m 32, imm 32
            // add r/m 64, sign-extended imm 32
            Add(new X64Form
            {
                Kind = X64Opcode.add,
                Opcode = 0x81,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.immediate,
                        ImmediateRule = X64ImmediateRule.SignExtendedWhenOperand64
                    }
                ]
            });

            // imul r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.imul,
                Opcode = 0xF6,
                Extension = 5,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // imul r/m 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.imul,
                Opcode = 0xF7,
                Extension = 5,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand }
                ]
            });

            // imul r 16/32/64, r/m 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.imul,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0xAF,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand }
                ]
            });

            // imul r 16/32/64, r/m 16/32/64, sign-extended imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.imul,
                Opcode = 0x6B,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.@byte,
                        ImmediateRule = X64ImmediateRule.SignExtended
                    }
                ]
            });

            // imul r 16, r/m 16, imm 16
            // imul r 32, r/m 32, imm 32
            // imul r 64, r/m 64, sign-extended imm 32
            Add(new X64Form
            {
                Kind = X64Opcode.imul,
                Opcode = 0x69,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.immediate,
                        ImmediateRule = X64ImmediateRule.SignExtendedWhenOperand64
                    }
                ]
            });

            // idiv r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.idiv,
                Opcode = 0xF6,
                Extension = 7,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // idiv r/m 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.idiv,
                Opcode = 0xF7,
                Extension = 7,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand }
                ]
            });

            // lea r 64, m
            Add(new X64Form
            {
                Kind = X64Opcode.lea,
                Opcode = 0x8D,
                SizeMode = X64OperandSizeMode.Fixed64,

                Operands = [
                    new()
                    {
                        Kind = X64OperandKind.Register,
                        Role = X64OperandRole.ModRmReg,
                        Size = X64Size.qword
                    },
                    new()
                    {
                        Kind = X64OperandKind.Memory,
                        Role = X64OperandRole.ModRmRm,
                        Size = X64Size.qword
                    }
                ]
            });

            // test r/m 8, r 8
            Add(new X64Form
            {
                Kind = X64Opcode.test,
                Opcode = 0x84,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.@byte }
                ]
            });

            // test r/m 16/32/64, r 16/32/64
            Add(new X64Form
            {
                Kind = X64Opcode.test,
                Opcode = 0x85,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand }
                ]
            });

            // test r/m 8, imm 8
            Add(new X64Form
            {
                Kind = X64Opcode.test,
                Opcode = 0xF6,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte },
                    new() { Kind = X64OperandKind.Immediate, Role = X64OperandRole.Immediate, Size = X64Size.@byte }
                ]
            });

            // test r/m 16, imm 16
            // test r/m 32, imm 32
            // test r/m 64, sign-extended imm 32 
            Add(new X64Form
            {
                Kind = X64Opcode.test,
                Opcode = 0xF7,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Standard,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand },
                    new()
                    {
                        Kind = X64OperandKind.Immediate,
                        Role = X64OperandRole.Immediate,
                        Size = X64Size.immediate,
                        ImmediateRule = X64ImmediateRule.SignExtendedWhenOperand64
                    }
                ]
            });

            // jnz rel 32
            Add(new X64Form
            {
                Kind = X64Opcode.jnz,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x85,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.Relative, Role = X64OperandRole.Relative, Size = X64Size.dword }
                ]
            });

            // setg r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.setg,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x9F,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // setge r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.setge,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x9D,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // setl r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.setl,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x9C,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // setle r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.setle,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x9E,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // sete r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.sete,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x94,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // setne r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.setne,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x95,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // jmp rel 32
            Add(new X64Form
            {
                Kind = X64Opcode.jmp,
                Opcode = 0xE9,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.Relative, Role = X64OperandRole.Relative, Size = X64Size.dword }
                ]
            });

            // jmp r/m 64
            Add(new X64Form
            {
                Kind = X64Opcode.jmp,
                Opcode = 0xFF,
                Extension = 4,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.qword }
                ]
            });

            // jz rel 32
            Add(new X64Form
            {
                Kind = X64Opcode.jz,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x84,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.Relative, Role = X64OperandRole.Relative, Size = X64Size.dword }
                ]
            });

            // call rel32
            Add(new X64Form
            {
                Kind = X64Opcode.call,
                Opcode = 0xE8,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.Relative, Role = X64OperandRole.Relative, Size = X64Size.dword }
                ]
            });

            // call r/m 64
            Add(new X64Form
            {
                Kind = X64Opcode.call,
                Opcode = 0xFF,
                Extension = 2,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.qword }
                ]
            });

            // ret
            Add(new X64Form
            {
                Kind = X64Opcode.ret,
                Opcode = 0xC3,
                SizeMode = X64OperandSizeMode.None
            });

            ////////// double/float

            // movss xmm, xmm/m 32
            Add(new X64Form
            {
                Kind = X64Opcode.movss,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x10,
                Prefixes = X64Prefixes.Rep,
                SizeMode = X64OperandSizeMode.None,

                Operands =
                [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.dword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.dword }
                ]
            });

            // movss m 32, xmm
            Add(new X64Form
            {
                Kind = X64Opcode.movss,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x11,
                Prefixes = X64Prefixes.Rep,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.Memory, Role = X64OperandRole.ModRmRm, Size = X64Size.dword },
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.dword }
                ]
            });

            // movsd xmm, xmm/m 64
            Add(new X64Form
            {
                Kind = X64Opcode.movsd,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x10,
                Prefixes = X64Prefixes.Repne,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.qword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.qword }
                ]
            });

            // movsd m 64, xmm
            Add(new X64Form
            {
                Kind = X64Opcode.movsd,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x11,
                Prefixes = X64Prefixes.Repne,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.Memory, Role = X64OperandRole.ModRmRm, Size = X64Size.qword },
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.qword }
                ]
            });

            // addss xmm, xmm/m 32
            Add(new X64Form
            {
                Kind = X64Opcode.addss,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x58,
                Prefixes = X64Prefixes.Rep,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.dword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.dword }
                ]
            });

            // addsd xmm, xmm/m 64
            Add(new X64Form
            {
                Kind = X64Opcode.addsd,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x58,
                Prefixes = X64Prefixes.Repne,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.qword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.qword }
                ]
            });

            // subss xmm, xmm/m 32
            Add(new X64Form
            {
                Kind = X64Opcode.subss,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x5C,
                Prefixes = X64Prefixes.Rep,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.dword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.dword }
                ]
            });

            // subsd xmm, xmm/m 64
            Add(new X64Form
            {
                Kind = X64Opcode.subsd,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x5C,
                Prefixes = X64Prefixes.Repne,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.qword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.qword }
                ]
            });

            // mulss xmm, xmm/m 32
            Add(new X64Form
            {
                Kind = X64Opcode.mulss,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x59,
                Prefixes = X64Prefixes.Rep,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.dword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.dword }
                ]
            });

            // mulsd xmm, xmm/m 64
            Add(new X64Form
            {
                Kind = X64Opcode.mulsd,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x59,
                Prefixes = X64Prefixes.Repne,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.qword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.qword }
                ]
            });

            // divss xmm, xmm/m 32
            Add(new X64Form
            {
                Kind = X64Opcode.divss,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x5E,
                Prefixes = X64Prefixes.Rep,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.dword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.dword }
                ]
            });

            // divsd xmm, xmm/m 64
            Add(new X64Form
            {
                Kind = X64Opcode.divsd,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x5E,
                Prefixes = X64Prefixes.Repne,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.qword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.qword }
                ]
            });

            // ucomiss xmm, xmm/m 32
            Add(new X64Form
            {
                Kind = X64Opcode.ucomiss,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x2E,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.dword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.dword }
                ]
            });

            // ucomisd xmm, xmm/m 64
            Add(new X64Form
            {
                Kind = X64Opcode.ucomisd,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x2E,
                Prefixes = X64Prefixes.OperandSize,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.qword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.qword }
                ]
            });

            // cvtsi2ss xmm, r/m 32
            // cvtsi2ss xmm, r/m 64
            Add(new X64Form
            {
                Kind = X64Opcode.cvtsi2ss,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x2A,
                Prefixes = X64Prefixes.Rep,
                SizeMode = X64OperandSizeMode.Integer32Or64,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.dword },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand }
                ]
            });

            // cvtsi2sd xmm, r/m 32
            // cvtsi2sd xmm, r/m 64
            Add(new X64Form
            {
                Kind = X64Opcode.cvtsi2sd,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x2A,
                Prefixes = X64Prefixes.Repne,
                SizeMode = X64OperandSizeMode.Integer32Or64,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.qword },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.operand }
                ]
            });

            // cvttss2si r 32, xmm/m 32
            // cvttss2si r 64, xmm/m 32
            Add(new X64Form
            {
                Kind = X64Opcode.cvttss2si,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x2C,
                Prefixes = X64Prefixes.Rep,
                SizeMode = X64OperandSizeMode.Integer32Or64,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.dword }
                ]
            });

            // cvttsd2si r 32, xmm/m 64
            // cvttsd2si r 64, xmm/m 64
            Add(new X64Form
            {
                Kind = X64Opcode.cvttsd2si,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x2C,
                Prefixes = X64Prefixes.Repne,
                SizeMode = X64OperandSizeMode.Integer32Or64,

                Operands = [
                    new() { Kind = X64OperandKind.Register, Role = X64OperandRole.ModRmReg, Size = X64Size.operand },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.qword }
                ]
            });

            // cvtss2sd xmm, xmm/m 32
            Add(new X64Form
            {
                Kind = X64Opcode.cvtss2sd,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x5A,
                Prefixes = X64Prefixes.Rep,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.qword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.dword }
                ]
            });

            // cvtsd2ss xmm, xmm/m 64
            Add(new X64Form
            {
                Kind = X64Opcode.cvtsd2ss,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x5A,
                Prefixes = X64Prefixes.Repne,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.dword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.qword }
                ]
            });

            // movd xmm, r/m 32
            Add(new X64Form
            {
                Kind = X64Opcode.movd,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x6E,
                Prefixes = X64Prefixes.OperandSize,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.dword },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.dword }
                ]
            });

            // movd r/m 32, xmm
            Add(new X64Form
            {
                Kind = X64Opcode.movd,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x7E,
                Prefixes = X64Prefixes.OperandSize,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.dword },
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.dword }
                ]
            });

            // movq xmm, r/m 64
            Add(new X64Form
            {
                Kind = X64Opcode.movq,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x6E,
                Prefixes = X64Prefixes.OperandSize,
                SizeMode = X64OperandSizeMode.Fixed64,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.qword },
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.qword }
                ]
            });

            // movq r/m 64, xmm
            Add(new X64Form
            {
                Kind = X64Opcode.movq,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x7E,
                Prefixes = X64Prefixes.OperandSize,
                SizeMode = X64OperandSizeMode.Fixed64,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.qword },
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.qword }
                ]
            });

            // movq xmm, xmm/m 64
            Add(new X64Form
            {
                Kind = X64Opcode.movq,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x7E,
                Prefixes = X64Prefixes.Rep,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.qword },
                new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.qword }
                ]
            });

            // movq m 64, xmm
            Add(new X64Form
            {
                Kind = X64Opcode.movq,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0xD6,
                Prefixes = X64Prefixes.OperandSize,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.Memory, Role = X64OperandRole.ModRmRm, Size = X64Size.qword },
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.qword }
                ]
            });

            // xorps xmm, xmm/m 128
            Add(new X64Form
            {
                Kind = X64Opcode.xorps,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x57,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.oword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.oword }
                ]
            });

            // xorpd xmm, xmm/m 128
            Add(new X64Form
            {
                Kind = X64Opcode.xorpd,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x57,
                Prefixes = X64Prefixes.OperandSize,
                SizeMode = X64OperandSizeMode.None,

                Operands = [
                    new() { Kind = X64OperandKind.XmmRegister, Role = X64OperandRole.ModRmReg, Size = X64Size.oword },
                    new() { Kind = X64OperandKind.XmmRegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.oword }
                ]
            });

            // seta r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.seta,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x97,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // setae r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.setae,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x93,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // setb r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.setb,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x92,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // setbe r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.setbe,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x96,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // setp r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.setp,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x9A,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });

            // setnp r/m 8
            Add(new X64Form
            {
                Kind = X64Opcode.setnp,
                Map = X64OpcodeMap.Map0F,
                Opcode = 0x9B,
                Extension = 0,
                SizeMode = X64OperandSizeMode.Byte,

                Operands = [
                    new() { Kind = X64OperandKind.RegisterOrMemory, Role = X64OperandRole.ModRmRm, Size = X64Size.@byte }
                ]
            });
        }

        private static void Add(X64Form form)
        {
            if (!_forms.TryGetValue(form.Kind, out var forms))
            {
                forms = [form];
                _forms.Add(form.Kind, forms);
                return;
            }
            forms.Add(form);
        }
        public static List<X64Form> GetForms(X64Opcode kind)
        {
            if (!_forms.TryGetValue(kind, out var forms)) return [];
            return forms;
        }
    }
}
