using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using VBTranspiler.Parser;

namespace VBTranspiler.CodeGenerator
{
    public abstract class CodeGeneratorBase : VisualBasic6BaseVisitor<CodeGeneratorBase>
    {
        /// <summary>
        /// Members of the main class/module.
        /// </summary>
        private readonly List<StatementSyntax> mMainDeclMembers;
        /// <summary>
        /// The parse tree to generate code from.
        /// </summary>
        private readonly VisualBasic6Parser.ModuleContext mParseTree;

        /// <summary>
        /// Initialises this instance with the given VB6 AST.
        /// </summary>
        /// <param name="parseTree"></param>
        public CodeGeneratorBase(VisualBasic6Parser.ModuleContext parseTree)
        {
            mParseTree = parseTree;
            mMainDeclMembers = new List<StatementSyntax>();
        }

        /// <summary>
        /// Generate the .NET code for a VB6 AST.
        /// </summary>
        /// <returns></returns>
        public string GenerateCode()
        {
            //Walk the parse tree, generating the equivalent .NET code along the way.
            Visit(mParseTree);

            //Build the list of imports
            var imports = new List<ImportsStatementSyntax>();
            imports.Add(CreateImportStatement("System"));

            AddAdditionalImports(imports);
            imports.Add(CreateImportStatement("Microsoft.VisualBasic"));

            //Build the main compilation unit now that all the members have been processed.
            var cu = SyntaxFactory.CompilationUnit()
                                  .AddImports(imports.ToArray())
                                  .AddMembers(CreateTopLevelTypeDeclaration(mMainDeclMembers))
                                  .NormalizeWhitespace();

            return cu.ToFullString();
        }

        /// <summary>
        /// Creates the top level class or module declaration.
        /// </summary>
        /// <returns>The type declaration.</returns>
        protected abstract TypeBlockSyntax CreateTopLevelTypeDeclaration(IEnumerable<StatementSyntax> members);

        protected abstract void AddAdditionalImports(List<ImportsStatementSyntax> imports);

        /// <summary>
        /// Creates a module declaration.
        /// </summary>
        /// <returns>The module declaration AST node.</returns>
        private ModuleBlockSyntax CreateModuleDeclaration()
        {
            var publicModifier = RoslynUtils.PublicModifier;
            return SyntaxFactory.ModuleBlock(SyntaxFactory.ModuleStatement(GetVBNameAttributeValue()).WithModifiers(publicModifier))
                                .WithMembers(SyntaxFactory.List(mMainDeclMembers));
        }

        /// <summary>
        /// Creates a new import clause for the specified namespace.
        /// </summary>
        /// <param name="namespaceName">Namespace to import</param>
        /// <returns>The new import statement AST node</returns>
        protected ImportsStatementSyntax CreateImportStatement(string namespaceName)
        {
            var importsClause = SyntaxFactory.SimpleImportsClause(SyntaxFactory.ParseName(namespaceName));
            return SyntaxFactory.ImportsStatement(SyntaxFactory.SeparatedList(new ImportsClauseSyntax[] { importsClause }));
        }

        /// <summary>
        /// Gets the name of the VB6 class/module
        /// </summary>
        /// <returns></returns>
        protected string GetVBNameAttributeValue()
        {
            foreach (var attr in mParseTree.moduleAttributes().attributeStmt())
                if (attr.implicitCallStmt_InStmt().GetText() == "VB_Name")
                    return attr.literal()[0].STRINGLITERAL().GetText().Trim('"');

            throw new ApplicationException("Unable to determine class name.");
        }

