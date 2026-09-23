using Best_language.src.front;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Best_language.src.middle
{
    public class BoundTree
    {
        private BoundScopes oHTP5Aeztv = new();
        private Dictionary<string, int> BrYuQtkDUtc92o = new();
        private int SEgPnknmDDR7;

        public BoundProgram aIdgAY48O5qIjPDa(ProgramNode sMPlZ4JaeZR758e69)
        {
            _V8O8UDwiawCrf9(sMPlZ4JaeZR758e69.Lt6RjPqqO);

            List<BoundStatement> dDF7r9qO0UZT7CeF = new();
            oHTP5Aeztv.TxTcHSOCYZ69AABP();

            foreach (AstNode opKIGQBXk1qIUhw7 in sMPlZ4JaeZR758e69.Lt6RjPqqO)
            {
                dDF7r9qO0UZT7CeF.Add(WZMxUp499S(opKIGQBXk1qIUhw7));
            }
            return new BoundProgram(new List<BoundFunction>() { new BoundFunction(VariableType.QOa6p9YEuH1RAbA6O7q5, "main", dDF7r9qO0UZT7CeF) });
        }
        private void _V8O8UDwiawCrf9(List<AstNode> CZaJj4hwP5v17)
        {
            foreach (AstNode opKIGQBXk1qIUhw7 in CZaJj4hwP5v17)
            {
                switch (opKIGQBXk1qIUhw7)
                {
                    case GotoDeclaration eV5GbDX:
                        {
                            if (BrYuQtkDUtc92o.ContainsKey(eV5GbDX.RDvxaYCKYSRSBjVq)) throw new Exception($"This label already exist: {eV5GbDX.RDvxaYCKYSRSBjVq}");
                            BrYuQtkDUtc92o.Add(eV5GbDX.RDvxaYCKYSRSBjVq, SEgPnknmDDR7++);
                            continue;
                        }
                    case IfDeclaration FMs8v8WFk7p4PXuibRJ:
                        {
                            _V8O8UDwiawCrf9(FMs8v8WFk7p4PXuibRJ._gLmRU6);
                            continue;
                        }
                }
            }
        }

        public BoundStatement WZMxUp499S(AstNode opKIGQBXk1qIUhw7)
        {
            switch (opKIGQBXk1qIUhw7)
            {
                case VariableDeclaration UHtIc5V2GaGHEr:
                    {
                        if (oHTP5Aeztv.ULgl9HlV35S8d5ZHHL(UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq, out _)) throw new Exception($"Variable {UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq} already exist");

                        BoundVariableDeclaration zBgs8nftvUtORz5x7k;
                        
                        if (UHtIc5V2GaGHEr.VFweLKwblUFT45a5dYsf is not null)
                             zBgs8nftvUtORz5x7k = new BoundVariableDeclaration(UHtIc5V2GaGHEr.rawbemMG, UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq, g51AXmN8YaVH0xrMm(UHtIc5V2GaGHEr.VFweLKwblUFT45a5dYsf));
                        else
                             zBgs8nftvUtORz5x7k = new BoundVariableDeclaration(UHtIc5V2GaGHEr.rawbemMG, UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq, null!);
                       
                        if (!oHTP5Aeztv.tCBUXJsr53kXoh1u3N(new(UHtIc5V2GaGHEr.rawbemMG, UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq))) throw new Exception($"Error in TryAddVariable");
                        return zBgs8nftvUtORz5x7k;
                    }
                case IfDeclaration FMs8v8WFk7p4PXuibRJ:
                    {
                        var R5E_HH = new BoundIfDeclaration(g51AXmN8YaVH0xrMm(FMs8v8WFk7p4PXuibRJ._huqzi));
                        oHTP5Aeztv.TxTcHSOCYZ69AABP();
                        foreach (var jn5gszuLQk in FMs8v8WFk7p4PXuibRJ._gLmRU6) R5E_HH.x6w4y6YVMJqhHPCw.Add(WZMxUp499S(jn5gszuLQk));
                        return R5E_HH;
                    }
                case GotoDeclaration eV5GbDX:
                    {
                        if(BrYuQtkDUtc92o.TryGetValue(eV5GbDX.RDvxaYCKYSRSBjVq, out int EHUizE_S0))
                            return new BoundGotoDeclaration(EHUizE_S0);
                        throw new Exception("Some odd Error in BoundTree");
                    }
                case GotoStatement eV5GbDX:
                    {
                        if (BrYuQtkDUtc92o.TryGetValue(eV5GbDX.AM17m62yJaIBgtiOg, out int EHUizE_S0))
                            return new BoundGotoStatement(EHUizE_S0);
                        throw new Exception($"@goto label: {eV5GbDX.AM17m62yJaIBgtiOg} not found");
                    }
                case AssignmentStatement Ya3KuoBYDAJuy5LuF:
                    {
                        if (!oHTP5Aeztv.ULgl9HlV35S8d5ZHHL(Ya3KuoBYDAJuy5LuF.RDvxaYCKYSRSBjVq, out _)) throw new Exception($"Variable {Ya3KuoBYDAJuy5LuF.RDvxaYCKYSRSBjVq} not exist");
                        if(Ya3KuoBYDAJuy5LuF.mfIdTBeeCV is Operations IU2nAxA)
                            return new BoundAssignmentStatement(Ya3KuoBYDAJuy5LuF.RDvxaYCKYSRSBjVq, g51AXmN8YaVH0xrMm(new BinaryExpression(new VariableExpression(Ya3KuoBYDAJuy5LuF.RDvxaYCKYSRSBjVq), IU2nAxA, Ya3KuoBYDAJuy5LuF.Kqh0aRU)));
                        return new BoundAssignmentStatement(Ya3KuoBYDAJuy5LuF.RDvxaYCKYSRSBjVq, g51AXmN8YaVH0xrMm(Ya3KuoBYDAJuy5LuF.Kqh0aRU));
                    }
            }
            throw new Exception("Failed to BindStatement");
        }

        public BoundExpression g51AXmN8YaVH0xrMm(Expression zvo1A5gjZ)
        {
            switch (zvo1A5gjZ)
            {
                case NumberExpression HvSPOv9ne_yrswTLzoo:
                    {                                                           
                        return new BoundNumberExpression(HvSPOv9ne_yrswTLzoo.rawbemMG, HvSPOv9ne_yrswTLzoo.Sb5MrQFjg8JlEcaS);
                    }
                case VariableExpression UHtIc5V2GaGHEr:
                    {
                        if (!oHTP5Aeztv.ULgl9HlV35S8d5ZHHL(UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq, out VariableType FubV605Z9qF5D9z3X)) throw new Exception($"Variable {UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq} not exist");
                        return new BoundVariableExpression(FubV605Z9qF5D9z3X, UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq);
                    }
                case BoolExpression FHZW5YiKBaTQMYfb:
                    {
                        return new BoundBoolExpression(FHZW5YiKBaTQMYfb.Sb5MrQFjg8JlEcaS);
                    }
                case BinaryExpression XxC0ynDET:
                    {
                        var RbW6GlIJt = g51AXmN8YaVH0xrMm(XxC0ynDET.bRC8MHJu);
                        var h3s7dbXXv22CfCfn = g51AXmN8YaVH0xrMm(XxC0ynDET.Kqh0aRU);

                        if (RbW6GlIJt.rawbemMG != h3s7dbXXv22CfCfn.rawbemMG) throw new Exception("Types of both sides should be the same");

                        return new BoundBinaryExpression(RbW6GlIJt.rawbemMG, XxC0ynDET.mfIdTBeeCV, RbW6GlIJt, h3s7dbXXv22CfCfn);
                    }
                case UnaryExpression xi54PsDn5M889REv3:
                    {
                        var h3s7dbXXv22CfCfn = g51AXmN8YaVH0xrMm(xi54PsDn5M889REv3.Kqh0aRU);

                        return new BoundUnaryExpression(h3s7dbXXv22CfCfn.rawbemMG, xi54PsDn5M889REv3.mfIdTBeeCV, h3s7dbXXv22CfCfn);
                    }
            }
            throw new Exception("Failed to BindExpression");
        }
    }
}
