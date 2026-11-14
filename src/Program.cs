using Best_language.src.back;
using Best_language.src.back.Encoders;
using Best_language.src.back.Generators;
using Best_language.src.back.Instructions;
using Best_language.src.back.Printers;
using Best_language.src.front;
using Best_language.src.middle;
using Best_language.src.optimizations;
using System.Diagnostics;
using static Best_language.src.middle.BoundScopes;

namespace Best_language.src
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string code = @"
                INT a1 = 0?
                a1 += 1?
                ";
            Lexer lexer = new(code);   
            var tokens = lexer.GetTokens();
            Console.WriteLine(string.Join(",", tokens));
            Console.WriteLine("\n ---- \n");

            Parser parser = new(tokens);
            ProgramNode node = parser.GetAstNodes();
            AstPrinter.Print(node);
            Console.WriteLine("\n ---- \n");

            BoundTree boundTree = new();
            BoundProgram boundProgram = boundTree.BindProgram(node);
            BoundTreePrinter.Print(boundProgram);
            Console.WriteLine("\n ---- \n");

            IrLowering ir = new();
            IrProgram irProgram = ir.Lower(boundProgram);
            IrPrinter.Print(irProgram);
            Console.WriteLine("\n ---- \n");

           // irProgram = Optimizations.ConstantFolding.Start(irProgram);
            IrPrinter.Print(irProgram);
            Console.WriteLine("\n ---- \n");

            X64CodeGenerator codeGen = new(irProgram);
            List<X64Instruction> instructions = codeGen.GenerateInstructions();

            string assembly = X64Printer.GenerateAsm(instructions);
            Console.WriteLine(assembly);

            Console.WriteLine("\n ---- \n");
            BuildAndRun(assembly);
            Console.WriteLine("\n ---- \n");

            var encoder = new X64Encoder();
            var tmp = encoder.Encode(instructions);

            var exe = PeWriter.Build(tmp.bytes, tmp.importFixups);
            File.WriteAllBytes("program_.exe", exe);
            RunProcess("program_.exe", "");
        }

        static void BuildAndRun(string assembly)
        {
            string asmPath = "program.asm";
            string objPath = "program.obj";
            string exePath = "program.exe";

            try
            {
                File.WriteAllText(asmPath, assembly);

                RunProcess("nasm", "-f win64 program.asm -o program.obj");

                RunProcess("x86_64-w64-mingw32-clang", "-nostartfiles program.obj -Wl,-e,start -lkernel32 -o program.exe");

                RunProcess(Path.GetFullPath(exePath), "");
            }
            finally
            {
                if (File.Exists(asmPath))
                    File.Delete(asmPath);

                if (File.Exists(objPath))
                    File.Delete(objPath);

                if (File.Exists(exePath))
                    File.Delete(exePath);
            }
        }

        static void RunProcess(string fileName, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,

                    UseShellExecute = false,

                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine(output);
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                Console.Error.WriteLine(error);
            }

            if (process.ExitCode != 0)
            {
                // Console.WriteLine($"exited with code {process.ExitCode}");
                throw new Exception($"exited with code {process.ExitCode}");
            }
        }
    }
}
