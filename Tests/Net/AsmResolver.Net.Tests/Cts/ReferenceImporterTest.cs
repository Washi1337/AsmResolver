using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class ReferenceImporterTests
    {
        private const string DummyAssemblyName = "SomeAssembly";
        private readonly SignatureComparer _comparer = new SignatureComparer();
        private string ExternalAssemblyName = DummyAssemblyName + "2";


        private void VerifyImportedReference(MetadataImage image, MemberReference reference, MemberReference newReference)
        {
            Assert.NotSame(reference, newReference);
            Assert.Equal(reference, newReference, _comparer);
            Assert.Equal(image, newReference.Image);
        }

        #region Assembly

        [Fact]
        public void ImportNewAssembly()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            const string assemblyName = "some_lib";
            var version = new Version(1, 2, 3, 4);

            var reference = new AssemblyReference(assemblyName, version);
            var newReference = importer.ImportAssembly(new AssemblyReference(assemblyName, version));

            Assert.NotSame(reference, newReference);
            Assert.Contains(newReference, image.Assembly.AssemblyReferences);
            Assert.Equal(reference, newReference, _comparer);
        }

        [Fact]
        public void ImportExistingAssembly()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            const string assemblyName = "some_lib";
            var version = new Version(1, 2, 3, 4);

            var reference = new AssemblyReference(assemblyName, version);
            image.Assembly.AssemblyReferences.Add(reference);

            var newReference = importer.ImportAssembly(new AssemblyReference(assemblyName, version));
            Assert.Equal(reference, newReference, _comparer);
        }

        [Fact]
        public void ImportReflectionAssembly()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var assemblyName = typeof(Form).Assembly.GetName();

            var wrapper = new ReflectionAssemblyNameWrapper(assemblyName);
            var newReference = importer.ImportAssembly(assemblyName);
            Assert.Contains(newReference, image.Assembly.AssemblyReferences);
            Assert.True(_comparer.Equals(wrapper, newReference));
        }

        #endregion

        #region Types

        #region Type reference
        [Fact]
        public void ImportNewTypeReference()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            const string typeNamespace = "System.Windows.Forms";
            const string typeName = "Form";

            var assemblyDescr = new ReflectionAssemblyNameWrapper(typeof(Form).Assembly.GetName());
            var reference = new TypeReference(importer.ImportAssembly(assemblyDescr), typeNamespace, typeName);

            var newReference = importer.ImportType(reference);

            Assert.NotSame(reference, newReference);
            Assert.Equal(image, newReference.Image);
            Assert.Equal(reference, newReference, _comparer);
            Assert.Contains(image.Assembly.AssemblyReferences, x => _comparer.Equals(x, assemblyDescr));
        }

        #endregion

        #region Reflection

        [Fact]
        public void ImportReflectionType()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var assemblyDescr = new ReflectionAssemblyNameWrapper(typeof(Form).Assembly.GetName());

            var expected = new TypeReference(new AssemblyReference(assemblyDescr), typeof(Form).Namespace, typeof(Form).Name);
            var newReference = importer.ImportType(typeof(Form));

            Assert.Equal(expected, newReference, _comparer);
            Assert.Equal(image, newReference.Image);
            Assert.Contains(image.Assembly.AssemblyReferences, x => _comparer.Equals(x, assemblyDescr));
        }

        #endregion

        #region Type definition

        [Fact]
        public void ImportInternalTypeDefinition()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var typeDef = new TypeDefinition("SomeNamespace", "SomeName");
            image.Assembly.Modules[0].Types.Add(typeDef);

            var newReference = importer.ImportType(typeDef);

            Assert.Same(typeDef, newReference);
        }

        [Fact]
        public void ImportExternalTypeDefinition()
        {
            var externalAssembly = NetAssemblyFactory.CreateAssembly(ExternalAssemblyName, true);
            var externalImage = externalAssembly.NetDirectory.MetadataHeader.LockMetadata();
            var externalType = new TypeDefinition("SomeNamespace", "SomeName");
            externalImage.Assembly.Modules[0].Types.Add(externalType);

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var newReference = importer.ImportType(externalType);

            Assert.NotSame(externalType, newReference);
            Assert.Equal(externalType, newReference, _comparer);
            Assert.Equal(image, newReference.Image);
            Assert.Contains(image.Assembly.AssemblyReferences, x => _comparer.Equals(x, externalImage.Assembly));
        }

        #endregion

        #region Type signature

        private static TypeDefOrRefSignature CreateTypeDefOrRef(Type type)
        {
            TypeReference typeRef = CreateTypeReference(type);
            return new TypeDefOrRefSignature(typeRef);
        }

        private static TypeReference CreateTypeReference(Type type)
        {
            return new TypeReference(
                CreateResolutionScope(type),
                type.Namespace,
                type.Name);
        }

        private static AssemblyReference CreateResolutionScope(Type type)
        {
            return new AssemblyReference(new ReflectionAssemblyNameWrapper(type.Assembly.GetName()));
        }

        [Fact]
        public void ImportTypeDefOrRefSignature()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var signature = CreateTypeDefOrRef(typeof(Form));

            var newSignature = importer.ImportTypeSignature(signature);

            Assert.NotSame(signature, newSignature);
            Assert.Equal(signature, newSignature, _comparer);
        }

        private TSignature TestTypeSpecification<TSignature>(TSignature signature, ITypeDefOrRef baseType)
            where TSignature : TypeSpecificationSignature
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var newSignature = importer.ImportTypeSignature(signature);

            Assert.IsType<TSignature>(newSignature);
            Assert.NotSame(signature, newSignature);
            Assert.Equal(signature, newSignature, _comparer);
            
            var elementType = newSignature.GetElementType();
            Assert.IsAssignableFrom<ITypeDefOrRef>(elementType);
            Assert.Equal(baseType, elementType, _comparer);
            Assert.Equal(image, ((ITypeDefOrRef) elementType).Image);

            return (TSignature)newSignature;
        }

        [Fact]
        public void ImportCorlibType()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var signature = image.TypeSystem.Byte;
            var newSignature = importer.ImportTypeSignature(signature);
            Assert.Same(signature, newSignature);
        }

        [Fact]
        public void ImportArrayType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            var arrayType = new ArrayTypeSignature(baseType);

            arrayType.Dimensions.Add(new ArrayDimension(null, 0));
            arrayType.Dimensions.Add(new ArrayDimension(null, 1));

            var newType = TestTypeSpecification(arrayType, baseType.Type);

            Assert.Equal(arrayType.Dimensions.Count, newType.Dimensions.Count);

            for (int i = 0; i < arrayType.Dimensions.Count; i++)
            {
                Assert.Equal(arrayType.Dimensions[i].Size, newType.Dimensions[i].Size);
                Assert.Equal(arrayType.Dimensions[i].LowerBound, newType.Dimensions[i].LowerBound);
            }
        }

        [Fact]
        public void ImportBoxedType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            TestTypeSpecification(new BoxedTypeSignature(baseType), baseType.Type);
        }

        [Fact]
        public void ImportByReferenceType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            TestTypeSpecification(new ByReferenceTypeSignature(baseType), baseType.Type);
        }

        [Fact]
        public void ImportOptionalModifierType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            var modifierType = CreateTypeReference(typeof(IsVolatile));
            var modOptType = new OptionalModifierSignature(modifierType, baseType);
            var newType = TestTypeSpecification(modOptType, baseType.Type);

            Assert.NotSame(modifierType, newType.ModifierType);
        }

        [Fact]
        public void ImportPinnedType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            TestTypeSpecification(new PinnedTypeSignature(baseType), baseType.Type);
        }

        [Fact]
        public void ImportPointerType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            TestTypeSpecification(new PointerTypeSignature(baseType), baseType.Type);
        }

        [Fact]
        public void ImportRequiredModifierType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            var modifierType = CreateTypeReference(typeof(IsVolatile));
            var modOptType = new RequiredModifierSignature(modifierType, baseType);
            var newType = TestTypeSpecification(modOptType, baseType.Type);

            Assert.NotSame(modifierType, newType.ModifierType);
        }

        [Fact]
        public void ImportSentinelType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            TestTypeSpecification(new SentinelTypeSignature(baseType), baseType.Type);
        }

        [Fact]
        public void ImportSzArrayType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            TestTypeSpecification(new SzArrayTypeSignature(baseType), baseType.Type);
        }

        [Fact]
        public void ImportGenericInstanceType()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var signature = new GenericInstanceTypeSignature(CreateTypeReference(typeof(List<>)));
            var genericArg = CreateTypeDefOrRef(typeof(Form));
            signature.GenericArguments.Add(genericArg);

            var newSignature = importer.ImportTypeSignature(signature);

            Assert.NotSame(signature, newSignature);
            Assert.Equal(signature, newSignature, _comparer);

            var newGenericSiganture = (GenericInstanceTypeSignature) newSignature;
            Assert.Equal(image, newGenericSiganture.GenericType.Image);

            var genericArgElementType = newGenericSiganture.GenericArguments[0].GetElementType();
            Assert.IsAssignableFrom<ITypeDefOrRef>(genericArgElementType);
            Assert.Equal(image, ((ITypeDefOrRef) genericArgElementType).Image);
        }

        #endregion

        #region Type specification

        [Fact]
        public void ImportTypeSpecification()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var typeSpec = new TypeSpecification(CreateTypeDefOrRef(typeof(Form)));

            var newSpec = importer.ImportType(typeSpec);

            Assert.NotSame(typeSpec, newSpec);
            Assert.Equal(typeSpec, newSpec, _comparer);
        }

        #endregion

        #endregion

        #region Methods

        [Fact]
        public void ImportNewMethodReference()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var reference = new MemberReference(
                CreateTypeReference(typeof(Console)),
                "WriteLine",
                new MethodSignature(new[] { image.TypeSystem.String }, image.TypeSystem.Void));

            VerifyImportedReference(image, reference, importer.ImportMember(reference));
        }

        [Fact]
        public void ImportReflectionMethod()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var reference = new MemberReference(
                CreateTypeReference(typeof(Console)),
                "WriteLine",
                new MethodSignature(new[] { image.TypeSystem.String }, image.TypeSystem.Void));

            var newReference = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));

            VerifyImportedReference(image, reference, newReference);
        }

        [Fact]
        public void ImportInternalMethodDefinition()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var type = new TypeDefinition("SomeNamespace", "SomeType");
            image.Assembly.Modules[0].Types.Add(type);

            var method = new MethodDefinition("SomeMethod", MethodAttributes.Public | MethodAttributes.Static,
                new MethodSignature(new[] { image.TypeSystem.String }, image.TypeSystem.Void));

            type.Methods.Add(method);

            var newReference = importer.ImportMethod(method);
            Assert.Same(method, newReference);
        }

        [Fact]
        public void ImportExternalMethodDefinition()
        {
            var externalAssembly = NetAssemblyFactory.CreateAssembly(ExternalAssemblyName, true);
            var externalImage = externalAssembly.NetDirectory.MetadataHeader.LockMetadata();

            var type = new TypeDefinition("SomeNamespace", "SomeType");
            externalImage.Assembly.Modules[0].Types.Add(type);

            var method = new MethodDefinition("SomeMethod", MethodAttributes.Public | MethodAttributes.Static,
                new MethodSignature(new[] { externalImage.TypeSystem.String }, externalImage.TypeSystem.Void));

            type.Methods.Add(method);

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var newReference = importer.ImportMethod(method) as MemberReference;

            Assert.NotSame(method, newReference);
            Assert.Equal((IMemberReference) method, newReference, _comparer);
        }

        #endregion

        #region Fields

        [Fact]
        public void ImportNewFieldReference()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var reference = new MemberReference(
                CreateTypeReference(typeof(string)),
                "Empty",
                new FieldSignature(image.TypeSystem.String));

            VerifyImportedReference(image, reference, importer.ImportMember(reference));
        }

        [Fact]
        public void ImportReflectionField()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);
            
            var reference = new MemberReference(
                CreateTypeReference(typeof(string)),
                "Empty",
                new FieldSignature(image.TypeSystem.String));

            var newReference = importer.ImportField(typeof(string).GetField("Empty", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));
            VerifyImportedReference(image, reference, newReference);
        }


        [Fact]
        public void ImportInternalFieldDefinition()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var type = new TypeDefinition("SomeNamespace", "SomeType");
            image.Assembly.Modules[0].Types.Add(type);

            var field = new FieldDefinition("SomeField", FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(image.TypeSystem.String));

            type.Fields.Add(field);

            var newReference = importer.ImportField(field);

            Assert.Same(field, newReference);
        }

        [Fact]
        public void ImportExternalFieldDefinition()
        {
            var externalAssembly = NetAssemblyFactory.CreateAssembly(ExternalAssemblyName, true);
            var externalImage = externalAssembly.NetDirectory.MetadataHeader.LockMetadata();

            var type = new TypeDefinition("SomeNamespace", "SomeType");
            externalImage.Assembly.Modules[0].Types.Add(type);

            var field = new FieldDefinition("SomeField", FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(externalImage.TypeSystem.String));

            type.Fields.Add(field);

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            var importer = new ReferenceImporter(image);

            var newReference = importer.ImportField(field) as MemberReference;

            Assert.NotSame(field, newReference);
            Assert.Equal((IMemberReference) field, newReference, _comparer);
        }
        #endregion
    }
}