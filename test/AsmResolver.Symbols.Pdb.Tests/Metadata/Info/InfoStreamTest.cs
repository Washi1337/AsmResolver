using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Metadata.Info;
using AsmResolver.Symbols.Pdb.Msf;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Metadata.Info;

public class InfoStreamTest
{
    private static InfoStream GetInfoStream(bool rebuild)
    {
        var file = MsfFile.FromBytes(Properties.Resources.SimpleDllPdb);
        var infoStream = InfoStream.FromReader(file.Streams[InfoStream.StreamIndex].CreateReader());

        if (rebuild)
        {
            using var stream = new MemoryStream();
            infoStream.Write(new BinaryStreamWriter(stream));
            infoStream = InfoStream.FromReader(new BinaryStreamReader(stream.ToArray()));
        }

        return infoStream;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Header(bool rebuild)
    {
        var infoStream = GetInfoStream(rebuild);

        Assert.Equal(InfoStreamVersion.VC70, infoStream.Version);
        Assert.Equal(1u, infoStream.Age);
        Assert.Equal(Guid.Parse("205dc366-d8f8-4175-8e06-26dd76722df5"), infoStream.UniqueId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NameTable(bool rebuild)
    {
        var infoStream = GetInfoStream(rebuild);

        Assert.Equal(new Dictionary<Utf8String, int>
        {
            ["/UDTSRCLINEUNDONE"] = 48,
            ["/src/headerblock"] = 46,
            ["/LinkInfo"] = 5,
            ["/TMCache"] = 6,
            ["/names"] = 12
        }, infoStream.StreamIndices);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FeatureCodes(bool rebuild)
    {
        var infoStream = GetInfoStream(rebuild);

        Assert.Equal(new[] {PdbFeature.VC140}, infoStream.Features);
    }

    [Fact]
    public void SizeCalculation()
    {
        var file = MsfFile.FromBytes(Properties.Resources.SimpleDllPdb);
        var infoStream = InfoStream.FromReader(file.Streams[InfoStream.StreamIndex].CreateReader());

        uint calculatedSize = infoStream.GetPhysicalSize();

        using var stream = new MemoryStream();
        infoStream.Write(new BinaryStreamWriter(stream));

        Assert.Equal(stream.Length, calculatedSize);
    }
}
