using ICSharpCode.NRefactory.CSharp;

namespace cslib
{
    public class EnsureClassNameMatchesFileNamePolicy : LintPolicy
    {
        public EnsureClassNameMatchesFileNamePolicy(LintResults results)
            : base(results)
        {
        }

        public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        {
            if (!typeDeclaration.HasModifier(Modifiers.Public))
            {
                base.VisitTypeDeclaration(typeDeclaration);
                return;
            }

            var idx = this.Results.BaseName.LastIndexOf('.');
            var filename = this.Results.BaseName;
            if (idx != -1)
                filename = filename.Substring(0, idx);
            if (typeDeclaration.Name != filename)
            {
                this.Results.Issues.Add(new LintIssue(typeDeclaration)
                {
                    Index = LintIssueIndex.ClassNameDoesNotMatchFileName,
                    Parameters = new[]
                    {
                        typeDeclaration.Name,
                        filename
                    }
                });
            }

            base.VisitTypeDeclaration(typeDeclaration);
        }
    }
}

