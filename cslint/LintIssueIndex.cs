using System;

namespace CSharpLinter
{
    public class LintIssueIndex
    {
        public static readonly LintIssueIndex MoreThanOneClassInFile = new LintIssueIndex
        {
            Name = "More Than One Class In File",
            Code = "0001",
            Message = "You have more than one class declared in a single file.  " +
                "'%s' was found, but '%s' has already been declared.",
            Severity = LintSeverity.WARNING
        };
        public static readonly LintIssueIndex PublicNestedClassDefined = new LintIssueIndex
        {
            Name = "Public Nested Class Defined",
            Code = "0002",
            Message = "You have defined a nested public class called '%s'.  " +
                "Nested public classes are not recommended as they can't " +
                "be easily addressed from other code without a " +
                "fully qualified name.",
            Severity = LintSeverity.WARNING
        };
        public static readonly LintIssueIndex ClassNameDoesNotMatchFileName = new LintIssueIndex
        {
            Name = "Class Name Does Not Match Filename",
            Code = "0003",
            Message = "The only public class in a file should match the name " +
                "of the file that it is declared in.  The class name was '%s' " +
                "but the filename is '%s'.",
            Severity = LintSeverity.WARNING
        };

        public string Name { get; private set; }
        public string Code { get; private set; }
        public string Message { get; private set; }
        public LintSeverity Severity { get; private set; }

        private LintIssueIndex()
        {
        }
    }
}

