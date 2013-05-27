using System;
using ICSharpCode.NRefactory.CSharp;
using System.IO;
using System.Web.Script.Serialization;

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

            var serializer = new JavaScriptSerializer();
            Console.Write(serializer.Serialize(results));
        }
    }
}
