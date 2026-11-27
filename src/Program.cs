using Best_language.src.back;
using Best_language.src.back.Encoders;
using Best_language.src.back.Generators;
using Best_language.src.back.Instructions;
using Best_language.src.back.Printers;
using Best_language.src.front;
using Best_language.src.middle;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;

namespace Best_language.src
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string code;

            if (args.Length <= 0)
            {
#if !DEBUG
                throw new ArgumentException("Expected args, at least path to code");
#else
                code = @"
int
main()
{
        int arr[2];

        arr[0] = 1;
        arr[1] = 2;

        return arr[0] + arr[1] - 3;
}

                ";
#endif
            }
            else
            {
                code = GetCodeFromFile(args[0]);
            }

            Console.WriteLine(code);
            Console.WriteLine("\n ---- \n");

            var sw = Stopwatch.StartNew();

            Lexer lexer = new(code);
            var tokens = lexer.GetTokens();

            sw.Stop();
            Console.WriteLine($"Lexer done in: {sw.ElapsedTicks} ticks");
            Console.WriteLine("\n ---- \n");

            Console.WriteLine(string.Join(",", tokens));
            Console.WriteLine("\n ---- \n");

            sw.Restart();

            Parser parser = new();
            var node = parser.GetAstNodes(tokens);
            sw.Stop();
            Console.WriteLine($"Parser done in: {sw.ElapsedTicks} ticks");
            Console.WriteLine("\n ---- \n");
            AstPrinter.Print(node);
            Console.WriteLine("\n ---- \n");

            sw.Restart();

            BoundTree boundTree = new();
            BoundProgram boundProgram = boundTree.BindProgram(node, Parser.typeTable);

            sw.Stop();
            Console.WriteLine($"BoundTree done in: {sw.ElapsedTicks} ticks");
            Console.WriteLine("\n ---- \n");

            BoundTreePrinter.Print(boundProgram);
            Console.WriteLine("\n ---- \n");

            sw.Restart();

            IrLowering ir = new();
            IrProgram irProgram = ir.Lower(boundProgram, Parser.typeTable);

            sw.Stop();
            Console.WriteLine($"IrLowering done in: {sw.ElapsedTicks} ticks");
            Console.WriteLine("\n ---- \n");

            // irProgram = Optimizations.ConstantFolding.Start(irProgram);
            IrPrinter.Print(irProgram);
            Console.WriteLine("\n ---- \n");

            sw.Restart();

            X64CodeGenerator codeGen = new(irProgram);
            List<X64Instruction> instructions = codeGen.GenerateInstructions();

            sw.Stop();
            Console.WriteLine($"X64CodeGenerator done in: {sw.ElapsedTicks} ticks");
            Console.WriteLine("\n ---- \n");

            string assembly = X64Printer.GenerateAsm(instructions);
            Console.WriteLine(assembly);

            BuildAndRun(assembly);
            Console.WriteLine("\n ---- \n");

            sw.Restart();

            var encoder = new X64Encoder();
            var tmp = encoder.Encode(instructions);

            sw.Stop();
            Console.WriteLine($"X64Encoder done in: {sw.ElapsedTicks} ticks");
            Console.WriteLine("\n ---- \n");

            var exe = PeWriter.Build(tmp.bytes, tmp.importFixups);
            File.WriteAllBytes("program_.exe", exe);
            RunProcess("program_.exe", "");
        }

        private static string GetCodeFromFile(string s)
        {
            try
            {
                return File.ReadAllText(s);
            }
            catch (Exception ex)
            {
                Console.Error.Write(ex.Message);
                return "";
            }
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

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(5000))
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit();

                throw new TimeoutException($"time out for: {fileName}");
            }

            string output = outputTask.GetAwaiter().GetResult();
            string error = errorTask.GetAwaiter().GetResult();

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
