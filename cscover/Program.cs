using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NDesk.Options;
using Ninject;
using cslib;
using System.Diagnostics;
using System.Xml;

namespace cscover
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var kernel = new StandardKernel();
            kernel.Load<CSharpLibraryNinjectModule>();
            
            string output = null;
            string command = null;
            string commandArgs = null;
            string workDir = null;
            bool help = false;
            string copyTo = null;
            var options = new OptionSet
            {
                { "o|output=", v => output = v },
                { "c|command=", v => command = v },
                { "a|args=", v => commandArgs = v },
                { "w|work-dir=", v => workDir = v },
                { "h|help", v => help = true }
            };
            
            List<string> extra;
            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("cscover: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `cscover --help' for more information.");
                return;
            }
            
            if (help)
            {
                ShowHelp(options);
                return;
            }
            
            if (extra.Count == 0)
            {
                Console.Write("cscover: ");
                Console.WriteLine("You must supply at least one assembly to instrument.");
                Console.WriteLine("Try `cscover --help' for more information.");
                return;
            }
            
            if (extra.Any(x => !File.Exists(x)))
            {
                Console.Write("cscover: ");
                Console.WriteLine("Unable to find `" + extra.First(x => !File.Exists(x)) + "` to instrument.");
                Console.WriteLine("Try `cscover --help' for more information.");
                return;
            }
            
            var trackTemp = Path.GetTempFileName();
            int total = 0;
            var instrumented = new List<ReportLine>();
            
            try
            {
                var instrumenter = kernel.Get<IInstrumenter>();
                foreach (var assemblyFile in extra)
                {
                    try
                    {
                        // Backup the original assembly.
                        Console.WriteLine("Backing up " + assemblyFile);
                        if (File.Exists(assemblyFile + ".bak"))
                            File.Delete(assemblyFile + ".bak");
                        File.Move(assemblyFile, assemblyFile + ".bak");
                        if (File.Exists(assemblyFile + ".mdb"))
                        {
                            if (File.Exists(assemblyFile + ".bak.mdb"))
                                File.Delete(assemblyFile + ".bak.mdb");
                            File.Move(assemblyFile + ".mdb", assemblyFile + ".bak.mdb");
                        }
                        
                        // Instrument the assembly for execution.
                        Console.WriteLine("Instrumenting " + assemblyFile);
                        var assembly = AssemblyDefinition.ReadAssembly(assemblyFile + ".bak", new ReaderParameters { ReadSymbols = File.Exists(assemblyFile + ".bak.mdb") });
                        total += instrumenter.InstrumentAssembly(
                            assembly,
                            trackTemp,
                            (start, end, document) => 
                                instrumented.Add(new ReportLine { StartLine = start, EndLine = end, Document = document }));
                        assembly.Write(assemblyFile);
                    }
                    catch (ApplicationException ex)
                    {
                        Console.WriteLine("Unable to instrument " + assemblyFile + " because " + ex.Message);
                    }
                    catch (AssemblyResolutionException)
                    {
                        Console.WriteLine("Unable to instrument " + assemblyFile + " due to assembly resolution error");
                    }
                }
                
                // Execute the command.
                Console.WriteLine("Executing command");
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = commandArgs,
                    WorkingDirectory = workDir
                });
                process.WaitForExit();
            }
            finally
            {
                // Restore assemblies.
                foreach (var assemblyFile in extra)
                {
                    if (!File.Exists(assemblyFile + ".bak"))
                        continue;
                    Console.WriteLine("Restoring " + assemblyFile);
                    File.Delete(assemblyFile);
                    File.Move(assemblyFile + ".bak", assemblyFile);
                    try
                    {
                        if (File.Exists(assemblyFile + ".bak.mdb"))
                            File.Move(assemblyFile + ".bak.mdb", assemblyFile + ".mdb");
                    }
                    catch (IOException) { }
                }
            }
            
            // Generate report.
            using (var reader = new StreamReader(trackTemp))
            {
                using (var writer = XmlWriter.Create(output, new XmlWriterSettings { Indent = true }))
                {
                    writer.WriteStartElement("report");
                    writer.WriteStartElement("total");
                    writer.WriteString(total.ToString());
                    writer.WriteEndElement();
                    foreach (var instrument in instrumented)
                    {
                        writer.WriteStartElement("instrumented");
                        writer.WriteAttributeString("start", instrument.StartLine.ToString());
                        writer.WriteAttributeString("end", instrument.EndLine.ToString());
                        writer.WriteAttributeString("file", instrument.Document);
                        writer.WriteEndElement();
                    }
                    var unique = new List<string>();
                    while (!reader.EndOfStream)
                    {
                        var entry = reader.ReadLine().Split(new[] { ' ' }, 3);
                        var hash = entry[2] + ":" + entry[0] + "-" + entry[1];
                        if (unique.Contains(hash))
                            continue;
                        unique.Add(hash);
                        writer.WriteStartElement("executed");
                        writer.WriteAttributeString("start", entry[0]);
                        writer.WriteAttributeString("end", entry[1]);
                        writer.WriteAttributeString("file", entry[2]);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
            }
            File.Delete(trackTemp);
            Console.WriteLine("Report saved to " + output);
        }
        
        private struct ReportLine
        {
            public int StartLine { get; set; }
            public int EndLine { get; set; }
            public string Document { get; set; }
        }

        public static void ShowHelp (OptionSet p)
        {
            Console.WriteLine("Usage: cscover [OPTIONS]+ assembly1..assemblyN");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
