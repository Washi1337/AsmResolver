using System.IO;
using System.Linq;
using AsmResolver.PE.Builder;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.PE.Tests.Builder;

public class UnmanagedPEFileBuilderTest : IClassFixture<TemporaryDirectoryFixture>
{
    private readonly TemporaryDirectoryFixture _fixture;

    public UnmanagedPEFileBuilderTest(TemporaryDirectoryFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void RoundTripMixedModeAssembly()
    {
        var image = PEImage.FromBytes(Properties.Resources.MixedModeHelloWorld);
        var file = image.ToPEFile(new UnmanagedPEFileBuilder());

        _fixture.GetRunner<NativePERunner>().RebuildAndRun(
            file,
            "MixedModeHelloWorld.exe",
            "Hello\n1 + 2 = 3\n"
        );
    }

    [Fact]
    public void AddMetadataToMixedModeAssembly()
    {
        const string name = "#Test";
        byte[] data = [1, 2, 3, 4];

        var image = PEImage.FromBytes(Properties.Resources.MixedModeHelloWorld);
        image.DotNetDirectory!.Metadata!.Streams.Add(new CustomMetadataStream(
            name, new DataSegment(data)
        ));

        var file = image.ToPEFile(new UnmanagedPEFileBuilder());
        using var stream = new MemoryStream();
        file.Write(stream);

        var newImage = PEImage.FromBytes(stream.ToArray());
        var metadataStream = Assert.IsAssignableFrom<CustomMetadataStream>(
            newImage.DotNetDirectory!.Metadata!.Streams.First(x => x.Name == name)
        );

        Assert.Equal(data, metadataStream.Contents.WriteIntoArray());

        _fixture.GetRunner<NativePERunner>().RebuildAndRun(
            file,
            "MixedModeHelloWorld.exe",
            "Hello\n1 + 2 = 3\n"
        );
    }
}
