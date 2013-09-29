using System;
using StyleCop;
using System.Collections.Generic;
using System.IO;

namespace cslib
{
    public class StyleCopLinter : ILinter
    {
        private readonly IProjectDiscovery m_ProjectDiscovery;
    
        public StyleCopLinter(
            IProjectDiscovery projectDiscovery)
        {
            this.m_ProjectDiscovery = projectDiscovery;
        }
    
        public void Process(LintResults results)
        {
            var file = results.FileName;
            
            // Start the StyleCop console.
            var console = new StyleCopConsole(Environment.CurrentDirectory, true, null, null, true);
            
            // Create the StyleCop configuration.
            var configuration = new Configuration(new string[] { "DEBUG" });
            
            // Add the source files.
            var projects = new List<CodeProject>();  
            foreach (var myProject in this.m_ProjectDiscovery.DiscoverProjects(file))
            {
                var project = new CodeProject(myProject.Path.GetHashCode(), myProject.Path, configuration);
            
                // Add each source file to this project.
                foreach (var sourceFilePath in myProject.DiscoveredFiles)
                {
                    console.Core.Environment.AddSourceCode(project, sourceFilePath.Path, null);
                }
                
                projects.Add(project);
            }
            
            // Define event handlers.
            EventHandler<OutputEventArgs> outputGenerated = (sender, e) =>
            {
            };
            EventHandler<ViolationEventArgs> violationEncountered = (sender, e) =>
            {
                if (e.SourceCode.Path != Path.Combine(Environment.CurrentDirectory, file))
                    return;
                var index = new LintIssueIndex
                {
                    Name = e.Violation.Rule.Name,
                    Code = e.Violation.Rule.CheckId,
                    Message = e.Message,
                    Severity = e.Warning ? LintSeverity.WARNING : LintSeverity.ERROR
                };
                results.Issues.Add(new LintIssue
                {
                    Index = index,
                    LineNumber = e.LineNumber,
                    Column = 0
                });
            };
            
            // Assign event handlers.
            console.OutputGenerated += outputGenerated;
            console.ViolationEncountered += violationEncountered;
            
            // Run StyleCop.
            console.Start(projects, false);
            
            // Finalise.
            console.OutputGenerated -= outputGenerated;
            console.ViolationEncountered -= violationEncountered;
        }
    }
}

