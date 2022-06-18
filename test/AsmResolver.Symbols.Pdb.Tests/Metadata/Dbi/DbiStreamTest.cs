using AsmResolver.PE.File.Headers;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;
using AsmResolver.Symbols.Pdb.Msf;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Metadata.Dbi;

public class DbiStreamTest
{
    [Fact]
    public void Read()
    {
        var file = MsfFile.FromBytes(Properties.Resources.SimpleDllPdb);
        var dbiStream = DbiStream.FromReader(file.Streams[DbiStream.StreamIndex].CreateReader());

        Assert.Equal(1u, dbiStream.Age);
        Assert.Equal(DbiAttributes.None, dbiStream.Attributes);
        Assert.Equal(MachineType.I386, dbiStream.Machine);
    }
}
