using System;
using System.Collections.Generic;
using AsmResolver.Symbols.Pdb.Metadata.Info;
using AsmResolver.Symbols.Pdb.Msf;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Metadata.Info;

public class InfoStreamTest
{
    [Fact]
    public void Read()
    {
        var file = MsfFile.FromBytes(Properties.Resources.SimpleDllPdb);
        var infoStream = InfoStream.FromReader(file.Streams[1].CreateReader());

        Assert.Equal(InfoStreamVersion.VC70, infoStream.Version);
        Assert.Equal(1u, infoStream.Age);
        Assert.Equal(Guid.Parse("205dc366-d8f8-4175-8e06-26dd76722df5"), infoStream.UniqueId);
        Assert.Equal(new Dictionary<Utf8String, int>
        {
            ["/UDTSRCLINEUNDONE"] = 48,
            ["/src/headerblock"] = 46,
            ["/LinkInfo"] = 5,
            ["/TMCache"] = 6,
            ["/names"] = 12
        }, infoStream.StreamIndices);
        Assert.Equal(new[] {PdbFeature.VC140}, infoStream.Features);
    }
}
