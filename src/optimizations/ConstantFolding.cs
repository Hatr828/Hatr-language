using Best_language.src.front;
using Best_language.src.middle;
using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.optimizations
{
    public static partial class Optimizations
    {
        public static class ConstantFolding
        {
            public static IrProgram Start(IrProgram irProgram)
            {
                Dictionary<IrValue, long> constants = new();
                IrProgram res = new();

                foreach (var instruction in irProgram.Instructions)
                {
                    if (instruction is IrConstant)
                    {
                        IrConstant constant = (IrConstant)instruction;
                        constants.Add(constant.Left, constant.Right);
                    }
                    else if(instruction is IrBinary)
                    {
                        IrBinary binary = (IrBinary)instruction;
                        if(constants.TryGetValue(binary.Left, out long left) && constants.TryGetValue(binary.Right, out long right))
                        {
                            constants.Remove(binary.Left);
                            constants.Add(binary.Left, Combine(left, binary.Op, right)); 
                        }
                    }
                    else if(instruction is IrVariable)
                    {
                        IrVariable variable = (IrVariable)instruction;
                        if (constants.TryGetValue(variable.Right, out long value))
                        {
                            IrVariable v = new(variable.Left, variable.Right);
                            res.Instructions.Add(new IrConstant(variable.Right, value));
                            res.Instructions.Add(v);
                        }
                        else
                        {
                            res.Instructions.Add(variable);
                        }
                    }
                    else
                    {
                        res.Instructions.Add(instruction);
                    }
                }

                return res;
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
