using System;

namespace CSharpLinter
{
    public class LintIssue
    {
        public int LineNumber;
        public LintIssueIndex Index;
        public string[] Parameters;

        public LintIssue()
        {
            this.Parameters = new string[0];
        }
    }

    public enum LintSeverity
    {
        ADVICE,
        AUTOFIX,
        WARNING,
        ERROR,
        DISABLED
    }
}

