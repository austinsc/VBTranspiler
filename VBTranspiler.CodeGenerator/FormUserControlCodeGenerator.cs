using System.Collections.Generic;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using VBTranspiler.Parser;

namespace VBTranspiler.CodeGenerator
{
    public abstract class FormsCodeGeneratorBase : ClassModuleCodeGenerator
    {
        public FormsCodeGeneratorBase(VisualBasic6Parser.ModuleContext parseTree)
            : base(parseTree)
        {
        }

        protected abstract TypeSyntax InheritsType { get; }

        protected override TypeBlockSyntax CreateTopLevelTypeDeclaration(IEnumerable<StatementSyntax> members)
        {
            var classDecl = (ClassBlockSyntax)base.CreateTopLevelTypeDeclaration(members);

            TypeSyntax[] typeArr = { InheritsType };
            InheritsStatementSyntax[] inheritArr = { SyntaxFactory.InheritsStatement(typeArr) };
            var inherits = SyntaxFactory.List(inheritArr);

            classDecl = classDecl.WithInherits(inherits);

            return classDecl;
        }

        protected override void AddAdditionalImports(List<ImportsStatementSyntax> imports)
        {
            imports.Add(CreateImportStatement("System.Windows.Forms"));
        }
    }

    public class FormCodeGenerator : FormsCodeGeneratorBase
    {
        public FormCodeGenerator(VisualBasic6Parser.ModuleContext parseTree)
            : base(parseTree)
        {
        }

        protected override TypeSyntax InheritsType => SyntaxFactory.ParseTypeName("Form");
    }

    public class UserControlCodeGenerator : FormsCodeGeneratorBase
    {
        public UserControlCodeGenerator(VisualBasic6Parser.ModuleContext parseTree)
            : base(parseTree)
        {
        }

        protected override TypeSyntax InheritsType => SyntaxFactory.ParseTypeName("UserControl");
    }
}