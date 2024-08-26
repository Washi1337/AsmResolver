using System.IO;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;
using AsmResolver.Symbols.Pdb.Metadata.Modi;
using AsmResolver.Symbols.Pdb.Msf;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Metadata.Modi;

public class C13LineInfoStreamTest
{
    private C13LineInfoStream GetLineInfoStream(bool rebuild)
    {
        var file = MsfFile.FromBytes(Properties.Resources.SimpleDllPdb);
        var dbiStream = DbiStream.FromReader(file.Streams[DbiStream.StreamIndex].CreateReader());

        var module = dbiStream.Modules.First(x => x.ModuleName!.Contains("dllmain.obj"));
        var modi = ModiStream.FromReader(file.Streams[module.SymbolStreamIndex].CreateReader(), module);
        var lineInfo = modi.C13LineInfo;
        Assert.NotNull(lineInfo);

        if (rebuild)
        {
            using var stream = new MemoryStream();
            lineInfo.Write(new BinaryStreamWriter(stream));
            lineInfo = new SerializedC13LineInfoStream(new BinaryStreamReader(stream.ToArray()));
        }

        return lineInfo;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Sections(bool rebuild)
    {
        var info = GetLineInfoStream(rebuild);
        Assert.Equal(new[]
            {
                C13SubSectionType.FileChecksums,
                C13SubSectionType.Lines
            },
            info.Sections.Select(x => x.Type)
        );
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FileChecksums(bool rebuild)
    {
        var info = GetLineInfoStream(rebuild);
        var section = info.Sections.OfType<C13FileChecksumsSection>().First();
        Assert.Equal(
            new[] {C13FileChecksumType.Md5, C13FileChecksumType.Md5, C13FileChecksumType.None},
            section.Checksums.Select(x => x.Type)
        );
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LinesSection(bool rebuild)
    {
        var info = GetLineInfoStream(rebuild);
        var section = info.Sections.OfType<C13LinesSection>().First();
        var block = Assert.Single(section.Blocks);

        Assert.Equal(0x18u, block.FileId);
        Assert.Equal(
            new[] {(8u, 0u), (17u, 0u), (18u, 5u)},
            block.Lines.Select(x => (x.LineNumberStart, x.Offset))
        );
    }
}
