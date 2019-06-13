using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Emit
{
    public class MetadataBuilderTest
    {
        private sealed class CustomBuilder : DefaultMetadataBuilder
        {
            private readonly IDictionary<BlobSignature, uint> _extraBlobs;

            public CustomBuilder(IEnumerable<BlobSignature> extraBlobs)
            {
                _extraBlobs = extraBlobs.ToDictionary(x => x, _ => 0u);
            }

            public uint GetBlobOffset(BlobSignature signature)
            {
                return _extraBlobs[signature];
            }

            public override MetadataBuffer Rebuild(MetadataImage image)
            {
                var buffer = base.Rebuild(image);

                foreach (var blob in _extraBlobs.Keys.ToArray())
                    _extraBlobs[blob] = buffer.BlobStreamBuffer.GetBlobOffset(blob);   
                
                return buffer;
            }
        }

        [Fact]
        public void CustomBlobs()
        {
            var assembly = NetAssemblyFactory.CreateAssembly("SomeAssembly", false);
            var header = assembly.NetDirectory.MetadataHeader;
            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);
            var writeLine = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] {typeof(string)}));

            var main = new MethodDefinition("Main", MethodAttributes.Public | MethodAttributes.Static,
                new MethodSignature(image.TypeSystem.Void));

            main.CilMethodBody = new CilMethodBody(main);
            main.CilMethodBody.Instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldstr, "Hello, world!"),
                CilInstruction.Create(CilOpCodes.Call, writeLine),
                CilInstruction.Create(CilOpCodes.Ret)
            });

            image.Assembly.Modules[0].TopLevelTypes[0].Methods.Add(main);
            
            image.ManagedEntrypoint = main;

            var extraBlobs = new BlobSignature[]
            {
                new DataBlobSignature(new byte[] {1, 2, 3}),
                new DataBlobSignature(new byte[] {4, 5, 6})
            };

            // Commit changes to assembly
            var builder = new CustomBuilder(extraBlobs);
            image.Header.UnlockMetadata(builder);

            var blobStream = header.GetStream<BlobStream>();
            Assert.Equal(new byte[] {1, 2, 3}, blobStream.GetBlobByOffset(builder.GetBlobOffset(extraBlobs[0])));
            Assert.Equal(new byte[] {4, 5, 6}, blobStream.GetBlobByOffset(builder.GetBlobOffset(extraBlobs[1])));
        }
    }
}