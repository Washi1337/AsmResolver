
using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class TypeDefinitionTest
    {
        private const string DummyAssemblyName = "SomeAssemblyName";

        [Fact]
        public void PersistentAttributes()
        {
            const TypeAttributes newAttributes = TypeAttributes.Public | TypeAttributes.Sealed;

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var type = image.Assembly.Modules[0].Types[0];
            type.Attributes = newAttributes;

            var mapping = header.UnlockMetadata();

            var typeRow = header.GetStream<TableStream>().GetTable<TypeDefinitionTable>()[(int)(mapping[type].Rid - 1)];
            Assert.Equal(newAttributes, typeRow.Column1);

            image = header.LockMetadata();
            Assert.Equal(newAttributes, ((TypeDefinition)image.ResolveMember(mapping[type])).Attributes);
        }

        [Fact]
        public void PersistentName()
        {
            const string newName = "SomeTypeName";

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var type = image.Assembly.Modules[0].Types[0];
            type.Name = newName;

            var mapping = header.UnlockMetadata();

            var typeRow = header.GetStream<TableStream>().GetTable<TypeDefinitionTable>()[(int)(mapping[type].Rid - 1)];
            Assert.Equal(newName, header.GetStream<StringStream>().GetStringByOffset(typeRow.Column2));

            image = header.LockMetadata();
            Assert.Equal(newName, ((TypeDefinition)image.ResolveMember(mapping[type])).Name);
        }

        [Fact]
        public void PersistentNamespace()
        {
            const string newNamespace = "SomeTypeNamespace";

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var type = image.Assembly.Modules[0].Types[0];
            type.Namespace = newNamespace;

            var mapping = header.UnlockMetadata();

            var typeRow = header.GetStream<TableStream>().GetTable<TypeDefinitionTable>()[(int)(mapping[type].Rid - 1)];
            Assert.Equal(newNamespace, header.GetStream<StringStream>().GetStringByOffset(typeRow.Column3));

            image = header.LockMetadata();
            Assert.Equal(newNamespace, ((TypeDefinition)image.ResolveMember(mapping[type])).Namespace);
        }
    }
}
