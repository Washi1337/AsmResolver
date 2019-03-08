using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.Net;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class TypeSpecificationTest
    {
        private static readonly SignatureComparer Comparer = new SignatureComparer();
            
        private TypeSpecification CreateDummyType()
        {
            var assembly = NetAssemblyFactory.CreateAssembly("SomeAssembly", true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var typeSpec = (TypeSpecification) importer.ImportType(new TypeSpecification(image.TypeSystem.Void));
            
            var method = new MethodDefinition("SomeMethod", 
                MethodAttributes.Public | MethodAttributes.Static,
                new MethodSignature(image.TypeSystem.Void));
            method.CilMethodBody = new CilMethodBody(method)
            {
                Instructions =
                {
                    CilInstruction.Create(CilOpCodes.Ldtoken, typeSpec),
                    CilInstruction.Create(CilOpCodes.Pop),
                    CilInstruction.Create(CilOpCodes.Ret),
                }
            };
            image.Assembly.Modules[0].TopLevelTypes[0].Methods.Add(method);

            return typeSpec;
        }
        
        [Fact]
        public void SingleArgGenericTypeInstance()
        {
            var spec = CreateDummyType();
            var image = spec.Image;
            var importer = new ReferenceImporter(image);            
            
            var signature = new GenericInstanceTypeSignature(importer.ImportType(typeof(List<>)), image.TypeSystem.String);
            spec.Signature = signature;
            
            var header = image.Header;
            var mapping = header.UnlockMetadata();

            var newImage = header.LockMetadata();
            var newSpec = (TypeSpecification) newImage.ResolveMember(mapping[spec]);
            
            Assert.Equal(signature, newSpec.Signature, Comparer);
        }
        
        [Fact]
        public void ComplexGenericTypeInstance()
        {
            var spec = CreateDummyType();
            var image = spec.Image;
            var importer = new ReferenceImporter(image);

            // Tuple<Tuple<decimal, decimal>, Tuple<decimal, decimal>>
            var signature =
                new GenericInstanceTypeSignature(importer.ImportType(typeof(Tuple<,>)),
                    new GenericInstanceTypeSignature(
                        importer.ImportType(typeof(Tuple<,>)),
                        importer.ImportTypeSignature(typeof(decimal)),
                        importer.ImportTypeSignature(typeof(decimal))),
                    new GenericInstanceTypeSignature(
                        importer.ImportType(typeof(Tuple<,>)),
                        importer.ImportTypeSignature(typeof(decimal)),
                        importer.ImportTypeSignature(typeof(decimal))));
            
            spec.Signature = signature;
            
            var header = image.Header;
            var mapping = header.UnlockMetadata();

            var newImage = header.LockMetadata();
            var newSpec = (TypeSpecification) newImage.ResolveMember(mapping[spec]);
            
            Assert.Equal(signature, newSpec.Signature, Comparer);
        }
        
        [Fact]
        public void MaliciousMetadataLoop()
        {
            var spec = CreateDummyType();
            var image = spec.Image;
            var importer = new ReferenceImporter(image);

            spec.Signature = new MaliciousTypeSignature(
                importer.ImportType(typeof(object)),
                spec);
            
            var header = image.Header;
            var mapping = header.UnlockMetadata();

            var newImage = header.LockMetadata();
            var newSpec = (TypeSpecification) newImage.ResolveMember(mapping[spec]);

            Assert.Equal(
                InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.MetadataLoop),
                newSpec.Signature.GetElementType(), 
                Comparer);
        }
        
        [Fact]
        public void MaliciousMetadataLoop2()
        {
            var spec = CreateDummyType();
            var image = spec.Image;
            var importer = new ReferenceImporter(image);

            spec.Signature =
                new GenericInstanceTypeSignature(importer.ImportType(typeof(Tuple<,>)),
                    new MaliciousTypeSignature(
                        importer.ImportType(typeof(object)),
                        spec),
                    importer.ImportTypeSignature(typeof(decimal)));
            
            var header = image.Header;
            var mapping = header.UnlockMetadata();

            var newImage = header.LockMetadata();
            var newSpec = (TypeSpecification) newImage.ResolveMember(mapping[spec]);

            Assert.NotEqual(spec.Signature, newSpec.Signature, Comparer);
        }

        private sealed class MaliciousTypeSignature : TypeDefOrRefSignature
        {
            private readonly ITypeDefOrRef _actual;

            public MaliciousTypeSignature(ITypeDefOrRef dummyType, ITypeDefOrRef actual) 
                : base(dummyType)
            {
                _actual = actual;
            }

            public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
            {
                writer.WriteByte((byte) ElementType);
                WriteTypeDefOrRef(buffer, writer, _actual);
            }
        } 
    }
}