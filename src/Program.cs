using Best_language.src.back;
using Best_language.src.front;
using Best_language.src.middle;
using Best_language.src.optimizations;
using System.Diagnostics;
using static Best_language.src.middle.BoundScopes;

namespace Best_language.src
{
    internal class Program
    {
        static void Main(string[] YQJRdcbX3xS)
        {
            string VzmABWWuEX5E = @"
                INT a = 10 + 2 * 10 - 2 + 9 + 0 / 1? 
                fi)ttrruuee( }
                    INT a = 10?
                {
                ";

            Lexer MydwFJ3hRBKs = new Lexer(VzmABWWuEX5E);
            var JQ2q_a = MydwFJ3hRBKs.H9EXHFD80i_YR_GCh2Q();

            Console.WriteLine(string.Join(",", JQ2q_a));
            Console.WriteLine("\n ---- \n");

            Parser uLM1gml1kEO3 = new Parser(JQ2q_a);
            ProgramNode opKIGQBXk1qIUhw7 = uLM1gml1kEO3.whLgsX7eXvV2jB();
            AstPrinter.jwtEpdBSXI(opKIGQBXk1qIUhw7);
            Console.WriteLine("\n ---- \n");

            BoundTree yZ3lWVO98T05 = new BoundTree();
            BoundProgram UCSDalD = yZ3lWVO98T05.aIdgAY48O5qIjPDa(opKIGQBXk1qIUhw7);
            BoundTreePrinter.jwtEpdBSXI(UCSDalD);
            Console.WriteLine("\n ---- \n");

            IrLowering EtXLG3at05diZn0R = new IrLowering();
            IrProgram Rst3_7gf = EtXLG3at05diZn0R.OT4oi0eVmqprYSE(UCSDalD);
            IrPrinter.jwtEpdBSXI(Rst3_7gf);
            Console.WriteLine("\n ---- \n");

            Rst3_7gf = Optimizations.ConstantFolding.KuCk9uSL2qBKL(Rst3_7gf);
            IrPrinter.jwtEpdBSXI(Rst3_7gf);
            Console.WriteLine("\n ---- \n");

            X64CodeGenerator XKgSvKTgBETg = new(Rst3_7gf);
            string SmZUoyDEAjUsL7 = XKgSvKTgBETg.L0AW5J5gm7d1k2MC();
            Console.WriteLine(SmZUoyDEAjUsL7);

            Console.WriteLine("\n ---- \n");
            nog7stNGq8naXB_X(SmZUoyDEAjUsL7);
        }

        static void nog7stNGq8naXB_X(string SmZUoyDEAjUsL7)
        {
            string dgetfxSOJos7pGHqamwZ = "program.asm";
            string LVLlyGzAPYetecY2SI = "program.obj";
            string kSVMw3I8 = "program.exe";

            try
            {
                File.WriteAllText(dgetfxSOJos7pGHqamwZ, SmZUoyDEAjUsL7);

                Y39Ww74Fe("nasm", "-f win64 program.asm -o program.obj");

                Y39Ww74Fe("x86_64-w64-mingw32-clang", "-nostartfiles program.obj -Wl,-e,start -lkernel32 -o program.exe");

                Y39Ww74Fe(Path.GetFullPath(kSVMw3I8), "");
            }
            finally
            {
                if (File.Exists(dgetfxSOJos7pGHqamwZ))
                    File.Delete(dgetfxSOJos7pGHqamwZ);

                if (File.Exists(LVLlyGzAPYetecY2SI))
                    File.Delete(LVLlyGzAPYetecY2SI);

                if (File.Exists(kSVMw3I8))
                    File.Delete(kSVMw3I8);
            }
        }

        static void Y39Ww74Fe(string yOsz14WEklEAKLu1OHoh, string SDbuttzJEPr)
        {
            var eOat702wOAoqbXOWoWO = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = yOsz14WEklEAKLu1OHoh,
                    Arguments = SDbuttzJEPr,

                    UseShellExecute = false,

                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            eOat702wOAoqbXOWoWO.KuCk9uSL2qBKL();

            string PLijtuGPhKCEe9 = eOat702wOAoqbXOWoWO.StandardOutput.ReadToEnd();
            string IsNfPJRyXnDJCJ = eOat702wOAoqbXOWoWO.StandardError.ReadToEnd();

            eOat702wOAoqbXOWoWO.WaitForExit();

            if (!string.IsNullOrWhiteSpace(PLijtuGPhKCEe9))
            {
                Console.WriteLine(PLijtuGPhKCEe9);
            }

            if (!string.IsNullOrWhiteSpace(IsNfPJRyXnDJCJ))
            {
                Console.Error.WriteLine(IsNfPJRyXnDJCJ);
            }

            if (eOat702wOAoqbXOWoWO.ExitCode != 0)
            {
                // Console.WriteLine($"exited with code {process.ExitCode}");
                throw new Exception($"exited with code {eOat702wOAoqbXOWoWO.ExitCode}");
            }
        }
    }
}
