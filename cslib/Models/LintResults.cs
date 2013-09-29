using System.Collections.Generic;

namespace cslib
{
    public class LintResults
    {
        public List<LintIssue> Issues = new List<LintIssue>();
        public string FileName;
        public string BaseName;
    }
}

