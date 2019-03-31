using System;
using System.IO;
using System.Linq;
using VBTranspiler.CodeGenerator;
using VBTranspiler.Parser;

namespace VBTranspiler
{
    public class InOut
    {
        private static readonly object _consoleLock = new object();
        public string OutFile { get; set; }
        public string SourceFile { get; set; }

        public void Transpile()
        {
            this.WritePaths();
            var parseTree = VisualBasic6Parser.ParseSource(this.SourceFile);
            CodeGeneratorBase codeGen;
            switch (Path.GetExtension(this.SourceFile)?.ToLower())
            {
                default:
                case ".cls":
                    codeGen = new ClassModuleCodeGenerator(parseTree);
                    break;
                case ".bas":
                    codeGen = new ModuleCodeGenerator(parseTree);
                    break;
                case ".frm":
                    codeGen = new FormCodeGenerator(parseTree);
                    break;
                case ".ctl":
                    codeGen = new UserControlCodeGenerator(parseTree);
                    break;
            }

            if (File.Exists(this.OutFile))
                File.Delete(this.OutFile);
            using (var output = File.CreateText(this.OutFile))
            {
                output.Write(codeGen.GenerateCode());
            }
        }

        private void WritePaths()
        {
            lock (_consoleLock)
            {
                Console.Write("From: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(this.SourceFile);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("\tTo: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(this.OutFile);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }

    internal class Program
    {
        private static readonly string[] _extensions = { "*.cls", "*.bas", "*.frm", "*.ctl" };

        private static void Main(string[] args)
        {
#if DEBUG
            var srcdir = new DirectoryInfo(@"C:\Users\saustin\Code\Unified2\SedonaOffice");
#else
            var srcdir = new DirectoryInfo(args.FirstOrDefault() ?? Directory.GetCurrentDirectory());
#endif
            var outdir = srcdir.Parent?.CreateSubdirectory($"{srcdir.Name}Converted") ?? Directory.CreateDirectory($"\\{srcdir.Name}Converted");
            var files = _extensions
                        .Select(x => srcdir.EnumerateFiles(x, SearchOption.AllDirectories))
                        .SelectMany(x => x.Select(y => new InOut
                        {
                            SourceFile = y.FullName,
                            OutFile = y.FullName.Replace(srcdir.FullName, outdir.FullName).Replace(Path.GetExtension(y.FullName), ".vb")
                        }))
                        .Take(1);
            foreach (var x in files)
                x.Transpile();
        }
    }
}