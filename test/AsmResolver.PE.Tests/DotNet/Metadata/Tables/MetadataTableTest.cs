using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables;

public class MetadataTableTest
{
    [Fact]
    public void TryGetRidByKeyShouldRespectSortedFlag()
    {
        var image = PEImage.FromBytes(Properties.Resources.MetadataUnsortedTable);
        var table = image.DotNetDirectory!.Metadata!
            .GetStream<TablesStream>()
            .GetTable<ClassLayoutRow>(TableIndex.ClassLayout);

        const int parentColumnIndex = 2;
        const uint parent = 4;

        Assert.False(table.IsSorted);
        Assert.True(table.TryGetRidByKey(parentColumnIndex, parent, out uint rid));
        Assert.Equal(3u, rid);

        table.IsSorted = true;
        Assert.False(table.TryGetRidByKey(parentColumnIndex, parent, out _));
    }
}
