using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace VBTranspiler.CodeGenerator
{
    public static class RoslynUtils
    {
        /// <summary>
        /// Creates a syntax token list representing a friend modifier.
        /// </summary>
        public static SyntaxTokenList FriendModifier => SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.FriendKeyword));

        /// <summary>
        /// Creates a syntax token list representing a private modifier.
        /// </summary>
        public static SyntaxTokenList PrivateModifier => SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

        /// <summary>
        /// Creates a syntax token list representing a public modifier.
        /// </summary>
        public static SyntaxTokenList PublicModifier => SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
    }
}