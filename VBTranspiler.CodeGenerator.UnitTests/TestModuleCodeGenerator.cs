using Microsoft.VisualStudio.TestTools.UnitTesting;
using VBTranspiler.Parser;

namespace VBTranspiler.CodeGenerator.UnitTests
{
    [TestClass]
    public class TestModuleCodeGenerator : TestBase
    {
        protected override CodeGeneratorBase CreateCodeGenerator(VisualBasic6Parser.ModuleContext parseTree) => new ModuleCodeGenerator(parseTree);

        [TestMethod]
        public void TestClassNameTakenFromVBNameAttributeForModule()
        {
            var inputCode =
                @"VERSION 1.0 CLASS
Attribute VB_Name = ""SomeModule""
";

            var expectedCode =
                @"Imports System
Imports Microsoft.VisualBasic

Public Module SomeModule
End Module
";

            VerifyGeneratedCode(inputCode, expectedCode);
        }
    }
}