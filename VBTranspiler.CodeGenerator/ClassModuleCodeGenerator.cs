using System.Collections.Generic;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using VBTranspiler.Parser;

namespace VBTranspiler.CodeGenerator
{
    public class ClassModuleCodeGenerator : CodeGeneratorBase
    {
        public ClassModuleCodeGenerator(VisualBasic6Parser.ModuleContext parseTree)
            : base(parseTree)
        {
        }

        protected override void AddAdditionalImports(List<ImportsStatementSyntax> imports)
        {
            //Stub
        }

        /// <summary>
        /// Creates a class declaration.
        /// </summary>
        /// <returns>The class declaration AST node.</returns>
        protected override TypeBlockSyntax CreateTopLevelTypeDeclaration(IEnumerable<StatementSyntax> members)
        {
            var publicModifier = RoslynUtils.PublicModifier;
            return SyntaxFactory.ClassBlock(SyntaxFactory.ClassStatement(GetVBNameAttributeValue()).WithModifiers(publicModifier))
                                .WithMembers(SyntaxFactory.List(members));
        }
    }
}