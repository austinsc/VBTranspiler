using System.IO;
using VBTranspiler.CodeGenerator;
using VBTranspiler.Parser;

namespace VBTranspiler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var inputFile = @"Test2.frm";
            var outputFile = @"Test.vb";

            var parseTree = VisualBasic6Parser.ParseSource(inputFile);
            var codeGen = new ClassModuleCodeGenerator(parseTree);

            using (var output = new StreamWriter(outputFile))
            {
                output.Write(codeGen.GenerateCode());
            }
        }
    }
}