using Best_language.src.front;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Best_language.src.middle
{
    public class IrLowering
    {
        private int _nextId;
        private int _nextLebelId;
        private IrProgram _currentProgram = new();

        private Dictionary<string, IrValue> _variables = new();

        private IrValue NewIrValue() => new(_nextId++);
        private string NewLabelName(string name) => $"{name}_{_nextLebelId++}";

        public IrProgram Lower(BoundProgram mainNode) 
        {
            foreach (BoundFunction function in mainNode.Functions)
            {
                
                foreach (BoundStatement st in function.Statements)
                {
                    LowerNode(st);
                }

                return _currentProgram;
            }

            throw new Exception("BoundProgram is empty");
        }

        public void LowerNode(BoundStatement node)
        {
            switch(node)
            {
                case BoundVariableDeclaration variable:
                    {
                        if (variable.Initializer is null) return;

                        var value = LowerExpression(variable.Initializer);
                        var res = EmitCopy(value);
                        _variables.Add(variable.Name, res);
                        return;
                    }
                case BoundIfDeclaration If:
                    {
                        IrValue condition = LowerExpression(If.Expression);

                        List<string> labels = new();
                        for(int i = 0; i < If.ElseIfs?.Count; i++) labels.Add(NewLabelName("elseIf"));
                        string ifLabel = NewLabelName("if");
                        string endLabel = NewLabelName("end");
                        string elseLabel = NewLabelName("else");

                        Emit(new IrJumpIfTrue(condition, ifLabel));

                        for (int i = 0; i < If.ElseIfs?.Count; i++) Emit(new IrJumpIfTrue(LowerExpression(If.ElseIfs[i].Expression), labels[i]));
                        if(If.Else is not null) Emit(new IrJump(elseLabel));
                        Emit(new IrJump(endLabel)); //end label if there is no else and everything before is false. Redundant if there is else, possible size reduction for asm and therefore for .exe.

                        //if part
                        Emit(new IrLabel(ifLabel));
                        foreach (var statement in If.Statements)
                            LowerNode(statement);
                        Emit(new IrJump(endLabel));

                        // elseIfs parts
                        for (int i = 0; i < If.ElseIfs?.Count; i++)
                        {
                            Emit(new IrLabel(labels[i]));
                            foreach (var statement in If.ElseIfs[i].Statements)
                                LowerNode(statement);
                            Emit(new IrJump(endLabel));
                        }
                        // else part
                        if (If.Else is not null)
                        {
                            Emit(new IrLabel(elseLabel));
                            foreach (var statement in If.Else.Statements)
                                LowerNode(statement);
                        }
                        // end label
                        Emit(new IrLabel(endLabel));
                        return;
                    }
                case BoundGotoDeclaration @goto:
                        Emit(new IrLabel("user_" + @goto.Id)); return;
                case BoundGotoStatement @goto:
                        Emit(new IrJump("user_" + @goto.ToId)); return;
                case BoundAssignmentStatement assignment:
                    {
                        var res = ResolveVariable(assignment.Name);
                        var source = LowerExpression(assignment.Right);

                        Emit(new IrVariable(res, source));
                        return;
                    }
            }
            throw new Exception("Cannot lower: " + node.GetType());
        }

        public IrValue LowerExpression(BoundExpression expression)
        {
            switch(expression)
            {
                case BoundNumberExpression num:
                    return EmitConstant(num.Value);
                case BoundBoolExpression ex:
                    return EmitConstant(ex.Value ? 1 : 0);
                case BoundVariableExpression variable:
                    return ResolveVariable(variable.Name);
                case BoundBinaryExpression binary:
                    {
                        var left = LowerExpression(binary.Left);
                        var res = EmitCopy(left);
                        var right = LowerExpression(binary.Right);

                        Emit(new IrBinary(binary.Op, res, right));

                        return res;
                    }
                case BoundUnaryExpression unary:
                    {
                        var right = LowerExpression(unary.Right);

                        if (unary.Op is Operations.PlusPlusLeft or Operations.MinusMinusLeft)
                        {
                            Emit(new IrUnary(unary.Op, right));
                            return right;
                        }

                        if (unary.Op is Operations.PlusPlusRight or Operations.MinusMinusRight)
                        {
                            var res = EmitCopy(right);
                            Emit(new IrUnary(unary.Op, right));
                            return res;
                        }

                        var res2 = EmitCopy(right);
                        Emit(new IrUnary(unary.Op, res2));
                        return res2;
                    }
            }
            throw new Exception("Cannot lower: " + expression.GetType());
        }
        private IrValue EmitConstant(long value)
        {
            var result = NewIrValue();
            Emit(new IrConstant(result, value));
            return result;
        }
        private IrValue EmitCopy(IrValue source)
        {
            var result = NewIrValue();
            Emit(new IrVariable(result, source));
            return result;
        }

        private void Emit(IrInstruction instruction)
        {
            _currentProgram.Instructions.Add(instruction);
        }
        private IrValue ResolveVariable(string name)
        {
            if (_variables.TryGetValue(name, out var value))
                return value;

            throw new InvalidOperationException($"Unknown variable: '{name}'.");
        }
    }
}
