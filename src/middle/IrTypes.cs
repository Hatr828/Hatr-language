using Best_language.src.front;
using System.Numerics;

namespace Best_language.src.middle
{
    public abstract class IrInstruction;

    public readonly record struct IrValue(int Id, TypeSymbol Type)
    {
        public override string ToString() => $"t{Id}";
    }
    public class IrLabel(string name) : IrInstruction
    {
        public string Name { get; } = name;
    }

    public class IrConstant(IrValue left, ulong right, TypeSymbol type) : IrInstruction
    {
        public IrValue Left { get; } = left;
        public ulong Right { get; } = right;
        public TypeSymbol TypeRight { get; } = type;
    }
    public class IrJumpIfTrue(IrValue condition, string target) : IrInstruction
    {
        public string Target { get; } = target;
        public IrValue Condition { get; } = condition;
    }
    public class IrJumpIfNotTrue(IrValue condition, string target) : IrInstruction
    {
        public string Target { get; } = target;
        public IrValue Condition { get; } = condition;
    }
    public class IrJump(string target) : IrInstruction
    {
        public string Target { get; } = target;
    }
    public class IrBinary(Operations op, IrValue left, IrValue right) : IrInstruction
    {
        public Operations Op { get; } = op;
        public IrValue Left { get; } = left;
        public IrValue Right { get; } = right;
    }
    public class IrConversion(IrValue left, IrValue right) : IrInstruction
    {
        public IrValue Left { get; } = left;
        public IrValue Right { get; } = right;
    }
    public class IrUnary(Operations op, IrValue right) : IrInstruction
    {
        public Operations Op { get; } = op;
        public IrValue Right { get; } = right;
    }
    public class IrVariable(IrValue left, IrValue right) : IrInstruction
    {
        public IrValue Left { get; } = left;
        public IrValue Right { get; } = right;
    }
    public class IrProgram : IrInstruction
    {
        public List<IrInstruction> Instructions { get; } = new();
    }

    public static class IrPrinter
    {
        public static void Print(IrProgram root)
        {
            Console.WriteLine("Ir");
            PrintIr(root, "", true);
        }

        private static void PrintIr(IrInstruction node, string indent, bool last)
        {
            Console.Write(indent);
            Console.Write(last ? "└── " : "├── ");

            switch (node)
            {
                case IrProgram main:
                    {
                        Console.WriteLine("Program");
                        for (int i = 0; i < main.Instructions.Count; i++)
                        {
                            PrintIr(main.Instructions[i], indent + (last ? "    " : "│   "), i == main.Instructions.Count - 1);
                        }
                        break;
                    }
                case IrVariable variable:
                    {
                        Console.WriteLine($"IrVariable: {variable.Left} ({variable.Right})");
                        break;
                    }
                case IrLabel label:
                    {
                        Console.WriteLine(label.Name + ":");
                        break;
                    }
                case IrConstant number:
                    {
                        Console.WriteLine($"IrConstant: {number.Left} ({number.Right})");
                        break;
                    }
                case IrConversion conversion:
                    {
                        Console.WriteLine($"IrConversion: {conversion.Left} ({conversion.Right})");
                        break;
                    }
                case IrJumpIfTrue jump:
                    {
                        Console.WriteLine($"IrJumpIfTrue: {jump.Condition} {jump.Target}");
                        break;
                    }
                case IrJumpIfNotTrue jump:
                    {
                        Console.WriteLine($"IrJumpIfNotTrue: {jump.Condition} {jump.Target}");
                        break;
                    }
                case IrJump jump:
                    {
                        Console.WriteLine($"IrJump: {jump.Target}");
                        break;
                    }
                case IrBinary binary:
                    {
                        Console.WriteLine($"IrBinary: {binary.Op} ({binary.Left}, {binary.Right})");
                        break;
                    }
                case IrUnary unary:
                    {
                        Console.WriteLine($"IrUnary: {unary.Op} ({unary.Right})");
                        break;
                    }
                default:
                    {
                        Console.WriteLine(node.GetType());
                        break;
                    }
            }
        }
    }
}
