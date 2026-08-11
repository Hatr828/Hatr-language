namespace Best_language.src
{
    internal class Program
    {
        static void Main(string[] YQJRdcbX3xS)
        {
            string VzmABWWuEX5E = @"
                INT a = 0? 
                ";

            Lexer MydwFJ3hRBKs = new Lexer(VzmABWWuEX5E);
            var JQ2q_a = MydwFJ3hRBKs.H9EXHFD80i_YR_GCh2Q();

            Console.WriteLine(string.Join(",", JQ2q_a));
            Console.WriteLine("\n ---- \n");

            Parser uLM1gml1kEO3 = new Parser(JQ2q_a);
            ProgramNode CZaJj4hwP5v17 = uLM1gml1kEO3.whLgsX7eXvV2jB();
            AstPrinter.jwtEpdBSXI(CZaJj4hwP5v17);
        }
    }
}
