using System.IO;
using ICSharpCode.NRefactory.CSharp;

namespace cslib
{
    public class BuiltinLinter : ILinter
    {
        public void Process(LintResults results)
        {
            var parser = new CSharpParser();
            SyntaxTree tree;
            using (var reader = new StreamReader(results.FileName))
            {
                tree = parser.Parse(reader.ReadToEnd(), results.FileName);
            }

            // Apply policies
            tree.AcceptVisitor(new EnsureClassNameMatchesFileNamePolicy(results));
            tree.AcceptVisitor(new EnsureNoNestedPublicClassesPolicy(results));
            tree.AcceptVisitor(new EnsureOnePublicClassPerFilePolicy(results));
            tree.AcceptVisitor(new UseImplicitVariableTypeInDeclarationPolicy(results));
            tree.AcceptVisitor(new ConsoleWriteUsedOutsideOfProgramMainPolicy(results));
        }
    }
}

