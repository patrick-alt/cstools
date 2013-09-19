using Mono.Cecil;
using System.Reflection;
using System;

namespace cslib
{
    public interface IInstrumenter
    {
        /// <summary>
        /// Instruments the assembly.
        /// </summary>
        /// <returns>The number of instructions instrumented.</returns>
        int InstrumentAssembly(AssemblyDefinition assembly, MethodInfo endpoint, string output, Action<int, int, string> instructionInstrumented);
    }
}

