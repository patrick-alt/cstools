using System;
using ICSharpCode.NRefactory.CSharp;

namespace CSharpLinter
{
    public class EnsureNoNestedPublicClassesPolicy : LintPolicy
    {
        public EnsureNoNestedPublicClassesPolicy(LintResults results)
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

            var parent = typeDeclaration.GetParent<TypeDeclaration>();
            if (parent != null && typeDeclaration.HasModifier(Modifiers.Public))
            {
                this.Results.Issues.Add(new LintIssue(typeDeclaration)
                {
                    Index = LintIssueIndex.PublicNestedClassDefined,
                    Parameters = new[]
                    {
                        typeDeclaration.Name
                    }
                });
            }

            base.VisitTypeDeclaration(typeDeclaration);
        }
    }
}

