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
    public class FieldRvaTests
    {
        private const string FieldName = "MyField";

        [TestMethod]
        public void CreateInt32FieldRva()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            TestFieldRva(assembly, assembly.NetDirectory.MetadataHeader.TypeSystem.Int32,
                BitConverter.GetBytes(0x1337C0DE), false);
        }

        [TestMethod]
        public void CreateByteFieldRva()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            TestFieldRva(assembly, assembly.NetDirectory.MetadataHeader.TypeSystem.Byte,
                new byte[] { 0x30 }, false);
        }

        [TestMethod]
        public void CreateCustomSizeFieldRva()
        {
            const int dataSize = 128;

            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var typeTable = tableStream.GetTable<TypeDefinition>();
            var classLayoutTable = tableStream.GetTable<ClassLayout>();
            var importer = new ReferenceImporter(tableStream);

            var type = new TypeDefinition(string.Empty, "__StaticArrayInitTypeSize=" + dataSize,
                importer.ImportType(typeof(ValueType)));
            type.MetadataRow.Column5 = 2; // FieldList
            type.MetadataRow.Column6 = 2; // MethodList
            typeTable.Add(type);

            var layout = new ClassLayout(type, 128, 1);
            type.ClassLayout = layout;
            classLayoutTable.Add(layout);

            TestFieldRva(assembly, new TypeDefOrRefSignature(type), 
                Enumerable.Repeat((byte)1, dataSize).ToArray(), false);
        }

        [TestMethod]
        public void TestInterpretData()
        {
            var assembly = Utilities.CreateTempNetAssembly();

            var field = new FieldDefinition(FieldName,
                FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.HasFieldRva,
                new FieldSignature(assembly.NetDirectory.MetadataHeader.TypeSystem.Int32));

            var fieldRva = new FieldRva(field, BitConverter.GetBytes(1337));
            Assert.AreEqual(1337, fieldRva.InterpretData(field.Signature.FieldType));
        }

        [TestMethod]
        public void TestInterpretAsArray()
        {
            var elementType = ElementType.I4;
            int[] expected = new int[] { 1, 2, 3 };
            byte[] data = new byte[] { 1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0 };

            var assembly = Utilities.CreateTempNetAssembly();
            var importer = new ReferenceImporter(assembly.NetDirectory.MetadataHeader.GetStream<TableStream>());
            var fieldType = new TypeDefinition(string.Empty, "__StaticArrayInitTypeSize=" + data.Length,
                 importer.ImportType(typeof(ValueType)));
            
            var field = new FieldDefinition(FieldName,
                FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.HasFieldRva,
                new FieldSignature(new TypeDefOrRefSignature(fieldType) { IsValueType = true }));

            var fieldRva = new FieldRva(field, data);

            int[] actual = fieldRva.InterpretAsArray(elementType).Cast<int>().ToArray();

            Assert.AreEqual(expected.Length, actual.Length, "Length of arrays differ.");
            for (int i = 0; i < actual.Length; i++)
                Assert.AreEqual(expected[i], actual[i], "Element " + i + " differs from expected.");
        }

        private static void TestFieldRva(WindowsAssembly assembly, TypeSignature fieldType, byte[] fieldRvaData, bool saveToDisk)
        {
            // set up temp assembly.
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var fieldRvaTable = tableStream.GetTable<FieldRva>();

            // create temp field.
            var field = new FieldDefinition(FieldName,
                FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.HasFieldRva,
                new FieldSignature(fieldType));
            fieldTable.Add(field);

            // create field rva.
            var fieldRva = new FieldRva(field, fieldRvaData);
            field.FieldRva = fieldRva;
            fieldRvaTable.Add(fieldRva);

            assembly = Utilities.RebuildNetAssembly(assembly, saveToDisk);
            tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            fieldTable = tableStream.GetTable<FieldDefinition>();

            field = fieldTable.FirstOrDefault(x => x.Name == FieldName);
            Assert.IsNotNull(field);
            Assert.IsNotNull(field.FieldRva);
            Utilities.ValidateByteArrays(fieldRvaData, field.FieldRva.Data);
        }
    }
}