        /// <summary>
        /// Generates .NET code for an enumeration.
        /// </summary>
        /// <param name="context">Enumeration AST.</param>
        /// <returns></returns>
        public override CodeGeneratorBase VisitEnumerationStmt(VisualBasic6Parser.EnumerationStmtContext context)
        {
            var accessibility = new SyntaxTokenList();
            var vis = context.publicPrivateVisibility();

            if (vis != null)
            {
                if (vis.PUBLIC() != null)
                    accessibility = RoslynUtils.PublicModifier;
                else if (vis.PRIVATE() != null)
                    accessibility = RoslynUtils.PrivateModifier;
            }

            var stmt = SyntaxFactory.EnumStatement(context.ambiguousIdentifier().GetText()).WithModifiers(accessibility);
            var enumBlock = SyntaxFactory.EnumBlock(stmt);
            var members = new List<EnumMemberDeclarationSyntax>();

            foreach (var constant in context.enumerationStmt_Constant())
            {
                var constantName = constant.ambiguousIdentifier().GetText();

                if (constant.valueStmt() != null)
                {
                    var constantValue = constant.valueStmt().GetText();

                    members.Add(SyntaxFactory.EnumMemberDeclaration(constantName)
                                             .WithInitializer(SyntaxFactory.EqualsValue(SyntaxFactory.NumericLiteralExpression(SyntaxFactory.ParseToken(constantValue)))));
                }
                else
                {
                    members.Add(SyntaxFactory.EnumMemberDeclaration(constantName));
                }
            }

            mMainDeclMembers.Add(enumBlock.WithMembers(SyntaxFactory.List<StatementSyntax>(members)));
            return this;
        }

        /// <summary>
        /// Generates .NET code a constant field decl.
        /// </summary>
        /// <param name="context">Const decl AST.</param>
        /// <returns></returns>
        public override CodeGeneratorBase VisitConstStmt(VisualBasic6Parser.ConstStmtContext context)
        {
            var modifiers = new List<SyntaxToken>();
            var vis = context.publicPrivateGlobalVisibility();

            if (vis != null)
            {
                if (vis.PUBLIC() != null || vis.GLOBAL() != null)
                    modifiers.Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                else if (vis.PRIVATE() != null)
                    modifiers.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
            }

            modifiers.Add(SyntaxFactory.Token(SyntaxKind.ConstKeyword));

            foreach (var subStmt in context.constSubStmt())
            {
                SimpleAsClauseSyntax asClause = null;

                if (subStmt.asTypeClause() != null)
                    asClause = SyntaxFactory.SimpleAsClause(SyntaxFactory.ParseTypeName(subStmt.asTypeClause().type().GetText()));

                var identifier = SyntaxFactory.ModifiedIdentifier(subStmt.ambiguousIdentifier().GetText());
                ExpressionSyntax initialiser = null;

                var initialiserExpr = subStmt.valueStmt().GetText();

                //ParseExpression can't handle date/time literals - Roslyn bug? - //so handle them manually here
                if (initialiserExpr.StartsWith("#"))
                {
                    var trimmedInitialiser = initialiserExpr.Trim('#');
                    var parsedVal = DateTime.Parse(trimmedInitialiser);

                    //if (trimmedInitialiser.Contains("AM") || trimmedInitialiser.Contains("PM"))
                    //    parsedVal = DateTime.ParseExact(trimmedInitialiser, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                    //else
                    //    parsedVal = DateTime.ParseExact(trimmedInitialiser, "M/d/yyyy", CultureInfo.InvariantCulture);

                    initialiser = SyntaxFactory.DateLiteralExpression(SyntaxFactory.DateLiteralToken(initialiserExpr, parsedVal));
                }
                else
                {
                    initialiser = SyntaxFactory.ParseExpression(initialiserExpr);
                }

                var varDecl = SyntaxFactory.VariableDeclarator(identifier).WithInitializer(SyntaxFactory.EqualsValue(initialiser));

                if (asClause != null)
                    varDecl = varDecl.WithAsClause(asClause);

                mMainDeclMembers.Add(SyntaxFactory.FieldDeclaration(varDecl).WithModifiers(SyntaxFactory.TokenList(modifiers)));
            }

            return base.VisitConstStmt(context);
        }
    }
}