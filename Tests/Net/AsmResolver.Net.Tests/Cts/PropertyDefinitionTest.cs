using System;
using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class PropertyDefinitionTest
    {
        private const string DummyAssemblyName = "SomeAssembly";
        private readonly SignatureComparer _comparer = new SignatureComparer();

        private static PropertyDefinition CreateAndAddDummyProperty(MetadataImage image)
        {
            var type = new TypeDefinition("SomeNamespace", "SomeType");
            image.Assembly.Modules[0].TopLevelTypes.Add(type);

            var property = new PropertyDefinition("SomeProperty", new PropertySignature(image.TypeSystem.Int32));
            type.PropertyMap = new PropertyMap
            {
                Properties = {property}
            };

            return property;
        }

        [Fact]
        public void PersistentAttributes()
        {
            const PropertyAttributes newAttributes = PropertyAttributes.SpecialName | PropertyAttributes.RtSpecialName;

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var property = CreateAndAddDummyProperty(image);
            property.Attributes = newAttributes;

            var mapping = header.UnlockMetadata();
            var propertyRow = header.GetStream<TableStream>().GetTable<PropertyDefinitionTable>()[(int)(mapping[property].Rid - 1)];
            Assert.Equal(newAttributes, propertyRow.Column1);

            image = header.LockMetadata();
            property = (PropertyDefinition) image.ResolveMember(mapping[property]);
            Assert.Equal(newAttributes, property.Attributes);
        }

        [Fact]
        public void PersistentName()
        {
            const string newName = "SomeName";

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var property = CreateAndAddDummyProperty(image);
            property.Name = newName;

            var mapping = header.UnlockMetadata();
            var propertyRow = header.GetStream<TableStream>().GetTable<PropertyDefinitionTable>()[(int)(mapping[property].Rid - 1)];
            Assert.Equal(newName, header.GetStream<StringStream>().GetStringByOffset(propertyRow.Column2));

            image = header.LockMetadata();
            property = (PropertyDefinition) image.ResolveMember(mapping[property]);
            Assert.Equal(newName, property.Name);
        }

        [Fact]
        public void PersistentSignature()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var property = CreateAndAddDummyProperty(image);
            var signature = new PropertySignature(image.TypeSystem.Byte);
            property.Signature = signature;

            var mapping = header.UnlockMetadata();
            
            image = header.LockMetadata();
            property = (PropertyDefinition) image.ResolveMember(mapping[property]);
            Assert.Equal(signature, property.Signature, _comparer);
        }

        [Fact]
        public void PersistentDeclaringType()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            
            var type = new TypeDefinition(null, "MyType", TypeAttributes.Public, null);
            image.Assembly.Modules[0].TopLevelTypes.Add(type);
            
            var property = CreateAndAddDummyProperty(image);
            property.PropertyMap.Parent.PropertyMap = null;
            type.PropertyMap = property.PropertyMap;

            var mapping = header.UnlockMetadata();
            
            image = header.LockMetadata();
            property = (PropertyDefinition) image.ResolveMember(mapping[property]);
            Assert.Equal(type, property.DeclaringType, _comparer);
        }

        [Fact]
        public void PersistentSemantics()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var property = CreateAndAddDummyProperty(image);
            var getMethod = new MethodDefinition("get_" + property.Name, 
                MethodAttributes.Public | MethodAttributes.SpecialName,
                new MethodSignature(property.Signature.PropertyType));
            property.PropertyMap.Parent.Methods.Add(getMethod);
            property.Semantics.Add(new MethodSemantics(getMethod, MethodSemanticsAttributes.Getter));

            var mapping = header.UnlockMetadata();
            
            image = header.LockMetadata();
            property = (PropertyDefinition) image.ResolveMember(mapping[property]);
            Assert.Equal(1, property.Semantics.Count);
            Assert.Equal(getMethod, property.Semantics[0].Method, _comparer);
            Assert.Equal(MethodSemanticsAttributes.Getter, property.Semantics[0].Attributes);
        }

        [Fact]
        public void PersistentConstant()
        {
            var constantValue = BitConverter.GetBytes(1337);
            
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var property = CreateAndAddDummyProperty(image);
            property.Constant = new Constant(ElementType.I4, new DataBlobSignature(constantValue));
            var mapping = header.UnlockMetadata();
            
            image = header.LockMetadata();
            property = (PropertyDefinition) image.ResolveMember(mapping[property]);
            Assert.NotNull(property.Constant);
            Assert.Equal(ElementType.I4, property.Constant.ConstantType);
            Assert.NotNull(property.Constant.Value);
            Assert.Equal(constantValue, property.Constant.Value.Data);
        }
    }
}