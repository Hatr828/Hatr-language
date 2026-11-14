using Best_language.src.back.Instructions;
using Best_language.src.front;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Best_language.src.back.Encoders
{
    public partial class X64Encoder
    {
        private List<byte> _code = new();
        private List<PeImport> _importFixups = new();
        private readonly Dictionary<string, string> _dlls = new()
        {
            ["ExitProcess"] = "kernel32.dll",
        };

        public void Mov(X64RegisterOperand left, X64LongOperand right)
        {
            _code.Add(0x48);
            _code.Add((byte)(0xB8 + GetRegisterCode(left.Register)));

            Span<byte> tmp = stackalloc byte[8];
            BinaryPrimitives.WriteInt64LittleEndian(tmp, right.Value);

            foreach (var t in tmp) _code.Add(t);
        }
        public void Mov(X64RegisterOperand left, X64IntOperand right)
        {
            _code.Add(0x48);
            _code.Add((byte)(0xB8 + GetRegisterCode(left.Register)));

            Span<byte> tmp = stackalloc byte[8];
            BinaryPrimitives.WriteInt64LittleEndian(tmp, right.Value);

            foreach (var t in tmp) _code.Add(t);
        }
        public void Mov(X64RegisterOperand left, X64RegisterOperand right)
        {
            byte l = GetRegisterCode(left.Register);
            byte r = GetRegisterCode(right.Register);

            _code.Add(0x48);
            _code.Add(0x8B);
            _code.Add((byte)(0xC0 | (l << 3) | r));
        }
        public void Mov(X64RegisterOperand left, X64MemoryOperand mem)
        {
            _code.Add(0x48);
            _code.Add(0x8B);
            WriteStackOperand(GetRegisterCode(left.Register), -mem.Offset);
        }
        public void Mov(X64MemoryOperand mem, X64RegisterOperand right)
        {
            _code.Add(0x48);
            _code.Add(0x89);
            WriteStackOperand(GetRegisterCode(right.Register), -mem.Offset);
        }

        public void Movzx(X64RegisterOperand left, X64RegisterOperand right)
        {
            byte l = GetRegisterCode(left.Register);
            byte r = GetRegisterCode(right.Register);

            _code.Add(0x48);
            _code.Add(0x0F);
            _code.Add(0xB6);
            _code.Add((byte)(0xC0 | (l << 3) | r));
        }

        public void Push(X64RegisterOperand left)
        {
            _code.Add((byte)(0x50 + GetRegisterCode(left.Register)));
        }

        public void Sub(X64RegisterOperand left, X64RegisterOperand right)
        {
            byte l = GetRegisterCode(left.Register);
            byte r = GetRegisterCode(right.Register);

            _code.Add(0x48);
            _code.Add(0x29);
            _code.Add((byte)(0xC0 | (r << 3) | l));
        }
        public void Sub(X64RegisterOperand left, X64IntOperand right)
        {
            byte l = GetRegisterCode(left.Register);

            _code.Add(0x48);

            if(right.Value >= -128 && right.Value <= 127)
            {
                _code.Add(0x83);
                _code.Add((byte)(0xC0 | (5 << 3) | l));
                _code.Add((byte)right.Value);
            }
            else
            {
                _code.Add(0x81);
                _code.Add((byte)(0xC0 | (5 << 3) | l));

                Span<byte> tmp = stackalloc byte[4];
                BinaryPrimitives.WriteInt64LittleEndian(tmp, right.Value);

                foreach (var t in tmp) _code.Add(t);
            }
        }

        public void Add(X64RegisterOperand left, X64RegisterOperand right)
        {
            byte l = GetRegisterCode(left.Register);
            byte r = GetRegisterCode(right.Register);

            _code.Add(0x48);
            _code.Add(0x01);
            _code.Add((byte)(0xC0 | (r << 3) | l));
        }
        public void Add(X64RegisterOperand left, X64IntOperand right)
        {
            byte l = GetRegisterCode(left.Register);

            _code.Add(0x48);

            if (right.Value >= -128 && right.Value <= 127)
            {
                _code.Add(0x83);
                _code.Add((byte)(0xC0 | l));
                _code.Add((byte)right.Value);
            }
            else
            {
                _code.Add(0x81);
                _code.Add((byte)(0xC0 | l));

                Span<byte> tmp = stackalloc byte[4];
                BinaryPrimitives.WriteInt64LittleEndian(tmp, right.Value);

                foreach (var t in tmp) _code.Add(t);
            }
        }

        public void Xor(X64RegisterOperand left, X64RegisterOperand right)
        {
            byte l = GetRegisterCode(left.Register);
            byte r = GetRegisterCode(right.Register);

            _code.Add(0x48);
            _code.Add(0x31);
            _code.Add((byte)(0xC0 | (r << 3) | l));
        }
        public void Xor(X64RegisterOperand left, X64IntOperand right)
        {
            byte l = GetRegisterCode(left.Register);

            _code.Add(0x48);

            if (right.Value >= -128 && right.Value <= 127)
            {
                _code.Add(0x83);
                _code.Add((byte)(0xC0 | (6 << 3) | l));
                _code.Add((byte)right.Value);
            }
            else
            {
                _code.Add(0x81);
                _code.Add((byte)(0xC0 | (6 << 3) | l));

                Span<byte> tmp = stackalloc byte[4];
                BinaryPrimitives.WriteInt64LittleEndian(tmp, right.Value);

                foreach (var t in tmp) _code.Add(t);
            }
        }

        public void Call(string dll, string name)
        {
            _code.Add(0xFF);
            _code.Add(0x15);

            int offset = _code.Count;

            _importFixups.Add(new PeImport
            {
                Offset = _code.Count,
                DLL = dll,
                Name = name
            });

            CollectionsMarshal.SetCount(_code, checked(offset + 4));
            CollectionsMarshal.AsSpan(_code).Slice(offset, 4).Clear();
        }
        public void Call(X64SymbolOperand op)
        {
            if(!_dlls.TryGetValue(op.Name, out var dll))
            {
                throw new ArgumentException("Unknown dll");
            }

            Call(dll, op.Name);
        }

        public void Ret() => _code.Add(0xC3);
    }
    // Helpers
    public partial class X64Encoder
    {
        public void MovHelper(X64Operand left, X64Operand right)
        {
            switch (left, right)
            {
                case (X64RegisterOperand l, X64RegisterOperand r):
                    Mov(l, r);
                    return;
                case (X64RegisterOperand l, X64LongOperand r):
                    Mov(l, r);
                    return;
                case (X64RegisterOperand l, X64MemoryOperand r):
                    Mov(l, r);
                    return;
                case (X64RegisterOperand l, X64IntOperand r):
                    Mov(l, r);
                    return;
                case (X64MemoryOperand l, X64RegisterOperand r):
                    Mov(l, r);
                    return;
            }

            throw new ArgumentException("Unknown encoding for mov");
        }
        public void MovzxHelper(X64Operand left, X64Operand right)
        {
            switch (right, left)
            {
                case (X64RegisterOperand l, X64RegisterOperand r):
                    Movzx(l, r);
                    return;
            }

            throw new ArgumentException("Unknown encoding for movzx");
        }
        public void AddHelper(X64Operand left, X64Operand right)
        {
            switch (left, right)
            {
                case (X64RegisterOperand l, X64RegisterOperand r):
                    Add(l, r);
                    return;
                case (X64RegisterOperand l, X64IntOperand r):
                    Add(l, r);
                    return;
            }

            throw new ArgumentException("Unknown encoding for add");
        }
        public void SubHelper(X64Operand left, X64Operand right)
        {
            switch (left, right)
            {
                case (X64RegisterOperand l, X64RegisterOperand r):
                    Sub(l, r);
                    return;
                case (X64RegisterOperand l, X64IntOperand r):
                    Sub(l, r);
                    return;
            }

            throw new ArgumentException("Unknown encoding for sub");
        }
        public void XorHelper(X64Operand left, X64Operand right)
        {
            switch(left, right)
            {
                case (X64RegisterOperand l, X64RegisterOperand r):
                    Xor(l, r);
                    return;
                case (X64RegisterOperand l, X64IntOperand r):
                    Xor(l, r);
                    return;
            }

            throw new ArgumentException("Unknown encoding for xor");
        }
        public void PushHelper(X64Operand left)
        {
            switch (left)
            {
                case (X64RegisterOperand l):
                    Push(l);
                    return;
            }

            throw new ArgumentException("Unknown encoding for push");
        }
        public void CallHelper(X64Operand left)
        {
            switch (left)
            {
                case (X64SymbolOperand l):
                    Call(l);
                    return;
            }

            throw new ArgumentException("Unknown encoding for push");
        }
        public void WriteStackOperand(int register, int offset)
        {
            if (offset >= -128 && offset <= 127)
            {
                _code.Add((byte)(0x40 | (register << 3) | 5));
                _code.Add((byte)offset);
            }
            else
            {
                _code.Add((byte)(0x80 | (register << 3) | 5));

                Span<byte> tmp = stackalloc byte[4];
                BinaryPrimitives.WriteInt32LittleEndian(tmp, offset);
                foreach (var t in tmp) _code.Add(t);
            }
        }

        private byte GetRegisterCode(X64Register reg) => reg switch
        {
            X64Register.rax or X64Register.al => 0,
            X64Register.rcx or X64Register.ecx or X64Register.cl => 1,
            X64Register.rdx => 2,
            X64Register.rbx => 3,
            X64Register.rsp => 4,
            X64Register.rbp => 5,
            X64Register.rsi => 6,
            X64Register.rdi => 7,

            _ => throw new ArgumentException($"Unknow register: {reg}")
        };
    }
}
