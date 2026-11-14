using Best_language.src.back.Instructions;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.back.Encoders
{
    public partial class X64Encoder
    {
        public (byte[] bytes, List<PeImport> importFixups)  Encode(List<X64Instruction> instructions)
        {
            _code = new();
            _importFixups = new();

            foreach(var instruction in instructions)
            {
                switch(instruction.Opcode)
                {
                    //2 params
                    case X64Opcode.mov:
                        MovHelper(instruction.Left!, instruction.Right!);
                        break;
                    case X64Opcode.sub:
                        SubHelper(instruction.Left!, instruction.Right!);
                        break;
                    case X64Opcode.xor:
                        XorHelper(instruction.Left!, instruction.Right!);
                        break;
                    case X64Opcode.add:
                        AddHelper(instruction.Left!, instruction.Right!);
                        break;

                    //1 param
                    case X64Opcode.push:
                        PushHelper(instruction.Left!);
                        break;
                    case X64Opcode.call:
                        CallHelper(instruction.Left!);
                        break;

                    //0 params
                    case X64Opcode.ret:
                        Ret();
                        break;
                }
            }

            return (_code.ToArray(), _importFixups);
        }
    }
}
