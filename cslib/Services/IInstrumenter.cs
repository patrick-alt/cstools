using Mono.Cecil;
using System.Reflection;

namespace cslib
{
    public interface IInstrumenter
    {
        void InstrumentAssembly(AssemblyDefinition assembly, MethodInfo endpoint, string output);
    }
}

