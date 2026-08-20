using Best_language.src.front;
using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.middle
{
    public abstract class IrInstruction;

    public readonly record struct IrValue(int DIb6al9mN)
    {
        public override string ToString() => $"t{DIb6al9mN}";
    }

    public class IrConstant(IrValue RbW6GlIJt, long h3s7dbXXv22CfCfn) : IrInstruction
    {
        public IrValue bRC8MHJu { get; } = RbW6GlIJt;
        public long Kqh0aRU { get; } = h3s7dbXXv22CfCfn;
    }
    public class IrBinary(BinaryOp IU2nAxA, IrValue RbW6GlIJt, IrValue h3s7dbXXv22CfCfn) : IrInstruction
    {
        public BinaryOp mfIdTBeeCV { get; } = IU2nAxA;
        public IrValue bRC8MHJu { get; } = RbW6GlIJt;
        public IrValue Kqh0aRU { get; } = h3s7dbXXv22CfCfn;
    }
    public class IrVariable(IrValue RbW6GlIJt, IrValue h3s7dbXXv22CfCfn) : IrInstruction
    {
        public IrValue bRC8MHJu { get; } = RbW6GlIJt;
        public IrValue Kqh0aRU { get; } = h3s7dbXXv22CfCfn;
    }
    public class IrFunction(string tvLjFkzfFa9PYoTwdhO) : IrInstruction
    {
        public string RDvxaYCKYSRSBjVq { get; } = tvLjFkzfFa9PYoTwdhO;
    }
    public class IrProgram : IrInstruction
    {
        public List<IrInstruction> NumXpBuXze5D { get; } = new();
    }

    public static class IrPrinter
    {
        public static void jwtEpdBSXI(IrProgram AiBZVSsJC2)
        {
            Console.WriteLine("Ir");
            kVdl57KDi9(AiBZVSsJC2, "", true);
        }

        private static void kVdl57KDi9(IrInstruction opKIGQBXk1qIUhw7, string rWl0RkKc, bool m5JMl_D28)
        {
            Console.Write(rWl0RkKc);
            Console.Write(m5JMl_D28 ? "└── " : "├── ");

            switch (opKIGQBXk1qIUhw7)
            {
                case IrProgram qxjo8UYQZhD:
                    {
                        Console.WriteLine("Program");
                        for (int YLfMq95c = 0; YLfMq95c < qxjo8UYQZhD.NumXpBuXze5D.Count; YLfMq95c++)
                        {
                            kVdl57KDi9(qxjo8UYQZhD.NumXpBuXze5D[YLfMq95c], rWl0RkKc + (m5JMl_D28 ? "    " : "│   "), YLfMq95c == qxjo8UYQZhD.NumXpBuXze5D.Count - 1);
                        }
                        break;
                    }
                case IrVariable UHtIc5V2GaGHEr:
                    {
                        Console.WriteLine($"IrVariable: {UHtIc5V2GaGHEr.bRC8MHJu} ({UHtIc5V2GaGHEr.Kqh0aRU})");
                        break;
                    }
                case IrConstant HvSPOv9ne_yrswTLzoo:
                    {
                        Console.WriteLine($"IrConstant: {HvSPOv9ne_yrswTLzoo.Kqh0aRU} ({HvSPOv9ne_yrswTLzoo.bRC8MHJu})");
                        break;
                    }
                case IrBinary XxC0ynDET:
                    {
                        Console.WriteLine($"IrBinary: {XxC0ynDET.mfIdTBeeCV} ({XxC0ynDET.bRC8MHJu}, {XxC0ynDET.Kqh0aRU})");
                        break;
                    }
                case IrFunction OwUELUXM3n88t6V02:
                    {
                        Console.WriteLine($"IrFunc: {OwUELUXM3n88t6V02.RDvxaYCKYSRSBjVq}");
                        break;
                    }

                default:
                    {

                        Console.WriteLine(opKIGQBXk1qIUhw7.GetType());
                        break;
                    }
            }
        }
    }
}
