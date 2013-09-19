using System;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.PatternMatching;
using System.Linq;

namespace CSharpLinter
{
    public class UseImplicitVariableTypeInDeclarationPolicy : LintPolicy
    {
        private VariableDeclarationStatement m_Pattern;

        public UseImplicitVariableTypeInDeclarationPolicy(LintResults results)
            : base(results)
        {
            this.m_Pattern = new VariableDeclarationStatement {
                Type = new AnyNode("type"),
                Variables = {
                    new VariableInitializer {
                        Name = Pattern.AnyString,
                        Initializer = new AnyNode("initializer")
                    }
                }};    
        }

        public override void VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement)
        {
            base.VisitVariableDeclarationStatement(variableDeclarationStatement);

            var match = this.m_Pattern.Match(variableDeclarationStatement);
            if (match.Success)
            {
                var type = match.Get<AstType>("type").Single();
                var initializer = match.Get<Expression>("initializer").Single();

                // FIXME: Is there a better way to test to see if the original type was 'var'?
                if (type.ToString() == "var" || type.ToString() == "dynamic")
                    return;

                // FIXME: Is there a better way to detect if the initializer is untyped?
                if (initializer.ToString() == "null")
                    return;

                // FIXME: Is there a better way to detect lambdas?
                if (initializer.ToString().Contains("=>") &&
                    !initializer.ToString().Trim().StartsWith("new"))
                    return;

                /*var original = variableDeclarationStatement.ToString();
                var replacementNode = variableDeclarationStatement.Clone();
                var replacementMatch = this.m_Pattern.Match(replacementNode);
                replacementMatch.Get<AstType>("type").Single().ReplaceWith(new SimpleType("var"));
                var replacement = replacementNode.ToString();*/
                this.Results.Issues.Add(new LintIssue(type)
                {
                    Index = LintIssueIndex.UseImplicitVariableDeclaration,
                    OriginalText = type.ToString(),
                    ReplacementText = "var"
                });
            }
        }
    }
}

