using System;
using ICSharpCode.NRefactory.CSharp;
using Newtonsoft.Json;
using System.IO;

namespace CSharpLinter
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var file = args[0];
            var parser = new CSharpParser();
            SyntaxTree tree;
            using (var reader = new StreamReader(file))
            {
                tree = parser.Parse(reader.ReadToEnd(), file);
            }
            var results = new LintResults();
            results.FileName = new FileInfo(file).Name;

            // Apply policies
            tree.AcceptVisitor(new EnsureClassNameMatchesFileNamePolicy(results));
            tree.AcceptVisitor(new EnsureNoNestedPublicClassesPolicy(results));
            tree.AcceptVisitor(new EnsureOnePublicClassPerFilePolicy(results));
            tree.AcceptVisitor(new UseImplicitVariableTypeInDeclarationPolicy(results));
            tree.AcceptVisitor(new ConsoleWriteUsedOutsideOfProgramMainPolicy(results));

            Console.Write(JsonConvert.SerializeObject(results));
        }
    }
}
