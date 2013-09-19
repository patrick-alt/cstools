using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using System.Collections.Generic;

namespace cslib
{
    public class DefaultInstrumenter : IInstrumenter
    {
        public int InstrumentAssembly(AssemblyDefinition assembly, MethodInfo endpoint, string output, Action<int, int, string> instructionInstrumented)
        {
            var i = 0;
            foreach (var module in assembly.Modules)
                i += this.InstrumentModule(module, endpoint, output, instructionInstrumented);
            return i;
        }

        private int InstrumentModule(ModuleDefinition module, MethodInfo endpoint, string output, Action<int, int, string> instructionInstrumented)
        {
            var i = 0;
            foreach (var type in module.Types)
                i += this.InstrumentType(type, endpoint, output, instructionInstrumented);
            return i;
        }

        private int InstrumentType(TypeDefinition type, MethodInfo endpoint, string output, Action<int, int, string> instructionInstrumented)
        {
            var i = 0;
            foreach (var method in type.Methods)
                i += this.InstrumentMethod(method, endpoint, output, instructionInstrumented);
            return i;
        }

        private int InstrumentMethod(MethodDefinition method, MethodInfo endpoint, string output, Action<int, int, string> instructionInstrumented)
        {
            if (!method.HasBody)
                return 0;
            var processor = method.Body.GetILProcessor();
            var importedEndpoint = method.Module.Import(endpoint);
            method.Body.MaxStackSize += 4;
            
            var i = 0;
            var unique = new List<string>();
            foreach (var instr in method.Body.Instructions.ToArray())
            {
                if (instr.SequencePoint != null &&
                    instr.SequencePoint.Document.Url != null &&
                    method.FullName != null)
                {
                    var insertOutput = processor.Create(OpCodes.Ldstr, output);
                    var insertMethodName = processor.Create(OpCodes.Ldstr, method.FullName);
                    var insertDocument = processor.Create(OpCodes.Ldstr, instr.SequencePoint.Document.Url);
                    var insertStartLine = processor.Create(OpCodes.Ldstr, instr.SequencePoint.StartLine.ToString());
                    var insertEndLine = processor.Create(OpCodes.Ldstr, instr.SequencePoint.EndLine.ToString());
                    var callEndpoint = processor.Create(OpCodes.Call, importedEndpoint);
                    processor.InsertBefore(instr, insertOutput);
                    processor.InsertAfter(insertOutput, insertMethodName);
                    processor.InsertAfter(insertMethodName, insertDocument);
                    processor.InsertAfter(insertDocument, insertStartLine);
                    processor.InsertAfter(insertStartLine, insertEndLine);
                    processor.InsertAfter(insertEndLine, callEndpoint);
                    
                    var hash = instr.SequencePoint.Document.Url + ":" + instr.SequencePoint.StartLine + "-" + instr.SequencePoint.EndLine;
                    if (!unique.Contains(hash))
                    {
                        unique.Add(hash);
                        instructionInstrumented(instr.SequencePoint.StartLine, instr.SequencePoint.EndLine, instr.SequencePoint.Document.Url);
                        i++;
                    }
                }
            }
            return i;
        }
    }
}

