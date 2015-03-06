using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class FieldMarshalTests
    {
        [TestMethod]
        public void CreateSimpleMarshalDescriptor()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var descriptor = new SimpleMarshalDescriptor(NativeType.U4);
            BuildAppWithDescriptor(assembly, assembly.NetDirectory.MetadataHeader.TypeSystem.Int32,
                descriptor, false);
        }

        [TestMethod]
        public void CreateFixedArrayMarshalDescriptor()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var descriptor = new FixedArrayMarshalDescriptor()
            {
                ElementType = NativeType.U4,
                NumberOfElements = 3,
            };

            var newDescriptor = BuildAppWithDescriptor(assembly,
                new SzArrayTypeSignature(assembly.NetDirectory.MetadataHeader.TypeSystem.Int32),
                descriptor, false);

            Assert.AreEqual(descriptor.ElementType, newDescriptor.ElementType);
            Assert.AreEqual(descriptor.NumberOfElements, newDescriptor.NumberOfElements);
        }

        [TestMethod]
        public void CreateSafeArrayMarshalDescriptor()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var descriptor = new SafeArrayMarshalDescriptor()
            {
                ElementType = VariantType.UI4,
            };

            var newDescriptor = BuildAppWithDescriptor(assembly,
                new SzArrayTypeSignature(assembly.NetDirectory.MetadataHeader.TypeSystem.Int32),
                descriptor, false);

            Assert.AreEqual(descriptor.ElementType, newDescriptor.ElementType);
        }

        [TestMethod]
        public void CreateArrayMarshalDescriptor()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var descriptor = new ArrayMarshalDescriptor(NativeType.U4)
            {
                NumberOfElements = 5,
                ParameterIndex = 1,
            };

            var newDescriptor = BuildAppWithDescriptor(assembly,
                new SzArrayTypeSignature(assembly.NetDirectory.MetadataHeader.TypeSystem.Int32),
                descriptor, false);

            Assert.AreEqual(descriptor.ElementType, newDescriptor.ElementType);
            Assert.AreEqual(descriptor.NumberOfElements, newDescriptor.NumberOfElements);
            Assert.AreEqual(descriptor.ParameterIndex, newDescriptor.ParameterIndex);
        }

        [TestMethod]
        public void CreateCustomMarshalDescriptor()
        {
            var assembly = Utilities.CreateTempNetAssembly();
            var descriptor = new CustomMarshalDescriptor()
            {
                Guid = Guid.NewGuid(),
                UnmanagedType = "SomeUnmanagedType",
                ManagedType = "System.WitchCraft.Marshaler",
                Cookie = "1337",
            };

            var newDescriptor = BuildAppWithDescriptor(assembly,
                new SzArrayTypeSignature(assembly.NetDirectory.MetadataHeader.TypeSystem.Int32),
                descriptor, false);

            Assert.AreEqual(descriptor.Guid, newDescriptor.Guid);
            Assert.AreEqual(descriptor.UnmanagedType, newDescriptor.UnmanagedType);
            Assert.AreEqual(descriptor.ManagedType, newDescriptor.ManagedType);
            Assert.AreEqual(descriptor.Cookie, newDescriptor.Cookie);
        }

        private static TDescriptor BuildAppWithDescriptor<TDescriptor>(WindowsAssembly assembly, TypeSignature fieldType, TDescriptor descriptor, bool saveToDisk)
            where TDescriptor : MarshalDescriptor
        {
            const string fieldName = "MyField";

            // set up temp assembly.
            
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var fieldMarshalTable = tableStream.GetTable<FieldMarshal>();

            // create temp field.
            var field = new FieldDefinition(fieldName,
                FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.HasFieldRva,
                new FieldSignature(fieldType));
            fieldTable.Add(field);

            // create field marshal.
            var marshal = new FieldMarshal(field, descriptor);
            field.FieldMarshal = marshal;
            fieldMarshalTable.Add(marshal);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly, saveToDisk);
            tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            fieldTable = tableStream.GetTable<FieldDefinition>();

            field = fieldTable.FirstOrDefault(x => x.Name == fieldName);
            Assert.IsNotNull(field);
            Assert.IsNotNull(field.FieldMarshal);
            Assert.AreEqual(descriptor.NativeType, field.FieldMarshal.MarshalDescriptor.NativeType);
            Assert.IsInstanceOfType(field.FieldMarshal.MarshalDescriptor, typeof(TDescriptor));
            return (TDescriptor)field.FieldMarshal.MarshalDescriptor;
        }
    }
}
