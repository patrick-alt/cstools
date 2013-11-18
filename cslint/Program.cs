using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using cslib;
using NDesk.Options;
using Newtonsoft.Json;
using Ninject;

namespace cslint
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var kernel = new StandardKernel();
            kernel.Load<CSharpLibraryNinjectModule>();

            string settings = null;
            string root = null;
            bool version = false;
            bool help = false;
            var options = new OptionSet
            {
                { "s|settings=", v => settings = v },
                { "settings-base64=", v => settings = 
                    Encoding.ASCII.GetString(Convert.FromBase64String(v)) },
                { "r|root=", v => root = v },
                { "h|help", v => help = true },
                { "v|version", v => version = true }
            };

            List<string> files;
            try
            {
                files = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("cslint: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `cslint --help' for more information.");
                return;
            }

            if (version)
            {
                Console.WriteLine("1");
                return;
            }

            if (help)
            {
                ShowHelp(options);
                return;
            }

            if (files.Count == 0)
            {
                Console.Write("cslint: ");
                Console.WriteLine("You must supply at least one file to lint.");
                Console.WriteLine("Try `cslint --help' for more information.");
                return;
            }

            if (!files.All(File.Exists))
            {
                Console.Write("cslint: ");
                Console.WriteLine("Unable to locate one or more files specified.");
                Console.WriteLine("Try `cslint --help' for more information.");
                return;
            }

            var settingsJson = new JsonSettings(settings);
            kernel.Bind<ISettings>().ToMethod(x => settingsJson);

            var allResults = new List<LintResults>();
            foreach (var file in files)
            {
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

                allResults.Add(results);
            }

            Console.Write(JsonConvert.SerializeObject(allResults));
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
