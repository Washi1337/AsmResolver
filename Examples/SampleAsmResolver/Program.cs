using System;
using System.IO;
using AsmResolver;
using AsmResolver.Net;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace SampleAsmResolver
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var assembly = NetAssemblyFactory.CreateAssembly("SomeAssembly", false);
            var header = assembly.NetDirectory.MetadataHeader;
            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            var mainMethod = new MethodDefinition("Main", MethodAttributes.Public | MethodAttributes.Static,
                new MethodSignature(image.TypeSystem.Void));

            image.Assembly.Modules[0].Types[0].Methods.Add(mainMethod);

            mainMethod.MethodBody = new CilMethodBody(mainMethod);
            var instructions = mainMethod.CilMethodBody.Instructions;
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldstr, "Hello, world!"));
            instructions.Add(CilInstruction.Create(CilOpCodes.Call,
                importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] {typeof(string)}))));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ret));

            var mapping = header.UnlockMetadata();

            assembly.NetDirectory.EntryPointToken = mapping[mainMethod].ToUInt32();

            string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "output.exe");

            var builder = new CompactNetAssemblyBuilder(assembly);
            builder.Build();
            using (var stream = File.Create(outputPath))
            {
                builder.Write(new WritingContext(assembly, new BinaryStreamWriter(stream)));
            }

        }
    }
}