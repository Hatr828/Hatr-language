using Best_language.src.front;
using System.Drawing;

namespace Best_language.src.middle
{
    public abstract class IrInstruction;

    public record struct IrValue(int Id, TypeSymbol Type)
    {
        public override string ToString() => $"t{Id}";
    }
    public class IrLabel(string name) : IrInstruction
    {
        public string Name { get; } = name;
        public override string ToString() => $"{nameof(IrLabel)}: {Name}";
    }

    public class IrConstant(IrValue left, ulong right, TypeSymbol type) : IrInstruction
    {
        public IrValue Left { get; } = left;
        public ulong Right { get; } = right;
        public TypeSymbol TypeRight { get; } = type;

        public override string ToString() => $"{nameof(IrConstant)}: {Left}, {Right}({TypeRight})";
    }
    public class IrArrayConstant(IrValue left, List<IrValue> right, TypeSymbol type) : IrInstruction
    {
        public IrValue Left { get; } = left;
        public List<IrValue> Right { get; } = right;
        public TypeSymbol TypeRight { get; } = type;

        public override string ToString() => $"{nameof(IrArrayConstant)}: {Left}, ({string.Join(", ", Right)})({TypeRight})";
    }
    public class IrFloatConstant(IrValue left, double right, TypeSymbol type) : IrInstruction
    {
        public IrValue Left { get; } = left;
        public double Right { get; } = right;
        public TypeSymbol TypeRight { get; } = type;

        public override string ToString() => $"{nameof(IrFloatConstant)}: {Left}, {Right}({TypeRight})";
    }
    public class IrJumpIfTrue(IrValue condition, string target) : IrInstruction
    {
        public string Target { get; } = target;
        public IrValue Condition { get; } = condition;

        public override string ToString() => $"{nameof(IrJumpIfTrue)}: {Target} ({Condition})";
    }
    public class IrJumpIfNotTrue(IrValue condition, string target) : IrInstruction
    {
        public string Target { get; } = target;
        public IrValue Condition { get; } = condition;

        public override string ToString() => $"{nameof(IrJumpIfNotTrue)}: {Target} ({Condition})";
    }
    public class IrJump(string target) : IrInstruction
    {
        public string Target { get; } = target;

        public override string ToString() => $"{nameof(IrJump)}: {Target}";
    }
    public class IrBinary(Operation op, IrValue left, IrValue right, IrValue result) : IrInstruction
    {
        public Operation Op { get; } = op;
        public IrValue Left { get; } = left;
        public IrValue Right { get; } = right;

        public IrValue Result { get; } = result;

        public override string ToString() => $"{nameof(IrBinary)}: {Result}, {Left} {Op} {Right}";
    }
    public class IrConversion(IrValue left, IrValue right) : IrInstruction
    {
        public IrValue Left { get; } = left;
        public IrValue Right { get; } = right;

        public override string ToString() => $"{nameof(IrConversion)}: {Left} ({Left.Type}), {Right} ({Right.Type})";
    }
    public class IrUnary(Operation op, IrValue right, IrValue result) : IrInstruction
    {
        public Operation Op { get; } = op;
        public IrValue Right { get; } = right;

        public IrValue Result { get; } = result;

        public override string ToString() => $"{nameof(IrUnary)}: {Result}, {Right}";
    }
    public class IrVariable(IrValue left, IrValue right) : IrInstruction
    {
        public IrValue Left { get; } = left;
        public IrValue Right { get; } = right;

        public override string ToString() => $"{nameof(IrVariable)}: {Left}, {Right}";
    }
    public class IrArray(IrValue left, int size, List<IrValue> right) : IrInstruction
    {
        public IrValue Left { get; } = left;
        public int Size { get; } = size;
        public List<IrValue> Right { get; } = right;

        public override string ToString() => $"{nameof(IrArray)}: {Left}, ({string.Join(", ", Right)})";
    }
    public class IrLoad(IrValue left, IrValue right, int offset) : IrInstruction
    {
        public IrValue Left { get; } = left;
        public IrValue Right { get; } = right;
        public int Offset { get; } = offset;

        public override string ToString() => $"{nameof(IrLoad)}: {Left}, {Right} (offset: {Offset})";

    }
    public class IrStore(IrValue left, int offset, IrValue right) : IrInstruction
    {
        public IrValue Left { get; } = left;
        public int Offset { get; } = offset;
        public IrValue Right { get; } = right;

        public override string ToString() => $"{nameof(IrStore)}: {Left} (offset: {Offset}), {Right}";

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
            
            foreach(var node in root.Instructions)
            {
                Console.WriteLine("    " + node);
            }
        }
    }
}
