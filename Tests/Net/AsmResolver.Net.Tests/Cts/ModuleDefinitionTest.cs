using System;
using System.Linq;
using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class ModuleDefinitionTest
    {
        private const string DummyAssemblyName = "SomeAssembly";

        [Fact]
        public void PersistentGeneration()
        {
            const ushort newGeneration = 0x1234;

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            image.Assembly.Modules[0].Generation = newGeneration;
            header.UnlockMetadata();

            var moduleRow = header.GetStream<TableStream>().GetTable<ModuleDefinitionTable>()[0];
            Assert.Equal(newGeneration, moduleRow.Column1);

            image = header.LockMetadata();
            Assert.Equal(newGeneration, image.Assembly.Modules[0].Generation);
        }

        [Fact]
        public void PersistentName()
        {
            const string newName = "SomeModuleName.dll";

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            image.Assembly.Modules[0].Name = newName;
            header.UnlockMetadata();

            var moduleRow = header.GetStream<TableStream>().GetTable<ModuleDefinitionTable>()[0];
            Assert.Equal(newName, header.GetStream<StringStream>().GetStringByOffset(moduleRow.Column2));

            image = header.LockMetadata();
            Assert.Equal(newName, image.Assembly.Modules[0].Name);
        }

        [Fact]
        public void PersistentMvid()
        {
            var newGuid = Guid.NewGuid();

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            image.Assembly.Modules[0].Mvid = newGuid;
            header.UnlockMetadata();

            var moduleRow = header.GetStream<TableStream>().GetTable<ModuleDefinitionTable>()[0];
            Assert.Equal(newGuid, header.GetStream<GuidStream>().GetGuidByOffset(moduleRow.Column3));

            image = header.LockMetadata();
            Assert.Equal(newGuid, image.Assembly.Modules[0].Mvid);
        }

        [Fact]
        public void PersistentEncId()
        {
            var newGuid = Guid.NewGuid();

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            image.Assembly.Modules[0].EncId = newGuid;
            header.UnlockMetadata();

            var moduleRow = header.GetStream<TableStream>().GetTable<ModuleDefinitionTable>()[0];
            Assert.Equal(newGuid, header.GetStream<GuidStream>().GetGuidByOffset(moduleRow.Column4));

            image = header.LockMetadata();
            Assert.Equal(newGuid, image.Assembly.Modules[0].EncId);
        }

        [Fact]
        public void PersistentEncBaseId()
        {
            var newGuid = Guid.NewGuid();

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            image.Assembly.Modules[0].EncBaseId = newGuid;
            header.UnlockMetadata();

            var moduleRow = header.GetStream<TableStream>().GetTable<ModuleDefinitionTable>()[0];
            Assert.Equal(newGuid, header.GetStream<GuidStream>().GetGuidByOffset(moduleRow.Column5));

            image = header.LockMetadata();
            Assert.Equal(newGuid, image.Assembly.Modules[0].EncBaseId);
        }

        [Fact]
        public void PersistentTypes()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            image.Assembly.Modules[0].TopLevelTypes.Add(new TypeDefinition("SomeNamespace", "SomeName"));
            int newCount = image.Assembly.Modules[0].TopLevelTypes.Count;
            header.UnlockMetadata();

            image = header.LockMetadata();
            Assert.Equal(newCount, image.Assembly.Modules[0].TopLevelTypes.Count);
        }

    }
}
