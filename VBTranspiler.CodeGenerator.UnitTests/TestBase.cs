using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VBTranspiler.Parser;

namespace VBTranspiler.CodeGenerator.UnitTests
{
    public abstract class TestBase
    {
        protected VisualBasic6Parser.ModuleContext ParseInputSource(string source)
        {
            using (var memStm = new MemoryStream(Encoding.ASCII.GetBytes(source)))
            {
                return VisualBasic6Parser.ParseSource(memStm);
            }
        }

        protected abstract CodeGeneratorBase CreateCodeGenerator(VisualBasic6Parser.ModuleContext parseTree);

        protected void VerifyGeneratedCode(string inputCode, string expectedCode)
        {
            var codeGen = CreateCodeGenerator(ParseInputSource(inputCode));
            var generatedCode = codeGen.GenerateCode();

            Assert.AreEqual(expectedCode, generatedCode);

            //Make sure the generated code is syntactically valid
            var parsedCode = VisualBasicSyntaxTree.ParseText(generatedCode);
            Assert.IsFalse(parsedCode.GetCompilationUnitRoot().ContainsDiagnostics);
        }
    }
}