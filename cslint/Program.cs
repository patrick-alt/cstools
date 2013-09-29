using System;
using System.Collections.Generic;
using System.IO;
using NDesk.Options;
using Newtonsoft.Json;
using Ninject;
using cslib;

namespace cslint
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var kernel = new StandardKernel();
            kernel.Load<CSharpLibraryNinjectModule>();
            
            string file = null;
            string settings = null;
            string root = null;
            bool help = false;
            var options = new OptionSet
            {
                { "s|settings=", v => settings = v },
                { "r|root=", v => root = v },
                { "h|help", v => help = true }
            };
            
            List<string> extra;
            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("cslint: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `cslint --help' for more information.");
                return;
            }
            
            if (help)
            {
                ShowHelp(options);
                return;
            }
            
            if (extra.Count > 0)
            {
                file = extra[0];
            }
            
            if (extra.Count > 1)
            {
                Console.Write("cslint: ");
                Console.WriteLine("You can only specify one file to lint.");
                Console.WriteLine("Try `cslint --help' for more information.");
                return;
            }
            
            if (string.IsNullOrEmpty(file))
            {
                Console.Write("cslint: ");
                Console.WriteLine("You must supply at the file to lint.");
                Console.WriteLine("Try `cslint --help' for more information.");
                return;
            }
            
            if (!File.Exists(file))
            {
                Console.Write("cslint: ");
                Console.WriteLine("Unable to locate the file to lint on disk.");
                Console.WriteLine("Try `cslint --help' for more information.");
                return;
            }
            
            var settingsJson = new JsonSettings(settings);
            kernel.Bind<ISettings>().ToMethod(x => settingsJson);
            
            var results = new LintResults();
            results.FileName = new FileInfo(file).FullName;
            results.BaseName = new FileInfo(file).Name;
            
            if (root != null && Directory.Exists(root))
                Directory.SetCurrentDirectory(root);
            
            if (results.FileName.StartsWith(Environment.CurrentDirectory))
                results.FileName = results.FileName.Substring(Environment.CurrentDirectory.Length + 1);
            
            var linters = kernel.GetAll<ILinter>();
            foreach (var linter in linters)
                linter.Process(results);

            Console.Write(JsonConvert.SerializeObject(results));
        }
        
        private static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: cslint [OPTIONS]+ filename");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
