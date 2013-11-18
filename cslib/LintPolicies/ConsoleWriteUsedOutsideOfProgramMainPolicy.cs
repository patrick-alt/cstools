using System;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.PatternMatching;

namespace cslib
{
    public class ConsoleWriteUsedOutsideOfProgramMainPolicy : LintPolicy
    {
        private InvocationExpression m_Pattern;

        public ConsoleWriteUsedOutsideOfProgramMainPolicy(LintResults results)
            : base(results)
        {
            this.m_Pattern = new InvocationExpression {
                Target = new MemberReferenceExpression
                {
                    Target = new IdentifierExpression("Console"),
                    MemberName = Pattern.AnyString,
                },
                Arguments = { new Repeat(new AnyNode()) }
            };
        }

        public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        {
            var match = this.m_Pattern.Match(invocationExpression);
            if (match.Success)
            {
                var method = (invocationExpression.Target as MemberReferenceExpression).MemberName;
                if (method == "Write" || method == "WriteLine")
                {
                    // FIXME: Couldn't get a pattern matcher to work on the MethodDeclaration.
                    var declMethod = invocationExpression.GetParent<MethodDeclaration>();
                    if (declMethod == null)
                    {
                        base.VisitInvocationExpression(invocationExpression);
                        return;
                    }
                    var allowed = (
                        declMethod.Name == "Main" &&
                        declMethod.Modifiers == (Modifiers.Public | Modifiers.Static) &&
                        declMethod.ReturnType.ToString() == "void" &&
                        declMethod.Parameters.Count == 1 &&
                        declMethod.Parameters.ToList()[0].Type.ToString() == "string[]");
                    if (!allowed)
                    {
                        this.Results.Issues.Add(new LintIssue(invocationExpression)
                        {
                            Index = LintIssueIndex.ConsoleWriteUsedOutsideOfProgramMain,
                            Parameters = new[]
                            {
                                declMethod.GetParent<TypeDeclaration>().Name + "." + declMethod.Name
                            }
                        });
                    }
                }
            }

            base.VisitInvocationExpression(invocationExpression);
        }
    }
}

