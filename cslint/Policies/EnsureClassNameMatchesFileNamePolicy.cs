using System;
using ICSharpCode.NRefactory.CSharp;

namespace CSharpLinter
{
    public class EnsureClassNameMatchesFileNamePolicy : LintPolicy
    {
        private int m_TotalClasses;
        private string m_FirstClassName;

        public EnsureClassNameMatchesFileNamePolicy(LintResults results)
            : base(results)
        {
            this.m_TotalClasses = 0;
            this.m_FirstClassName = null;
        }

        public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        {
            if (typeDeclaration.HasModifier(Modifiers.Public))
            {
                this.m_TotalClasses++;
                if (this.m_TotalClasses > 1)
                {
                    this.Results.Issues.Add(new LintIssue
                    {
                        LineNumber = typeDeclaration.StartLocation.Line,
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
            }

            var parent = typeDeclaration.GetParent<TypeDeclaration>();
            if (parent != null && typeDeclaration.HasModifier(Modifiers.Public))
            {
                this.Results.Issues.Add(new LintIssue
                {
                    LineNumber = typeDeclaration.StartLocation.Line,
                    Index = LintIssueIndex.PublicNestedClassDefined,
                    Parameters = new[] 
                    { 
                        typeDeclaration.Name
                    }
                });
            }

            var idx = this.Results.FileName.LastIndexOf('.');
            var filename = this.Results.FileName;
            if (idx != -1)
                filename = filename.Substring(0, idx);
            if (typeDeclaration.Name != filename)
            {
                this.Results.Issues.Add(new LintIssue
                {
                    LineNumber = typeDeclaration.StartLocation.Line,
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

