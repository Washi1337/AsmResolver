using System;
using System.Collections.Generic;
using System.Linq;
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

            var field = new FieldDefinition("MyField", FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(arraySignature)));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable[0];

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(ArrayTypeSignature));
            Assert.AreEqual(arraySignature.FullName, field.Signature.FieldType.FullName);
        }

        [TestMethod]
        public void CreateArrayFieldReflection()
        {
            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // create field.
            var field = new FieldDefinition("MyField", FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(typeof(int[,,,,]))));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable[0];

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(ArrayTypeSignature));
            Assert.AreEqual(typeof(int[,,,,]).FullName, field.Signature.FieldType.FullName);
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
            var field = new FieldDefinition("MyField", FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(new SzArrayTypeSignature(typeSystem.String))));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable[0];

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(SzArrayTypeSignature));
            Assert.AreEqual(typeof(string[]).FullName, field.Signature.FieldType.FullName);
        }

        [TestMethod]
        public void CreateSzArrayFieldReflection()
        {
            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);
            
            // create field.
            var field = new FieldDefinition("MyField", FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(typeof(string[]))));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable[0];

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(SzArrayTypeSignature));
            Assert.AreEqual(typeof(string[]).FullName, field.Signature.FieldType.FullName);
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
            var field = new FieldDefinition("MyField", FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(new PointerTypeSignature(typeSystem.Int32))));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable[0];

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(PointerTypeSignature));
            Assert.AreEqual(typeof(int*).FullName, field.Signature.FieldType.FullName);
        }

        [TestMethod]
        public void CreatePointerFieldReflection()
        {
            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // create field.
            var field = new FieldDefinition("MyField", FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(typeof(int*))));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable[0];

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(PointerTypeSignature));
            Assert.AreEqual(typeof(int*).FullName, field.Signature.FieldType.FullName);
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
            var field = new FieldDefinition("MyField", FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(new ByReferenceTypeSignature(typeSystem.Int32))));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable[0];

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(ByReferenceTypeSignature));
            Assert.AreEqual(typeof(int).MakeByRefType().FullName, field.Signature.FieldType.FullName);
        }

        [TestMethod]
        public void CreateByRefFieldReflection()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // create field.
            var field = new FieldDefinition("MyField", FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(importer.ImportTypeSignature(typeof(int).MakeByRefType())));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable[0];

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(ByReferenceTypeSignature));
            Assert.AreEqual(typeof(int).MakeByRefType().FullName, field.Signature.FieldType.FullName);
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
            var field = new FieldDefinition("MyField", FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(typeSignature));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable[0];

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(GenericInstanceTypeSignature));
            var newTypeSignature = (GenericInstanceTypeSignature)field.Signature.FieldType;
            Assert.AreEqual(typeSignature.GenericType.FullName, newTypeSignature.GenericType.FullName);
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
            var field = new FieldDefinition("MyField", FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(typeSignature));
            fieldTable.Add(field);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<FieldDefinition>();
            field = fieldTable[0];

            Assert.IsInstanceOfType(field.Signature.FieldType, typeof(GenericInstanceTypeSignature));
            var newTypeSignature = (GenericInstanceTypeSignature)field.Signature.FieldType;
            Assert.AreEqual(typeSignature.GenericType.FullName, newTypeSignature.GenericType.FullName);
            Assert.AreEqual(typeSignature.GenericArguments.Count, newTypeSignature.GenericArguments.Count);
            for (int i = 0; i < typeSignature.GenericArguments.Count; i++)
                Assert.AreEqual(typeSignature.GenericArguments[i].FullName,
                    newTypeSignature.GenericArguments[i].FullName);
        }

    }
}
