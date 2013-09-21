using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MethodInfo = System.Reflection.MethodInfo;

namespace cslib
{
    public class DefaultInstrumenter : IInstrumenter
    {
        private bool HasSkipInstrumentation(ICustomAttributeProvider type)
        {
            return type.CustomAttributes.Any(x => x.AttributeType.Name == "NoInstrumentationAttribute");
        }
        
        private class InstrumentationRecorder
        {
            private TypeDefinition m_RecorderDefinition;
            private ModuleDefinition m_MainModule;
            private string m_OutputPath;
            private SHA1Managed m_SHA1 = new SHA1Managed();
            private Dictionary<string, InstrumentationInfo> m_Mappings = new Dictionary<string, InstrumentationInfo>();
            
            private struct InstrumentationInfo
            {
                public string Filename;
                public int Start;
                public int End;
                public FieldDefinition FieldDef;
            }
            
            private string GetHash(string filename, int start, int end)
            {
                var hash = this.m_SHA1.ComputeHash(Encoding.ASCII.GetBytes(filename + ":" + start + "-" + end));
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
            
            public InstrumentationRecorder(AssemblyDefinition assembly, string output)
            {
                this.m_OutputPath = output;
                this.m_MainModule = assembly.MainModule;
                this.m_RecorderDefinition = new TypeDefinition(
                    "",
                    "InstrumentationRecorder",
                    TypeAttributes.Public | TypeAttributes.Class,
                    this.m_MainModule.Import(typeof(object)));
                if (this.m_MainModule.Types.Any(x => x.Name == "InstrumentationRecorder" && x.Namespace == ""))
                    throw new ApplicationException("this assembly is already instrumented");
                this.m_MainModule.Types.Add(this.m_RecorderDefinition);
            }

            public bool EmitRecord(ILProcessor processor, MethodDefinition method, Instruction instr)
            {
                var filename = instr.SequencePoint.Document.Url;
                var start = instr.SequencePoint.StartLine;
                var end = instr.SequencePoint.EndLine;
                var fieldName = "instrument_" + this.GetHash(filename, start, end);
                if (!this.m_RecorderDefinition.Fields.Any(x => x.Name == fieldName))
                {
                    // Create the field.
                    var fieldDef = new FieldDefinition(
                        fieldName,
                        FieldAttributes.Public | FieldAttributes.Static, 
                        this.m_MainModule.Import(typeof(bool)));
                    this.m_RecorderDefinition.Fields.Add(fieldDef);
                    this.m_Mappings.Add(fieldName, new InstrumentationInfo
                    {
                        Filename = filename,
                        Start = start,
                        End = end,
                        FieldDef = fieldDef
                    });
                }
                var field = this.m_RecorderDefinition.Fields.First(x => x.Name == fieldName);
                var insertBoolOp = processor.Create(OpCodes.Ldc_I4_1);
                var setFieldOp = processor.Create(OpCodes.Stsfld, field);
                processor.InsertBefore(instr, insertBoolOp);
                processor.InsertAfter(insertBoolOp, setFieldOp);
                return true;
            }
            
            public void Finalise()
            {
                // Add the static destructor.
                var destructorType = new TypeDefinition(
                    "",
                    "Finalizer",
                    TypeAttributes.Class | TypeAttributes.NestedPrivate | TypeAttributes.Sealed |
                    TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit,
                    this.m_MainModule.Import(typeof(object)));
                var destructor = new MethodDefinition(
                    "Finalize",
                    MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                    this.m_MainModule.Import(typeof(void)));
                destructor.Body.InitLocals = true;
                destructor.Body.Variables.Add(new VariableDefinition(
                    this.m_MainModule.Import(typeof(System.IO.StreamWriter))));
                var processor = destructor.Body.GetILProcessor();
                processor.Emit(OpCodes.Ldstr, this.m_OutputPath);
                processor.Emit(OpCodes.Ldc_I4_1);
                processor.Emit(
                    OpCodes.Newobj,
                    this.m_MainModule.Import(typeof(System.IO.StreamWriter).GetConstructor(new[] { typeof(string), typeof(bool) })));
                processor.Emit(OpCodes.Stloc_0);
                Instruction jumpFrom = null;
                foreach (var kv in this.m_Mappings)
                {
                    var next = processor.Create(OpCodes.Ldsfld, kv.Value.FieldDef);
                    processor.Append(next);
                    if (jumpFrom != null)
                    {
                        processor.InsertAfter(
                            jumpFrom,
                            processor.Create(OpCodes.Brfalse, next));
                    }
                    jumpFrom = next;
                    processor.Emit(OpCodes.Ldloc_0);
                    processor.Emit(OpCodes.Ldstr, kv.Value.Start + " " + kv.Value.End + " " + kv.Value.Filename);
                    processor.Emit(
                        OpCodes.Callvirt,
                        this.m_MainModule.Import(typeof(System.IO.TextWriter).GetMethod("WriteLine", new[] { typeof(string) })));
                }
                var nextOut = processor.Create(OpCodes.Ldloc_0);
                processor.Append(nextOut);
                if (jumpFrom != null)
                {
                    processor.InsertAfter(
                        jumpFrom,
                        processor.Create(OpCodes.Brfalse, nextOut));
                }
                processor.Emit(
                    OpCodes.Callvirt,
                    this.m_MainModule.Import(typeof(System.IO.TextWriter).GetMethod("Flush", Type.EmptyTypes)));
                processor.Emit(OpCodes.Ldloc_0);
                processor.Emit(
                    OpCodes.Callvirt,
                    this.m_MainModule.Import(typeof(System.IO.TextWriter).GetMethod("Close", Type.EmptyTypes)));
                processor.Emit(OpCodes.Ldarg_0);
                processor.Emit(
                    OpCodes.Call,
                    this.m_MainModule.Import(typeof(object).GetMethod("Finalize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)));
                processor.Emit(OpCodes.Ret);
                
                destructorType.Methods.Add(destructor);
                
                var destructorConstructor = new MethodDefinition(
                    ".ctor",
                    MethodAttributes.Public | MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    this.m_MainModule.Import(typeof(void)));
                var destructorConstructorProcessor = destructorConstructor.Body.GetILProcessor();
                destructorConstructorProcessor.Emit(OpCodes.Ldarg_0);
                destructorConstructorProcessor.Emit(
                    OpCodes.Call,
                    this.m_MainModule.Import(typeof(object).GetConstructor(Type.EmptyTypes)));
                destructorConstructorProcessor.Emit(OpCodes.Ret);
                destructorType.Methods.Add(destructorConstructor);
                
                var destructorFld = new FieldDefinition(
                    "_destructor",
                    FieldAttributes.Private | FieldAttributes.Static,
                    destructorType);
                this.m_RecorderDefinition.Fields.Add(destructorFld);
                this.m_RecorderDefinition.NestedTypes.Add(destructorType);
                var constructor = new MethodDefinition(
                    ".cctor",
                    MethodAttributes.Private | MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName | MethodAttributes.RTSpecialName |
                    MethodAttributes.Static,
                    this.m_MainModule.Import(typeof(void)));
                var constructorProcessor = constructor.Body.GetILProcessor();
                constructorProcessor.Emit(OpCodes.Newobj, destructorConstructor);
                constructorProcessor.Emit(OpCodes.Stsfld, destructorFld);
                constructorProcessor.Emit(OpCodes.Ret);
                this.m_RecorderDefinition.Methods.Add(constructor);
            }
        }
        
        public int InstrumentAssembly(AssemblyDefinition assembly, string output, Action<int, int, string> instructionInstrumented)
        {
            var recorder = new InstrumentationRecorder(assembly, output);
            var i = 0;
            foreach (var module in assembly.Modules)
                i += this.InstrumentModule(module, recorder, instructionInstrumented);
            recorder.Finalise();
            return i;
        }

        private int InstrumentModule(ModuleDefinition module, InstrumentationRecorder recorder, Action<int, int, string> instructionInstrumented)
        {
            var i = 0;
            foreach (var type in module.Types.Where(x => !HasSkipInstrumentation(x)))
                i += this.InstrumentType(type, recorder, instructionInstrumented);
            return i;
        }

        private int InstrumentType(TypeDefinition type, InstrumentationRecorder recorder, Action<int, int, string> instructionInstrumented)
        {
            var i = 0;
            foreach (var method in type.Methods.Where(x => !HasSkipInstrumentation(x)))
                i += this.InstrumentMethod(method, recorder, instructionInstrumented);
            return i;
        }

        private int InstrumentMethod(MethodDefinition method, InstrumentationRecorder recorder, Action<int, int, string> instructionInstrumented)
        {
            if (!method.HasBody)
                return 0;
            var processor = method.Body.GetILProcessor();
            method.Body.MaxStackSize += 4;
            
            var i = 0;
            var unique = new List<string>();
            foreach (var instr in method.Body.Instructions.ToArray())
            {
                if (instr.SequencePoint != null &&
                    instr.SequencePoint.Document.Url != null &&
                    method.FullName != null)
                {
                    if (recorder.EmitRecord(processor, method, instr))
                    {
                        var hash = instr.SequencePoint.Document.Url + ":" + instr.SequencePoint.StartLine + "-" + instr.SequencePoint.EndLine;
                        if (!unique.Contains(hash))
                        {
                            unique.Add(hash);
                            instructionInstrumented(instr.SequencePoint.StartLine, instr.SequencePoint.EndLine, instr.SequencePoint.Document.Url);
                            i++;
                        }
                    }
                }
            }
            return i;
        }
    }
}

