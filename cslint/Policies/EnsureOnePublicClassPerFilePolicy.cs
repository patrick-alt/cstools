using System;
using ICSharpCode.NRefactory.CSharp;

namespace CSharpLinter
{
    public class EnsureOnePublicClassPerFilePolicy : LintPolicy
    {
        private int m_TotalClasses;
        private string m_FirstClassName;

        public EnsureOnePublicClassPerFilePolicy(LintResults results)
            : base(results)
        {
            this.m_TotalClasses = 0;
            this.m_FirstClassName = null;
        }

        public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        {
            if (!typeDeclaration.HasModifier(Modifiers.Public))
            {
                base.VisitTypeDeclaration(typeDeclaration);
                return;
            }

            this.m_TotalClasses++;
            if (this.m_TotalClasses > 1)
            {
                this.Results.Issues.Add(new LintIssue(typeDeclaration)
                {
                    Index = LintIssueIndex.MoreThanOneClassInFile,
                    Parameters = new[]
                    {
                        typeDeclaration.Name,
                        this.m_FirstClassName
                    }
                });
            }
            else
            {
                this.m_FirstClassName = typeDeclaration.Name;
            }

            base.VisitTypeDeclaration(typeDeclaration);
        }
    }
}

