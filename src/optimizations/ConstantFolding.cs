using Best_language.src.front;
using Best_language.src.middle;

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

            private static long Combine(long left, Operation op, long right) => op switch
            {
                Operation.Plus => left + right,
                Operation.Minus => left - right,
                Operation.Multiply => left * right,
                Operation.Divide => left / right,

                Operation.LessThan => left < right ? 1 : 0,
                Operation.LessThanOrEqual => left <= right ? 1 : 0,
                Operation.GreaterThan => left > right ? 1 : 0,
                Operation.GreaterThanOrEqual => left >= right ? 1 : 0,
                Operation.EqualsEquals => left == right ? 1 : 0,

                Operation.And => left == 1 && right == 1 ? 1 : 0,
                Operation.Or => left == 1 || right == 1 ? 1 : 0,
            };
        }
    }
}
