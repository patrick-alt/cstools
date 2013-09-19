using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace cslib
{
    public class DefaultInstrumenter : IInstrumenter
    {
        public void InstrumentAssembly(AssemblyDefinition assembly, MethodInfo endpoint)
        {
            foreach (var module in assembly.Modules)
                this.InstrumentModule(module, endpoint);
        }

        private void InstrumentModule(ModuleDefinition module, MethodInfo endpoint)
        {
            foreach (var type in module.Types)
                this.InstrumentType(type, endpoint);
        }

        private void InstrumentType(TypeDefinition type, MethodInfo endpoint)
        {
            foreach (var method in type.Methods)
                this.InstrumentMethod(method, endpoint);
        }

        private void InstrumentMethod(MethodDefinition method, MethodInfo endpoint)
        {
            if (!method.HasBody)
                return;
            var processor = method.Body.GetILProcessor();
            var importedEndpoint = method.Module.Import(endpoint);
            method.Body.MaxStackSize += 4;
            
            foreach (var instr in method.Body.Instructions.ToArray())
            {
                if (instr.SequencePoint != null &&
                    instr.SequencePoint.Document.Url != null &&
                    method.FullName != null)
                {
                    var insertMethodName = processor.Create(OpCodes.Ldstr, method.FullName);
                    var insertDocument = processor.Create(OpCodes.Ldstr, instr.SequencePoint.Document.Url);
                    var insertStartLine = processor.Create(OpCodes.Ldstr, instr.SequencePoint.StartLine.ToString());
                    var insertEndLine = processor.Create(OpCodes.Ldstr, instr.SequencePoint.EndLine.ToString());
                    var callEndpoint = processor.Create(OpCodes.Call, importedEndpoint);
                    processor.InsertBefore(instr, insertMethodName);
                    processor.InsertAfter(insertMethodName, insertDocument);
                    processor.InsertAfter(insertDocument, insertStartLine);
                    processor.InsertAfter(insertStartLine, insertEndLine);
                    processor.InsertAfter(insertEndLine, callEndpoint);
                }
            }
        }
    }
}

