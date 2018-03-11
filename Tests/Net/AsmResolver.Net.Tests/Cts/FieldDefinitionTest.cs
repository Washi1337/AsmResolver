using System;
using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class FieldDefinitionTest
    {
        private const string DummyAssemblyName = "SomeAssembly";
        private readonly SignatureComparer _comparer = new SignatureComparer();

        private FieldDefinition CreateAndAddDummyField(MetadataImage image)
        {
            var type = new TypeDefinition("SomeNamespace", "SomeType");
            image.Assembly.Modules[0].TopLevelTypes.Add(type);

            var field = new FieldDefinition("SomeField", FieldAttributes.PrivateScope,
                new FieldSignature(image.TypeSystem.Int32));
            type.Fields.Add(field);

            return field;
        }

        [Fact]
        public void PersistentAttributes()
        {
            const FieldAttributes newAttributes = FieldAttributes.Public | FieldAttributes.Static;

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var field = CreateAndAddDummyField(image);
            field.Attributes = newAttributes;

            var mapping = header.UnlockMetadata();
            var fieldRow = header.GetStream<TableStream>()
                .GetTable<FieldDefinitionTable>()[(int) (mapping[field].Rid - 1)];
            Assert.Equal(newAttributes, fieldRow.Column1);

            image = header.LockMetadata();
            field = (FieldDefinition) image.ResolveMember(mapping[field]);
            Assert.Equal(newAttributes, field.Attributes);
        }

        [Fact]
        public void PersistentName()
        {
            const string newName = "SomeNewName";

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var field = CreateAndAddDummyField(image);
            field.Name = newName;

            var mapping = header.UnlockMetadata();
            var fieldRow = header.GetStream<TableStream>()
                .GetTable<FieldDefinitionTable>()[(int)(mapping[field].Rid - 1)];
            Assert.Equal(newName, header.GetStream<StringStream>().GetStringByOffset(fieldRow.Column2));

            image = header.LockMetadata();
            field = (FieldDefinition) image.ResolveMember(mapping[field]);
            Assert.Equal(newName, field.Name);
        }

        [Fact]
        public void PersistentSignature()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var field = CreateAndAddDummyField(image);
            var newSignature = new FieldSignature(image.TypeSystem.Boolean);
            field.Signature = newSignature;

            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            field = (FieldDefinition) image.ResolveMember(mapping[field]);
            Assert.Equal(newSignature, field.Signature, _comparer);
        }

        [Fact]
        public void PersistentDeclaringType()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);
            
            var type = new TypeDefinition("MyNamespace", "MyType", TypeAttributes.Public, importer.ImportType(typeof(object)));
            image.Assembly.Modules[0].TopLevelTypes.Add(type);
            
            var field = CreateAndAddDummyField(image);
            field.DeclaringType.Fields.Remove(field);
            type.Fields.Add(field);
            
            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            field = (FieldDefinition) image.ResolveMember(mapping[field]);
            Assert.Equal(type, field.DeclaringType, _comparer);
        }
        
        [Fact]
        public void PersistentConstant()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var field = CreateAndAddDummyField(image);
            var newConstant = new Constant(ElementType.U1, new DataBlobSignature(new byte [] { 42 }));
            field.Constant = newConstant;
            field.Attributes |= FieldAttributes.Literal;
            
            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            field = (FieldDefinition) image.ResolveMember(mapping[field]);
            Assert.NotNull(field.Constant);
            Assert.Equal(newConstant.ConstantType, field.Constant.ConstantType);
            Assert.Equal(newConstant.Value.Data, field.Constant.Value.Data);
        }

        [Fact]
        public void PersistentMarshal()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var field = CreateAndAddDummyField(image);
            var newMarshal = new FieldMarshal(new ArrayMarshalDescriptor(NativeType.U2));
            field.FieldMarshal = newMarshal;
            field.Attributes |= FieldAttributes.HasFieldMarshal;

            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            field = (FieldDefinition) image.ResolveMember(mapping[field]);
            Assert.NotNull(field.FieldMarshal);
            Assert.Equal(newMarshal.MarshalDescriptor, field.FieldMarshal.MarshalDescriptor, _comparer);
        }

        [Fact]
        public void PersistentLayout()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var field = CreateAndAddDummyField(image);
            var newLayout = new FieldLayout(0x1337);
            field.FieldLayout = newLayout;

            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            field = (FieldDefinition) image.ResolveMember(mapping[field]);
            Assert.NotNull(field.FieldLayout);
            Assert.Equal(newLayout.Offset, field.FieldLayout.Offset);
        }

        [Fact]
        public void PersistentPInvokeMap()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);
            var field = CreateAndAddDummyField(image);
            var newMap = new ImplementationMap(importer.ImportModule(new ModuleReference("SomeModule")), 
                "SomeImport",
                ImplementationMapAttributes.CharSetUnicode);
            field.PInvokeMap = newMap;
            field.Attributes |= FieldAttributes.PinvokeImpl;

            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            field = (FieldDefinition) image.ResolveMember(mapping[field]);
            Assert.NotNull(field.PInvokeMap);
            Assert.Equal(newMap.ImportScope, field.PInvokeMap.ImportScope, _comparer);
            Assert.Equal(newMap.ImportName, field.PInvokeMap.ImportName);
            Assert.Equal(newMap.Attributes, field.PInvokeMap.Attributes);
        }
    }
}
