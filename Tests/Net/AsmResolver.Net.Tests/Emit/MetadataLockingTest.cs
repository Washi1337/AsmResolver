using System;
using System.Linq;
using AsmResolver.Net;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Emit
{
    public class MetadataLockingTest
    {
        [Fact]
        public void NonExistentStreams()
        {
            var assembly = NetAssemblyFactory.CreateAssembly("SomeAssembly", true);
            var header = assembly.NetDirectory.MetadataHeader;
            
            // Remove #US Stream.
            var usHeader = header.StreamHeaders.FirstOrDefault(x => x.Name == "#US");
            if (usHeader != null)
                header.StreamHeaders.Remove(usHeader);

            // Lock metadata.
            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            // Create new method with a string.
            var method = new MethodDefinition("Test",
                MethodAttributes.Public | MethodAttributes.Static,
                new MethodSignature(image.TypeSystem.Void));

            method.CilMethodBody = new CilMethodBody(method)
            {
                Instructions =
                {
                    CilInstruction.Create(CilOpCodes.Ldstr, "Hello, world!"),
                    CilInstruction.Create(CilOpCodes.Call, importer.ImportMethod(
                        typeof(Console).GetMethod("WriteLine", new[] {typeof(string)}))),
                    CilInstruction.Create(CilOpCodes.Ret)
                }
            };
            image.Assembly.Modules[0].TopLevelTypes[0].Methods.Add(method);

            // Commit changes.
            header.UnlockMetadata();

            // Check if #US stream is added again.
            var usStream = header.GetStream<UserStringStream>();
            Assert.NotNull(usStream);
        }
    }
}