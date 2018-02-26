using System;
using AsmResolver.Net;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class AssemblyDefinitionTest
    {
        private const string DummyAssemblyName = "SomeAssembly";

        [Fact]
        public void PersistentHashAlgorithm()
        {
            const AssemblyHashAlgorithm newAlgorithm = AssemblyHashAlgorithm.Sha1;

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            image.Assembly.HashAlgorithm = newAlgorithm;
            header.UnlockMetadata();

            var assemblyRow = header.GetStream<TableStream>().GetTable<AssemblyDefinitionTable>()[0];

            Assert.Equal(newAlgorithm, assemblyRow.Column1);

            image.Header.LockMetadata();
            Assert.Equal(newAlgorithm, image.Assembly.HashAlgorithm);
        }

        [Fact]
        public void PersistentVersion()
        {
            var newVersion = new Version(1, 2, 3, 4);

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            image.Assembly.Version = newVersion;
            header.UnlockMetadata();

            var assemblyRow = header.GetStream<TableStream>().GetTable<AssemblyDefinitionTable>()[0];

            Assert.Equal(newVersion.Major, assemblyRow.Column2);
            Assert.Equal(newVersion.Minor, assemblyRow.Column3);
            Assert.Equal(newVersion.Build, assemblyRow.Column4);
            Assert.Equal(newVersion.Revision, assemblyRow.Column5);

            image.Header.LockMetadata();
            Assert.Equal(newVersion, image.Assembly.Version);
        }

        [Fact]
        public void PersistentAttributes()
        {
            const AssemblyAttributes newAttributes = AssemblyAttributes.Msil
                                                     | AssemblyAttributes.EnableJitCompileTracking;

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            image.Assembly.Attributes = newAttributes;
            header.UnlockMetadata();

            var assemblyRow = header.GetStream<TableStream>().GetTable<AssemblyDefinitionTable>()[0];
            Assert.Equal(newAttributes, assemblyRow.Column6);

            image.Header.LockMetadata();
            Assert.Equal(newAttributes, image.Assembly.Attributes);
        }

        [Fact]
        public void PersistentPublicKey()
        {
            var newPublicKey = new DataBlobSignature(new byte[] { 0x1, 0x2, 0x3, 0x4 });

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            image.Assembly.PublicKey = newPublicKey;
            header.UnlockMetadata();

            var assemblyRow = header.GetStream<TableStream>().GetTable<AssemblyDefinitionTable>()[0];
            Assert.Equal(newPublicKey.Data, header.GetStream<BlobStream>().GetBlobByOffset(assemblyRow.Column7));

            image.Header.LockMetadata();
            Assert.Equal(newPublicKey.Data, image.Assembly.PublicKey.Data);
        }

        [Fact]
        public void PersistentName()
        {
            const string newName = "SomeOtherAssembly";

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            image.Assembly.Name = newName;
            header.UnlockMetadata();

            var assemblyRow = header.GetStream<TableStream>().GetTable<AssemblyDefinitionTable>()[0];
            Assert.Equal(newName, header.GetStream<StringStream>().GetStringByOffset(assemblyRow.Column8));

            image.Header.LockMetadata();
            Assert.Equal(newName, image.Assembly.Name);
        }

        [Fact]
        public void PersistentCulture()
        {
            const string newCulture = "en-GB";

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            image.Assembly.Culture = newCulture;
            header.UnlockMetadata();

            var assemblyRow = header.GetStream<TableStream>().GetTable<AssemblyDefinitionTable>()[0];
            Assert.Equal(newCulture, header.GetStream<StringStream>().GetStringByOffset(assemblyRow.Column9));

            image.Header.LockMetadata();
            Assert.Equal(newCulture, image.Assembly.Culture);
        }
        
    }
}
