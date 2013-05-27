using System.Collections.Generic;

namespace CSharpLinter
{
    public class LintResults
    {
        public List<LintIssue> Issues = new List<LintIssue>();
        public string FileName;
    }
}

