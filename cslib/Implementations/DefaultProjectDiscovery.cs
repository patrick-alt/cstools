using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Linq;

namespace cslib
{
    public class DefaultProjectDiscovery : IProjectDiscovery
    {
        private readonly ISettings m_Settings;
        
        public DefaultProjectDiscovery(
            ISettings settings)
        {
            this.m_Settings = settings;
        }
        
        public IEnumerable<DiscoveredProject> DiscoverProjects(string file)
        {
            dynamic patterns = this.m_Settings.Get();
            Console.Error.WriteLine("Starting pattern match for " + file);
            foreach (var kv in patterns)
            {
                var regexPattern = kv.Key;
                var potentialRegexMatches = kv.Value;
                if (!Regex.IsMatch(file, regexPattern))
                {
                    Console.Error.WriteLine(file + " does not match " + regexPattern);
                    continue;
                }
                foreach (var potentialRegexMatch in potentialRegexMatches)
                {
                    var result = Regex.Replace(file, regexPattern, potentialRegexMatch.Value);
                    if (File.Exists(result))
                        yield return new DiscoveredProject
                        {
                            Path = result,
                            DiscoveredFiles = this.DiscoverFiles(result)
                        };
                    else
                        Console.Error.WriteLine(result + " does not exist on disk");
                }
            }
        }
        
        private IEnumerable<DiscoveredSourceFile> DiscoverFiles(string projectPath)
        {
            var doc = XDocument.Load(projectPath);
            foreach (var compile in doc.Descendants(XName.Get("Compile", "http://schemas.microsoft.com/developer/msbuild/2003")))
            {
                var include = compile.Attribute(XName.Get("Include")).Value;
                include = include.Replace('/', Path.DirectorySeparatorChar);
                include = include.Replace('\\', Path.DirectorySeparatorChar);
                var baseProject = new FileInfo(projectPath).Directory.FullName;
                var combine = Path.Combine(baseProject, include);
                if (File.Exists(combine))
                    yield return new DiscoveredSourceFile { Path = combine };
            }
        }
    }
}

