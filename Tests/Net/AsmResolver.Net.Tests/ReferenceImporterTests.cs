using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AsmResolver.Net;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsmResolver.Tests.Net
{
    [TestClass]
    public class ReferenceImporterTests
    {
        private static SignatureComparer _comparer;

        [ClassInitialize]
        public static void InitializeClass(TestContext context)
        {
            _comparer = new SignatureComparer();
        }

        private static void VerifyImportedReference(TableStream tableStream, MemberReference reference, MemberReference newReference)
        {
            Assert.AreNotSame(reference, newReference,
                "Imported reference is the same as original.");

            Assert.IsTrue(_comparer.MatchMembers(reference, newReference),
                "Imported reference does not match original.");

            Assert.IsTrue(tableStream.GetTable<MemberReference>().Contains(newReference),
                "Imported reference not added to table.");

            Assert.IsTrue(tableStream.GetTable<TypeReference>().Contains(newReference.DeclaringType),
                "Imported reference's declaring type not added to table.");
        }

        #region Assembly

        [TestMethod]
        public void ImportNewAssembly()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var refTable = tableStream.GetTable<AssemblyReference>();
            var importer = new ReferenceImporter(tableStream);

            const string assemblyName = "some_lib";
            var version = new Version(1, 2, 3, 4);

            var reference = new AssemblyReference(assemblyName, version);
            var newReference = importer.ImportAssembly(new AssemblyReference(assemblyName, version));

            Assert.AreNotSame(reference, newReference, "Imported reference is same object as original.");
            Assert.IsTrue(refTable.Contains(newReference), "Imported reference not added to reference table.");
            Assert.IsTrue(_comparer.MatchAssemblies(reference, newReference), "Imported reference does not match original.");
        }

        [TestMethod]
        public void ImportExistingAssembly()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var importer = new ReferenceImporter(tableStream);
            var refTable = tableStream.GetTable<AssemblyReference>();

            const string assemblyName = "some_lib";
            var version = new Version(1, 2, 3, 4);

            var reference = new AssemblyReference(assemblyName, version);
            refTable.Add(reference);

            var newReference = importer.ImportAssembly(new AssemblyReference(assemblyName, version));
            Assert.AreSame(reference, newReference,
                "Importered reference is not the same object as original.");
        }

        [TestMethod]
        public void ImportReflectionAssembly()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var importer = new ReferenceImporter(tableStream);
            var refTable = tableStream.GetTable<AssemblyReference>();

            var assemblyName = typeof(Form).Assembly.GetName();

            var wrapper = new ReflectionAssemblyNameWrapper(assemblyName);
            var newReference = importer.ImportAssembly(assemblyName);
            Assert.IsTrue(refTable.Contains(newReference), "Imported reference not added to reference table.");
            Assert.IsTrue(_comparer.MatchAssemblies(wrapper, newReference), "Imported reference does not match original.");
        }

        #endregion

        #region Types

        #region Type reference
        [TestMethod]
        public void ImportNewTypeReference()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var importer = new ReferenceImporter(tableStream);
            var typeRefTable = tableStream.GetTable<TypeReference>();
            var assemblyRefTable = tableStream.GetTable<AssemblyReference>();

            const string typeNamespace = "System.Windows.Forms";
            const string typeName = "Form";

            var assemblyDescr = new ReflectionAssemblyNameWrapper(typeof(Form).Assembly.GetName());
            var reference = new TypeReference(importer.ImportAssembly(assemblyDescr), typeNamespace, typeName);

            var newReference = importer.ImportType(reference);

            Assert.AreNotSame(reference, newReference,
                "Imported reference is the same object as original.");
            Assert.IsTrue(typeRefTable.Contains(newReference),
                "Imported reference not added to reference table.");

            Assert.IsTrue(_comparer.MatchTypes(reference, newReference),
                "Imported reference does not match original.");

            Assert.IsTrue(assemblyRefTable.FirstOrDefault(x => _comparer.MatchAssemblies(x, assemblyDescr)) != null,
                "Assembly reference not added to reference table.");
        }

        [TestMethod]
        public void ImportExistingTypeReference()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var importer = new ReferenceImporter(tableStream);
            var typeRefTable = tableStream.GetTable<TypeReference>();
            var assemblyRefTable = tableStream.GetTable<AssemblyReference>();

            const string typeNamespace = "System.Windows.Forms";
            const string typeName = "Form";

            var assemblyDescr = new ReflectionAssemblyNameWrapper(typeof(Form).Assembly.GetName());
            var reference = new TypeReference(importer.ImportAssembly(assemblyDescr), typeNamespace, typeName);
            typeRefTable.Add(reference);

            var newReference = importer.ImportType(reference);

            Assert.AreSame(reference, newReference,
                "Imported reference does not match original.");

            Assert.IsTrue(assemblyRefTable.FirstOrDefault(x => _comparer.MatchAssemblies(x, assemblyDescr)) != null,
                "Assembly reference not added to reference table.");
        }

        #endregion

        #region Reflection

        [TestMethod]
        public void ImportReflectionType()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var importer = new ReferenceImporter(tableStream);
            var typeRefTable = tableStream.GetTable<TypeReference>();
            var assemblyRefTable = tableStream.GetTable<AssemblyReference>();

            var assemblyDescr = new ReflectionAssemblyNameWrapper(typeof(Form).Assembly.GetName());

            var expected = new TypeReference(new AssemblyReference(assemblyDescr), typeof(Form).Namespace, typeof(Form).Name);
            var newReference = importer.ImportType(typeof(Form));

            Assert.IsTrue(_comparer.MatchTypes(expected, newReference),
                "Imported reference does not match original.");

            Assert.IsTrue(assemblyRefTable.FirstOrDefault(x => _comparer.MatchAssemblies(x, assemblyDescr)) != null,
                "Assembly reference not added to reference table.");
        }

        #endregion

        #region Type definition

        [TestMethod]
        public void ImportInternalTypeDefinition()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var importer = new ReferenceImporter(tableStream);

            var typeDef = new TypeDefinition("SomeNamespace", "SomeName");
            tableStream.GetTable<TypeDefinition>().Add(typeDef);

            var newReference = importer.ImportType(typeDef);

            Assert.AreSame(typeDef, newReference,
                "Imported reference is not the same object as original.");
        }

        [TestMethod]
        public void ImportExternalTypeDefinition()
        {
            var externalAssembly = Utilities.CreateTempNetAssembly();
            var tableStream = externalAssembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var externalAssemblyDef = tableStream.GetTable<AssemblyDefinition>().First();

            var externalType = new TypeDefinition("SomeNamespace", "SomeName");
            tableStream.GetTable<TypeDefinition>().Add(externalType);

            var assembly = Utilities.CreateTempNetAssembly();
            tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var importer = new ReferenceImporter(tableStream);

            var newReference = importer.ImportType(externalType);

            Assert.AreNotSame(externalType, newReference,
                "Imported reference is the same object as original.");

            Assert.IsTrue(_comparer.MatchTypes(externalType, newReference),
                "Imported reference does not match original.");

            Assert.IsTrue(tableStream.GetTable<AssemblyReference>().FirstOrDefault(x => _comparer.MatchAssemblies(x, externalAssemblyDef)) != null,
                "Assembly reference not added to table.");
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

        [TestMethod]
        public void ImportTypeDefOrRefSignature()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var importer = new ReferenceImporter(tableStream);

            var signature = CreateTypeDefOrRef(typeof(Form));

            var newSignature = importer.ImportTypeSignature(signature);

            Assert.AreNotSame(signature, newSignature,
                "Imported signature is the same object as original.");

            Assert.IsTrue(_comparer.MatchTypes((TypeSignature) signature, newSignature),
                "Imported signature does not match original.");

            Assert.IsTrue(tableStream.GetTable<TypeReference>().FirstOrDefault(x => _comparer.MatchTypes(x, signature.Type)) != null,
                "Type reference not added to table.");
        }

        private static TSignature TestTypeSpecification<TSignature>(TSignature signature, ITypeDefOrRef baseType)
            where TSignature : TypeSpecificationSignature
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var importer = new ReferenceImporter(tableStream);

            var newSignature = importer.ImportTypeSignature(signature);

            Assert.IsInstanceOfType(newSignature, typeof(TSignature));

            Assert.AreNotSame(signature, newSignature,
                "Imported signature is the same object as original.");

            Assert.IsTrue(_comparer.MatchTypes(signature, newSignature),
                "Imported signature does not match original.");

            Assert.IsTrue(tableStream.GetTable<TypeReference>().FirstOrDefault(x => _comparer.MatchTypes(x, baseType)) != null,
                "Base type reference not added to table.");

            return (TSignature) newSignature;
        }

        [TestMethod]
        public void ImportCorlibType()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var metadataHeader = assembly.NetDirectory.MetadataHeader;
            var tableStream = metadataHeader.GetStream<TableStream>();
            var importer = new ReferenceImporter(tableStream);

            var signature = metadataHeader.TypeSystem.Byte;
            var newSignature = importer.ImportTypeSignature(signature);
            Assert.AreSame(signature, newSignature,
                "Imported signature is not the same object as original.");
        }

        [TestMethod]
        public void ImportArrayType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            var arrayType = new ArrayTypeSignature(baseType);

            arrayType.Dimensions.Add(new ArrayDimension(null, 0));
            arrayType.Dimensions.Add(new ArrayDimension(null, 1));

            var newType = TestTypeSpecification(arrayType, baseType.Type);

            Assert.AreEqual(arrayType.Dimensions.Count, newType.Dimensions.Count);

            for (int i = 0; i < arrayType.Dimensions.Count; i++)
            {
                Assert.AreEqual(arrayType.Dimensions[i].Size, newType.Dimensions[i].Size);
                Assert.AreEqual(arrayType.Dimensions[i].LowerBound, newType.Dimensions[i].LowerBound);
            }
        }

        [TestMethod]
        public void ImportBoxedType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            TestTypeSpecification(new BoxedTypeSignature(baseType), baseType.Type);
        }

        [TestMethod]
        public void ImportByReferenceType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            TestTypeSpecification(new ByReferenceTypeSignature(baseType), baseType.Type);
        }

        [TestMethod]
        public void ImportOptionalModifierType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            var modifierType = CreateTypeReference(typeof(IsVolatile));
            var modOptType = new OptionalModifierSignature(modifierType, baseType);
            var newType = TestTypeSpecification(modOptType, baseType.Type);

            Assert.AreNotSame(modifierType, newType.ModifierType,
                "Modifier type is the same object as original.");
        }

        [TestMethod]
        public void ImportPinnedType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            TestTypeSpecification(new PinnedTypeSignature(baseType), baseType.Type);
        }

        [TestMethod]
        public void ImportPointerType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            TestTypeSpecification(new PointerTypeSignature(baseType), baseType.Type);
        }

        [TestMethod]
        public void ImportRequiredModifierType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            var modifierType = CreateTypeReference(typeof(IsVolatile));
            var modOptType = new RequiredModifierSignature(modifierType, baseType);
            var newType = TestTypeSpecification(modOptType, baseType.Type);

            Assert.AreNotSame(modifierType, newType.ModifierType,
                "Modifier type is the same object as original.");
        }

        [TestMethod]
        public void ImportSentinelType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            TestTypeSpecification(new SentinelTypeSignature(baseType), baseType.Type);
        }

        [TestMethod]
        public void ImportSzArrayType()
        {
            var baseType = CreateTypeDefOrRef(typeof(Form));
            TestTypeSpecification(new SzArrayTypeSignature(baseType), baseType.Type);
        }

        [TestMethod]
        public void ImportGenericInstanceType()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var importer = new ReferenceImporter(tableStream);

            var signature = new GenericInstanceTypeSignature(CreateTypeReference(typeof(List<>)));
            var genericArg = CreateTypeDefOrRef(typeof(Form));
            signature.GenericArguments.Add(genericArg);

            var newSignature = importer.ImportTypeSignature(signature);

            Assert.AreNotSame(signature, newSignature,
                "Imported signature is the same object as original.");

            Assert.IsTrue(_comparer.MatchTypes(signature, newSignature),
                "Imported signature does not match original.");

            Assert.IsTrue(tableStream.GetTable<TypeReference>().FirstOrDefault(x => _comparer.MatchTypes(x, signature.GenericType)) != null,
                "Generic type reference not added to table.");

            Assert.IsTrue(tableStream.GetTable<TypeReference>().FirstOrDefault(x => _comparer.MatchTypes(x, genericArg.Type)) != null,
                "Generic type argument not added to table.");

        }

        #endregion

        #region Type specification

        [TestMethod]
        public void ImportTypeSpecification()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var importer = new ReferenceImporter(tableStream);

            var typeSpec = new TypeSpecification(CreateTypeDefOrRef(typeof(Form)));

            var newSpec = importer.ImportType(typeSpec);

            Assert.AreNotSame(typeSpec, newSpec,
                "Imported type is the same object as original.");

            Assert.IsTrue(_comparer.MatchTypes(typeSpec, newSpec));
        }

        #endregion

        #endregion

        #region Methods

        [TestMethod]
        public void ImportNewMethodReference()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var metadataHeader = assembly.NetDirectory.MetadataHeader;
            var tableStream = metadataHeader.GetStream<TableStream>();

            var importer = new ReferenceImporter(tableStream);

            var reference = new MemberReference(
                CreateTypeReference(typeof(Console)),
                "WriteLine",
                new MethodSignature(new[] { metadataHeader.TypeSystem.String }, metadataHeader.TypeSystem.Void));

            VerifyImportedReference(tableStream, reference, importer.ImportMember(reference));
        }

        [TestMethod]
        public void ImportExistingMethodReference()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var metadataHeader = assembly.NetDirectory.MetadataHeader;
            var tableStream = metadataHeader.GetStream<TableStream>();
            var refTable = tableStream.GetTable<MemberReference>();

            var importer = new ReferenceImporter(tableStream);

            var reference = new MemberReference(
                CreateTypeReference(typeof(Console)),
                "WriteLine",
                new MethodSignature(new[] { metadataHeader.TypeSystem.String }, metadataHeader.TypeSystem.Void));
            refTable.Add(reference);

            var newReference = importer.ImportMember(reference);

            Assert.AreSame(reference, newReference,
                "Imported reference is not the same object as the original.");
        }

        [TestMethod]
        public void ImportReflectionMethod()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var metadataHeader = assembly.NetDirectory.MetadataHeader;
            var tableStream = metadataHeader.GetStream<TableStream>();

            var importer = new ReferenceImporter(tableStream);

            var reference = new MemberReference(
                CreateTypeReference(typeof(Console)),
                "WriteLine",
                new MethodSignature(new[] { metadataHeader.TypeSystem.String }, metadataHeader.TypeSystem.Void));

            var newReference = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));

            VerifyImportedReference(tableStream, reference, newReference);
        }

        [TestMethod]
        public void ImportInternalMethodDefinition()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var metadataHeader = assembly.NetDirectory.MetadataHeader;
            var tableStream = metadataHeader.GetStream<TableStream>();

            var importer = new ReferenceImporter(tableStream);

            var type = new TypeDefinition("SomeNamespace", "SomeType");
            tableStream.GetTable<TypeDefinition>().Add(type);

            var method = new MethodDefinition("SomeMethod", MethodAttributes.Public | MethodAttributes.Static,
                new MethodSignature(new[] { metadataHeader.TypeSystem.String }, metadataHeader.TypeSystem.Void));

            type.Methods.Add(method);
            tableStream.GetTable<MethodDefinition>().Add(method);

            var newReference = importer.ImportMethod(method);

            Assert.AreSame(method, newReference,
                "Imported method definition is not the same object as the original.");
        }

        [TestMethod]
        public void ImportExternalMethodDefinition()
        {
            var externalAssembly = Utilities.CreateTempNetAssembly();
            var metadataHeader = externalAssembly.NetDirectory.MetadataHeader;
            var tableStream = metadataHeader.GetStream<TableStream>();

            var type = new TypeDefinition("SomeNamespace", "SomeType");
            tableStream.GetTable<TypeDefinition>().Add(type);

            var method = new MethodDefinition("SomeMethod", MethodAttributes.Public | MethodAttributes.Static,
                new MethodSignature(new[] { metadataHeader.TypeSystem.String }, metadataHeader.TypeSystem.Void));

            type.Methods.Add(method);
            tableStream.GetTable<MethodDefinition>().Add(method);

            var assembly = Utilities.CreateTempNetAssembly();
            tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var importer = new ReferenceImporter(tableStream);

            var newReference = importer.ImportMethod(method) as MemberReference;

            Assert.AreNotSame(method, newReference,
                "Imported method definition is the same object as the original.");

            Assert.IsTrue(_comparer.MatchMembers(method, newReference),
                "Imported method definition does not match the original.");
        }

        #endregion

        #region Fields

        [TestMethod]
        public void ImportNewFieldReference()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var metadataHeader = assembly.NetDirectory.MetadataHeader;
            var tableStream = metadataHeader.GetStream<TableStream>();

            var importer = new ReferenceImporter(tableStream);

            var reference = new MemberReference(
                CreateTypeReference(typeof(string)),
                "Empty",
                new FieldSignature(metadataHeader.TypeSystem.String));

            VerifyImportedReference(tableStream, reference, importer.ImportMember(reference));
        }

        [TestMethod]
        public void ImportExistingFieldReference()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var metadataHeader = assembly.NetDirectory.MetadataHeader;
            var tableStream = metadataHeader.GetStream<TableStream>();
            var refTable = tableStream.GetTable<MemberReference>();

            var importer = new ReferenceImporter(tableStream);

            var reference = new MemberReference(
                CreateTypeReference(typeof(string)),
                "Empty",
                new FieldSignature(metadataHeader.TypeSystem.String));
            refTable.Add(reference);

            var newReference = importer.ImportMember(reference);

            Assert.AreSame(reference, newReference,
                "Imported field reference is not the same object as the original.");
        }

        [TestMethod]
        public void ImportReflectionField()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var metadataHeader = assembly.NetDirectory.MetadataHeader;
            var tableStream = metadataHeader.GetStream<TableStream>();
            var refTable = tableStream.GetTable<MemberReference>();

            var importer = new ReferenceImporter(tableStream);

            var reference = new MemberReference(
                CreateTypeReference(typeof(string)),
                "Empty",
                new FieldSignature(metadataHeader.TypeSystem.String));

            var newReference = importer.ImportField(typeof(string).GetField("Empty", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));
            VerifyImportedReference(tableStream, reference, newReference);
        }


        [TestMethod]
        public void ImportInternalFieldDefinition()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var metadataHeader = assembly.NetDirectory.MetadataHeader;
            var tableStream = metadataHeader.GetStream<TableStream>();

            var importer = new ReferenceImporter(tableStream);

            var type = new TypeDefinition("SomeNamespace", "SomeType");
            tableStream.GetTable<TypeDefinition>().Add(type);

            var field = new FieldDefinition("SomeField", FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(metadataHeader.TypeSystem.String));

            type.Fields.Add(field);
            tableStream.GetTable<FieldDefinition>().Add(field);

            var newReference = importer.ImportField(field);

            Assert.AreSame(field, newReference,
                "Imported field definition is not the same object as the original.");
        }

        [TestMethod]
        public void ImportExternalFieldDefinition()
        {
            var externalAssembly = Utilities.CreateTempNetAssembly();
            var metadataHeader = externalAssembly.NetDirectory.MetadataHeader;
            var tableStream = metadataHeader.GetStream<TableStream>();

            var type = new TypeDefinition("SomeNamespace", "SomeType");
            tableStream.GetTable<TypeDefinition>().Add(type);

            var field = new FieldDefinition("SomeField", FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(metadataHeader.TypeSystem.String));

            type.Fields.Add(field);
            tableStream.GetTable<FieldDefinition>().Add(field);

            var assembly = Utilities.CreateTempNetAssembly();
            tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var importer = new ReferenceImporter(tableStream);

            var newReference = importer.ImportField(field) as MemberReference;

            Assert.AreNotSame(field, newReference,
                "Imported field definition is the same object as the original.");

            Assert.IsTrue(_comparer.MatchMembers(field, newReference),
                "Imported field definition does not match the original.");
        }
        #endregion
    }
}
