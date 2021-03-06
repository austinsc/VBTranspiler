﻿using System;
using System.Diagnostics;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace VBTranspiler.Parser
{
    public partial class VisualBasic6Parser
    {
        public static ModuleContext ParseSource(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return ParseSource(stream);
            }
        }

        public static ModuleContext ParseSource(Stream stm)
        {
            var input = new AntlrInputStream(stm);
            ITokenStream tokens = new CommonTokenStream(new VisualBasic6Lexer(input));

            var parser = new VisualBasic6Parser(tokens);
            parser.AddParseListener(new ParserListener(parser));
            parser.AddErrorListener(new DebugErrorListener<IToken>());

            var ret = parser.module();

            if (parser.NumberOfSyntaxErrors > 0)
                throw new ApplicationException("Parser errors encountered");

            return ret;
        }
    }

    public class ParserListener : IParseTreeListener
    {
        private int mIndent;
        private readonly VisualBasic6Parser mParser;

        public ParserListener(VisualBasic6Parser parser) => mParser = parser;

        public void EnterEveryRule(ParserRuleContext ctx)
        {
            mIndent += 1;

            //Debug.Write("".PadLeft(mIndent));
            //Debug.WriteLine("Enter {0} {1}", mParser.RuleNames[ctx.RuleIndex], ctx.Start.Text);
        }

        public void ExitEveryRule(ParserRuleContext ctx)
        {
            //Debug.Write("".PadLeft(mIndent));
            //Debug.WriteLine(string.Format("Exit {0}", mParser.RuleNames[ctx.RuleIndex]));

            mIndent -= 1;
        }

        public void VisitErrorNode(IErrorNode node)
        {
        }

        public void VisitTerminal(ITerminalNode node)
        {
        }
    }

    public class DebugErrorListener<Symbol> : IAntlrErrorListener<Symbol>
    {
        public virtual void SyntaxError(IRecognizer recognizer, Symbol offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            Debug.WriteLine("line " + line + ":" + charPositionInLine + " " + msg);
        }
    }
}