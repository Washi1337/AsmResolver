using System.IO;
using System.Linq;
using AsmResolver.Symbols.Pdb.Msf;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Msf;

public class MsfFileTest
{
    [Fact]
    public void RoundTrip()
    {
        var file = MsfFile.FromBytes(Properties.Resources.SimpleDllPdb);

        using var stream = new MemoryStream();
        file.Write(stream);

        var newFile = MsfFile.FromBytes(stream.ToArray());

        Assert.Equal(file.BlockSize, newFile.BlockSize);
        Assert.Equal(file.Streams.Count, newFile.Streams.Count);
        Assert.All(Enumerable.Range(0, file.Streams.Count), i =>
        {
            Assert.Equal(file.Streams[i].CreateReader().ReadToEnd(), newFile.Streams[i].CreateReader().ReadToEnd());;
        });
    }
}
