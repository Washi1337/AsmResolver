using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AsmResolver;
using AsmResolver.Net;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using AsmResolver.X86;

namespace SampleAsmResolver
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // Create new assembly.
            var assembly = NetAssemblyFactory.CreateAssembly("SomeAssembly", false);
            var header = assembly.NetDirectory.MetadataHeader;
            
            // Lock the metadata so that we can add and remove members safely.
            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);
            
            // Create a native method.
            var nativeMethod = CreateNativeMethod(image);
            image.Assembly.Modules[0].TopLevelTypes[0].Methods.Add(nativeMethod);
            
            // Create a new type.
            var type = new TypeDefinition("SomeNamespace", "SomeType", importer.ImportType(typeof(object)));
            type.Attributes = TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed;
            image.Assembly.Modules[0].TopLevelTypes.Add(type);
            
            // Create a new main method.
            var mainMethod = CreateMainMethod(image, importer, nativeMethod);
            type.Methods.Add(mainMethod);
            image.ManagedEntrypoint = mainMethod;
            
            // Commit our changes.
            header.UnlockMetadata();
            
            // Save to disk!
            assembly.NetDirectory.Flags &= ~ImageNetDirectoryFlags.IlOnly; // Required for mixed mode apps.
            string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "output.exe");
            assembly.Write(outputPath, new CompactNetAssemblyBuilder(assembly));

        }

        private static MethodDefinition CreateMainMethod(
            MetadataImage image, 
            IReferenceImporter importer,
            IMemberReference getSecretNumberMethod)
        {
            var mainMethod = new MethodDefinition("Main", MethodAttributes.Public | MethodAttributes.Static,
                new MethodSignature(image.TypeSystem.Void));

            var cilBody = new CilMethodBody(mainMethod);
            var writeLine = importer.ImportMethod(
                typeof(Console).GetMethod("WriteLine", new[] {typeof(string), typeof(object)}));
            cilBody.Instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldstr, "The secret number is: {0}"),
                CilInstruction.Create(CilOpCodes.Call, getSecretNumberMethod),
                CilInstruction.Create(CilOpCodes.Box, importer.ImportType(typeof(int))),
                CilInstruction.Create(CilOpCodes.Call, writeLine),
                CilInstruction.Create(CilOpCodes.Ret)
            });
            mainMethod.MethodBody = cilBody;
            return mainMethod;
        }

        private static MethodDefinition CreateNativeMethod(MetadataImage image)
        {
            var nativeMethod = new MethodDefinition("MyNativeMethod",
                MethodAttributes.Assembly | MethodAttributes.Static | MethodAttributes.PInvokeImpl,
                new MethodSignature(image.TypeSystem.Int32));

            nativeMethod.ImplAttributes = MethodImplAttributes.Native
                                          | MethodImplAttributes.Unmanaged
                                          | MethodImplAttributes.PreserveSig;

            var nativeBody = new X86MethodBody();

            nativeBody.Instructions.Add(new X86Instruction
            {
                Mnemonic = X86Mnemonic.Mov,
                OpCode = X86OpCodes.Mov_Eax_Imm1632,
                Operand1 = new X86Operand(X86Register.Eax),
                Operand2 = new X86Operand(1337),
            });

            nativeBody.Instructions.Add(new X86Instruction
            {
                Mnemonic = X86Mnemonic.Retn,
                OpCode = X86OpCodes.Retn,
            });

            nativeMethod.MethodBody = nativeBody;
            return nativeMethod;
        }
    }
}