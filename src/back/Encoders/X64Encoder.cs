using Best_language.src.back.Instructions;
using Best_language.src.front;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.back.Encoders
{
    public class X64Encoder
    {
        public List<byte> _code = new();

        public void Mov(X64Register left, long right)
        {
            _code.Add(0x48);
            _code.Add((byte)(0xB8 + GetRegisterCode(left)));

            Span<byte> tmp = stackalloc byte[8];
            BinaryPrimitives.WriteInt64LittleEndian(tmp, right);

            foreach (var t in tmp) _code.Add(t);
        }
        public void Mov(X64Register left, X64Register right)
        {
            byte l = GetRegisterCode(left);
            byte r = GetRegisterCode(right);

            _code.Add(0x48);
            _code.Add(0x8B);
            _code.Add((byte)(0xC0 | (l << 3) | r));
        }
        public void Mov(X64Register left, int offset)
        {
            _code.Add(0x48);
            _code.Add(0x8B);
            WriteStackOperand(GetRegisterCode(left), offset);
        }
        public void Mov(int offset, X64Register right)
        {
            _code.Add(0x48);
            _code.Add(0x89);
            WriteStackOperand(GetRegisterCode(right), offset);
        }

        public void Movzx(X64Register left, X64Register right)
        {
            byte l = GetRegisterCode(left);
            byte r = GetRegisterCode(right);

            _code.Add(0x48);
            _code.Add(0x0F);
            _code.Add(0xB6);
            _code.Add((byte)(0xC0 | (l << 3) | r));
        }

        public void Push(X64Register left)
        {
            _code.Add((byte)(0x50 + GetRegisterCode(left)));
        }

        public void Sub(X64Register left, X64Register right)
        {
            byte l = GetRegisterCode(left);
            byte r = GetRegisterCode(right);

            _code.Add(0x48);
            _code.Add(0x29);
            _code.Add((byte)(0xC0 | (r << 3) | l));
        }
        public void Sub(X64Register left, int right)
        {
            byte l = GetRegisterCode(left);

            _code.Add(0x48);

            if(right >= -128 && right <= 127)
            {
                _code.Add(0x83);
                _code.Add((byte)(0xC0 | (5 << 3) | l));
                _code.Add((byte)right);
            }
            else
            {
                _code.Add(0x81);
                _code.Add((byte)(0xC0 | (5 << 3) | l));

                Span<byte> tmp = stackalloc byte[4];
                BinaryPrimitives.WriteInt64LittleEndian(tmp, right);

                foreach (var t in tmp) _code.Add(t);
            }
        }

        public void Add(X64Register left, X64Register right)
        {
            byte l = GetRegisterCode(left);
            byte r = GetRegisterCode(right);

            _code.Add(0x48);
            _code.Add(0x01);
            _code.Add((byte)(0xC0 | (r << 3) | l));
        }
        public void Add(X64Register left, int right)
        {
            byte l = GetRegisterCode(left);

            _code.Add(0x48);

            if (right >= -128 && right <= 127)
            {
                _code.Add(0x83);
                _code.Add((byte)(0xC0 | l));
                _code.Add((byte)right);
            }
            else
            {
                _code.Add(0x81);
                _code.Add((byte)(0xC0 | l));

                Span<byte> tmp = stackalloc byte[4];
                BinaryPrimitives.WriteInt64LittleEndian(tmp, right);

                foreach (var t in tmp) _code.Add(t);
            }
        }

        public void Xor(X64Register left, X64Register right)
        {
            byte l = GetRegisterCode(left);
            byte r = GetRegisterCode(right);

            _code.Add(0x48);
            _code.Add(0x31);
            _code.Add((byte)(0xC0 | (r << 3) | l));
        }
        public void Xor(X64Register left, int right)
        {
            byte l = GetRegisterCode(left);

            _code.Add(0x48);

            if (right >= -128 && right <= 127)
            {
                _code.Add(0x83);
                _code.Add((byte)(0xC0 | (6 << 3) | l));
                _code.Add((byte)right);
            }
            else
            {
                _code.Add(0x81);
                _code.Add((byte)(0xC0 | (6 << 3) | l));

                Span<byte> tmp = stackalloc byte[4];
                BinaryPrimitives.WriteInt64LittleEndian(tmp, right);

                foreach (var t in tmp) _code.Add(t);
            }
        }

        public void Ret() => _code.Add(0xC3);

        public void WriteStackOperand(int register, int offset)
        {
            if(offset >= -128 && offset <= 127)
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
