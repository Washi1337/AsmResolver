using System;
using AsmResolver.Net;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class MetadataImageTest
    {
        private const string DummyAssemblyName = "SomeAssembly";
        private readonly SignatureComparer _comparer = new SignatureComparer();
        
        [Fact]
        public void PersistentEntrypoint()
        {
            // Create new assembly.
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, false);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);
            var writeLine = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] {typeof(string)}));
            
            // Create and add main method.
            var main = new MethodDefinition("Main", 
                MethodAttributes.Public | MethodAttributes.Static,
                new MethodSignature(image.TypeSystem.Void));

            main.CilMethodBody = new CilMethodBody(main);
            main.CilMethodBody.Instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldstr, "Hello world!"),
                CilInstruction.Create(CilOpCodes.Call, writeLine),
                CilInstruction.Create(CilOpCodes.Ret),
            });
            image.Assembly.Modules[0].TopLevelTypes[0].Methods.Add(main);
            image.ManagedEntrypoint = main;
            
            // Commit.
            var mapping = image.Header.UnlockMetadata();
            
            // Test.
            Assert.Equal(mapping[main].ToUInt32(), assembly.NetDirectory.EntryPointToken);
            var newImage = assembly.NetDirectory.MetadataHeader.LockMetadata();
            Assert.Equal(main, newImage.ManagedEntrypoint, _comparer);
        }
    }
}