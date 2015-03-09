using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsmResolver.Tests.Net
{
    [TestClass]
    public class TypeSignatureTests
    {
        private const string FieldName = "MyField";

        [TestMethod]
        public void CreateSimpleField()
        {
            var type = typeof(Process);

            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // create field.
            var field = new FieldDefinition(FieldName, FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(type)));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable.First(x => x.Name == FieldName);

            Utilities.ValidateType(type, field.Signature.FieldType);
        }

        [TestMethod]
        public void CreateArrayField()
        {
            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var typeSystem = assembly.NetDirectory.MetadataHeader.TypeSystem;
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // create field.
            var arraySignature = new ArrayTypeSignature(typeSystem.Int32);
            arraySignature.Dimensions.Add(new ArrayDimension(2, 1));
            arraySignature.Dimensions.Add(new ArrayDimension(2));
            arraySignature.Dimensions.Add(new ArrayDimension());

            var field = new FieldDefinition(FieldName, FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(arraySignature)));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable.First(x => x.Name == FieldName);

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(ArrayTypeSignature));
            Utilities.ValidateType(arraySignature, field.Signature.FieldType);
        }

        [TestMethod]
        public void CreateArrayFieldReflection()
        {
            var type = typeof(int[, , , ,]);

            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // create field.
            var field = new FieldDefinition(FieldName, FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(type)));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable.First(x => x.Name == FieldName);

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(ArrayTypeSignature));
            Utilities.ValidateType(type, field.Signature.FieldType);
        }

        [TestMethod]
        public void CreateSzArrayField()
        {
            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var typeSystem = assembly.NetDirectory.MetadataHeader.TypeSystem;
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // create field.
            var field = new FieldDefinition(FieldName, FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(new SzArrayTypeSignature(typeSystem.String))));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable.First(x => x.Name == FieldName);

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(SzArrayTypeSignature));
            Utilities.ValidateType(typeof(string[]), field.Signature.FieldType);
        }

        [TestMethod]
        public void CreateSzArrayFieldReflection()
        {
            var type = typeof(string[]);

            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);
            
            // create field.
            var field = new FieldDefinition(FieldName, FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(type)));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable.First(x => x.Name == FieldName);

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(SzArrayTypeSignature));
            Utilities.ValidateType(type, field.Signature.FieldType);
        }

        [TestMethod]
        public void CreatePointerField()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var typeSystem = assembly.NetDirectory.MetadataHeader.TypeSystem;
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // create field.
            var field = new FieldDefinition(FieldName, FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(new PointerTypeSignature(typeSystem.Int32))));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable.First(x => x.Name == FieldName);

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(PointerTypeSignature));
            Utilities.ValidateType(typeof(int*), field.Signature.FieldType);
        }

        [TestMethod]
        public void CreatePointerFieldReflection()
        {
            var type = typeof(int*);

            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // create field.
            var field = new FieldDefinition(FieldName, FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(type)));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable.First(x => x.Name == FieldName);

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(PointerTypeSignature));
            Utilities.ValidateType(type, field.Signature.FieldType);
        }

        [TestMethod]
        public void CreateByRefField()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var typeSystem = assembly.NetDirectory.MetadataHeader.TypeSystem;
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // create field.
            var field = new FieldDefinition(FieldName, FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(new ByReferenceTypeSignature(typeSystem.Int32))));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable.First(x => x.Name == FieldName);

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(ByReferenceTypeSignature));
            Utilities.ValidateType(typeof(int).MakeByRefType(), field.Signature.FieldType);
        }

        [TestMethod]
        public void CreateByRefFieldReflection()
        {
            var type = typeof(int).MakeByRefType();

            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // create field.
            var field = new FieldDefinition(FieldName, FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(type)));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable.First(x => x.Name == FieldName);

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(ByReferenceTypeSignature));
            Utilities.ValidateType(type, field.Signature.FieldType);
        }

        [TestMethod]
        public void CreateGenericInstanceField()
        {
            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var typeSystem = assembly.NetDirectory.MetadataHeader.TypeSystem;
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // create field.
            var typeSignature = new GenericInstanceTypeSignature(importer.ImportType(typeof(List<>)));
            typeSignature.GenericArguments.Add(typeSystem.String);
            var field = new FieldDefinition(FieldName, FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(typeSignature));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable.First(x => x.Name == FieldName);

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(GenericInstanceTypeSignature));
            var newTypeSignature = (GenericInstanceTypeSignature)field.Signature.FieldType;
            Utilities.ValidateType(typeSignature.GenericType, newTypeSignature.GenericType);
            Assert.AreEqual(typeSignature.GenericArguments.Count, newTypeSignature.GenericArguments.Count);
            for (int i = 0; i < typeSignature.GenericArguments.Count; i++)
                Assert.AreEqual(typeSignature.GenericArguments[i].FullName,
                    newTypeSignature.GenericArguments[i].FullName);
        }

        [TestMethod]
        public void CreateGenericInstanceFieldReflection()
        {
            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // create field.
            var typeSignature = (GenericInstanceTypeSignature)importer.ImportTypeSignature(typeof(List<string>));
            var field = new FieldDefinition(FieldName, FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(typeSignature));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable.First(x => x.Name == FieldName);

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(GenericInstanceTypeSignature));
            var newTypeSignature = (GenericInstanceTypeSignature)field.Signature.FieldType;
            Utilities.ValidateType(typeSignature.GenericType, newTypeSignature.GenericType);
            Assert.AreEqual(typeSignature.GenericArguments.Count, newTypeSignature.GenericArguments.Count);
            for (int i = 0; i < typeSignature.GenericArguments.Count; i++)
                Assert.AreEqual(typeSignature.GenericArguments[i].FullName,
                    newTypeSignature.GenericArguments[i].FullName);
        }

        [TestMethod]
        public void CreateRequiredModifierField()
        {
            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var typeSystem = assembly.NetDirectory.MetadataHeader.TypeSystem;
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // create field.
            var typeSignature = new RequiredModifierSignature(importer.ImportType(typeof(IsVolatile)), typeSystem.Int32);
            var field = new FieldDefinition(FieldName, FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(typeSignature));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly, true);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable.First(x => x.Name == FieldName);

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(RequiredModifierSignature));
            var newTypeSignature = (RequiredModifierSignature)field.Signature.FieldType;
            Utilities.ValidateType(typeSignature.ModifierType, newTypeSignature.ModifierType);
            Utilities.ValidateType(typeSignature.BaseType, newTypeSignature.BaseType);
        }
    }
}
