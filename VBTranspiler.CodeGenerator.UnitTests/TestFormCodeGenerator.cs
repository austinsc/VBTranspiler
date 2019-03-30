using Microsoft.VisualStudio.TestTools.UnitTesting;
using VBTranspiler.Parser;

namespace VBTranspiler.CodeGenerator.UnitTests
{
    [TestClass]
    public class TestFormCodeGenerator : TestBase
    {
        protected override CodeGeneratorBase CreateCodeGenerator(VisualBasic6Parser.ModuleContext parseTree) => new FormCodeGenerator(parseTree);

        [TestMethod]
        public void TestClassNameTakenFromVBNameAttributeForForm()
        {
            var inputCode =
                @"VERSION 1.0 CLASS
Attribute VB_Name = ""SomeForm""
";

            var expectedCode =
                @"Imports System
Imports System.Windows.Forms
Imports Microsoft.VisualBasic

Public Class SomeForm
    Inherits Form

End Class
";

            VerifyGeneratedCode(inputCode, expectedCode);
        }
    }
}