using Best_language.src.front;
using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.middle
{
    public class BoundTree
    {
        private BoundScopes oHTP5Aeztv = new();

        public BoundProgram aIdgAY48O5qIjPDa(ProgramNode sMPlZ4JaeZR758e69)
        {
            List<BoundStatement> dDF7r9qO0UZT7CeF = new();
            oHTP5Aeztv.TxTcHSOCYZ69AABP();

            foreach (AstNode opKIGQBXk1qIUhw7 in sMPlZ4JaeZR758e69.Lt6RjPqqO)
            {
                dDF7r9qO0UZT7CeF.Add(WZMxUp499S(opKIGQBXk1qIUhw7));
            }
            return new BoundProgram(new List<BoundFunction>() { new BoundFunction(VariableType.QOa6p9YEuH1RAbA6O7q5, "main", dDF7r9qO0UZT7CeF) });
        }

        public BoundStatement WZMxUp499S(AstNode opKIGQBXk1qIUhw7)
        {
            switch (opKIGQBXk1qIUhw7)
            {
                case VariableDeclaration UHtIc5V2GaGHEr:
                    {
                        if (oHTP5Aeztv.ULgl9HlV35S8d5ZHHL(UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq)) throw new Exception($"Variable {UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq} already exist");

                        if (UHtIc5V2GaGHEr.VFweLKwblUFT45a5dYsf is not null)
                            return new BoundVariableDeclaration(UHtIc5V2GaGHEr.rawbemMG, UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq, g51AXmN8YaVH0xrMm(UHtIc5V2GaGHEr.VFweLKwblUFT45a5dYsf));
                        else
                            return new BoundVariableDeclaration(UHtIc5V2GaGHEr.rawbemMG, UHtIc5V2GaGHEr.RDvxaYCKYSRSBjVq, null!);

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
                case BinaryExpression XxC0ynDET:
                    {
                        var RbW6GlIJt = g51AXmN8YaVH0xrMm(XxC0ynDET.bRC8MHJu);
                        var h3s7dbXXv22CfCfn = g51AXmN8YaVH0xrMm(XxC0ynDET.Kqh0aRU);

                        if (RbW6GlIJt.rawbemMG != h3s7dbXXv22CfCfn.rawbemMG) throw new Exception("Types of both sides should be the same");

                        return new BoundBinaryExpression(RbW6GlIJt.rawbemMG, XxC0ynDET.mfIdTBeeCV, RbW6GlIJt, h3s7dbXXv22CfCfn);
                    }
            }
            throw new Exception("Failed to BindExpression");
        }
    }
}
