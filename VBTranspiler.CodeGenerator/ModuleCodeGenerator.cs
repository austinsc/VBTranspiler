﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using VBTranspiler.Parser;

namespace VBTranspiler.CodeGenerator
{
    public class ModuleCodeGenerator : CodeGeneratorBase
    {
        public ModuleCodeGenerator(VisualBasic6Parser.ModuleContext parseTree)
            : base(parseTree)
        {
        }

        protected override void AddAdditionalImports(List<ImportsStatementSyntax> imports)
        {
            //None
        }

        protected override TypeBlockSyntax CreateTopLevelTypeDeclaration(IEnumerable<StatementSyntax> members) => SyntaxFactory.ModuleBlock(SyntaxFactory.ModuleStatement(GetVBNameAttributeValue()).WithModifiers(RoslynUtils.PublicModifier));
    }
}