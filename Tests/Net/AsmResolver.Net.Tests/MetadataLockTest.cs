using System;
using AsmResolver.Net;
using AsmResolver.Net.Metadata;
using Xunit;

namespace AsmResolver.Tests.Net
{
    public class MetadataLockTest
    {
        [Fact]
        public void LockMetadata()
        {
            var assembly = NetAssemblyFactory.CreateAssembly("SomeAssembly", true);
            var header = assembly.NetDirectory.MetadataHeader;

            Assert.False(header.IsLocked);
            Assert.Null(header.Image);

            var tableStream = header.GetStream<TableStream>();
            Assert.False(tableStream.IsReadOnly);
            Assert.All(tableStream.GetPresentTables(), x => Assert.False(x.IsReadOnly));

            tableStream.GetTable<TypeReferenceTable>().Add(new MetadataRow<uint, uint, uint>
            {
                Column1 = 1,
                Column2 = 2,
                Column3 = 3,
            });

            var image = header.LockMetadata();
            Assert.True(header.IsLocked);
            Assert.Equal(header.Image, image);
            Assert.True(tableStream.IsReadOnly);
            Assert.All(tableStream.GetPresentTables(), x => Assert.True(x.IsReadOnly));

            Assert.Throws<InvalidOperationException>(() =>
            {
                tableStream.GetTable<TypeReferenceTable>()
                    .Add(new MetadataRow<uint, uint, uint>
                    {
                        Column1 = 1,
                        Column2 = 2,
                        Column3 = 3,
                    });
            });

            Assert.Throws<InvalidOperationException>(() =>
            {
                tableStream.GetTable<TypeReferenceTable>()[0].Column2 = 4;
            });

            header.UnlockMetadata();

            Assert.False(header.IsLocked);
            Assert.Null(header.Image);
            tableStream = header.GetStream<TableStream>();
            Assert.False(tableStream.IsReadOnly);
            Assert.All(tableStream.GetPresentTables(), x => Assert.False(x.IsReadOnly));
        }
    }
}
