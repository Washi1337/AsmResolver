using System;
using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class AssemblyReferenceTest
    {
        private const string DummyAssemblyName = "SomeAssembly";
        private const string ExternalAssemblyName = "ExternalAssembly";
        
        private static AssemblyReference CreateAndAddDummyReference(MetadataImage image)
        {
            var reference = new AssemblyReference(ExternalAssemblyName, new Version(), image);
            image.Assembly.Modules[0].TopLevelTypes.Add(new TypeDefinition("SomeNamespace", "SomeName",
                new TypeReference(reference, "Namespace", "Name")));
            image.Assembly.AssemblyReferences.Add(reference);
            return reference;
        }

        [Fact]
        public void PersistentHashValue()
        {
            var newHashValue = new DataBlobSignature(new byte[] { 0x1, 0x2, 0x3, 0x4 });

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var reference = CreateAndAddDummyReference(image);
            reference.HashValue = newHashValue;

            var mapping = header.UnlockMetadata();
            var assemblyRow = header.GetStream<TableStream>().GetTable<AssemblyReferenceTable>()[(int) (mapping[reference].Rid - 1)];

            Assert.Equal(newHashValue.Data, header.GetStream<BlobStream>().GetBlobByOffset(assemblyRow.Column9));

            image = header.LockMetadata();
            var newReference = (AssemblyReference)image.ResolveMember(mapping[reference]);
            Assert.Equal(newHashValue.Data, newReference.HashValue.Data);
        }

        [Fact]
        public void PersistentVersion()
        {
            var newVersion = new Version(1, 2, 3, 4);

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var reference = CreateAndAddDummyReference(image);
            reference.Version = newVersion;

            var mapping = header.UnlockMetadata();
            var assemblyRow = header.GetStream<TableStream>().GetTable<AssemblyReferenceTable>()[(int)(mapping[reference].Rid - 1)];

            Assert.Equal(newVersion.Major, assemblyRow.Column1);
            Assert.Equal(newVersion.Minor, assemblyRow.Column2);
            Assert.Equal(newVersion.Build, assemblyRow.Column3);
            Assert.Equal(newVersion.Revision, assemblyRow.Column4);

            image = header.LockMetadata();
            var newReference = (AssemblyReference)image.ResolveMember(mapping[reference]);
            Assert.Equal(newVersion, newReference.Version);
        }

        [Fact]
        public void PersistentAttributes()
        {
            const AssemblyAttributes newAttributes = AssemblyAttributes.Msil
                                                     | AssemblyAttributes.EnableJitCompileTracking;

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var reference = CreateAndAddDummyReference(image);
            reference.Attributes = newAttributes;

            var mapping = header.UnlockMetadata();
            var assemblyRow = header.GetStream<TableStream>().GetTable<AssemblyReferenceTable>()[(int)(mapping[reference].Rid - 1)];
            Assert.Equal(newAttributes, assemblyRow.Column5);

            image = header.LockMetadata();
            var newReference = (AssemblyReference)image.ResolveMember(mapping[reference]);
            Assert.Equal(newAttributes, newReference.Attributes);
        }

        [Fact]
        public void PersistentPublicKey()
        {
            var newPublicKey = new DataBlobSignature(new byte[] { 0x1, 0x2, 0x3, 0x4 });

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var reference = CreateAndAddDummyReference(image);
            reference.PublicKey = newPublicKey;

            var mapping = header.UnlockMetadata();
            var assemblyRow = header.GetStream<TableStream>().GetTable<AssemblyReferenceTable>()[(int)(mapping[reference].Rid - 1)];
            Assert.Equal(newPublicKey.Data, header.GetStream<BlobStream>().GetBlobByOffset(assemblyRow.Column6));

            image = header.LockMetadata();
            var newReference = (AssemblyReference)image.ResolveMember(mapping[reference]);
            Assert.Equal(newPublicKey.Data, newReference.PublicKey.Data);
        }

        [Fact]
        public void PersistentName()
        {
            const string newName = "SomeOtherAssembly";

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var reference = CreateAndAddDummyReference(image);
            reference.Name = newName;

            var mapping = header.UnlockMetadata();
            var assemblyRow = header.GetStream<TableStream>().GetTable<AssemblyReferenceTable>()[(int)(mapping[reference].Rid - 1)];
            Assert.Equal(newName, header.GetStream<StringStream>().GetStringByOffset(assemblyRow.Column7));

            image = header.LockMetadata();
            var newReference = (AssemblyReference)image.ResolveMember(mapping[reference]);
            Assert.Equal(newName, newReference.Name);
        }

        [Fact]
        public void PersistentCulture()
        {
            const string newCulture = "en-GB";

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var reference = CreateAndAddDummyReference(image);
            reference.Culture = newCulture;

            var mapping = header.UnlockMetadata();
            var assemblyRow = header.GetStream<TableStream>().GetTable<AssemblyReferenceTable>()[(int)(mapping[reference].Rid - 1)];
            Assert.Equal(newCulture, header.GetStream<StringStream>().GetStringByOffset(assemblyRow.Column8));

            image = header.LockMetadata();
            var newReference = (AssemblyReference)image.ResolveMember(mapping[reference]);
            Assert.Equal(newCulture, newReference.Culture);
        }
    }
}
