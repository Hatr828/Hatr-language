using Best_language.src.front;
using Best_language.src.middle;
using System.Numerics;

namespace Best_language.src.optimizations
{
    public static partial class Optimizations
    {
        public static class ConstantFolding
        {
            public static IrProgram Start(IrProgram irProgram)
            {      
                return null!;
            }

            private static long Combine(long left, Operations op, long right) => op switch
            {
                Operations.Plus => left + right,
                Operations.Minus => left - right,
                Operations.Multiply => left * right,
                Operations.Divide => left / right,

                Operations.LessThan => left < right ? 1 : 0,
                Operations.LessThanOrEqual => left <= right ? 1 : 0,
                Operations.GreaterThan => left > right ? 1 : 0,
                Operations.GreaterThanOrEqual => left >= right ? 1 : 0,
                Operations.EqualsEquals => left == right ? 1 : 0,

                Operations.And => left == 1 && right == 1 ? 1 : 0,
                Operations.Or => left == 1 || right == 1 ? 1 : 0,
            };
        }
    }
}
