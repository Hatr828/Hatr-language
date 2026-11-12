using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.back.Instructions
{
    public enum X64Register
    {
        rax, rcx, rdx, rbx, rbs, rsp, rbp, rsi, rdi, ecx, al, cl
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

    public abstract class X64Operand;
    public class RegisterOperand(X64Register register) : X64Operand
    {
        public X64Register Register { get; } = register;
    }
    public class LabelOperand(string label) : X64Operand
    {
        public string Label { get; } = label;
    }
    public class SymbolOperand(string name) : X64Operand
    {
        public string Name { get; } = name;
    }
    public class MemoryOperand(X64Register register, int offset) : X64Operand
    {
        public X64Register Register { get; } = register;
        public int Offset {  get; } = offset;
    }
    public class LongOperand(long value) : X64Operand
    {
        public long Value { get; } = value;
    }

    public class X64Instruction(X64Opcode opcode, X64Operand? left = null, X64Operand? right = null)
    {
        public X64Opcode Opcode { get; } = opcode;
        public X64Operand? Left { get; set; } = left;
        public X64Operand? Right { get; set; } = right;
    }                     
}
