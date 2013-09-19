using System;
using Ninject;
using cslib;
using Mono.Cecil;
using System.Reflection;

namespace cscover
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var kernel = new StandardKernel();
            kernel.Load<CSharpLibraryNinjectModule>();
            
            var instrumenter = kernel.Get<IInstrumenter>();
            var assembly = AssemblyDefinition.ReadAssembly(args[0], new ReaderParameters { ReadSymbols = true });
            instrumenter.InstrumentAssembly(
                assembly,
                typeof(InstrumentationEndpoint).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Static));
            assembly.Write("Instrumented." + args[0]);
        }
    }
}