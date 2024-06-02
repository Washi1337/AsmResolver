using System;
using System.IO;
using System.Linq;
using AsmResolver.PE.Builder;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.File;
using AsmResolver.PE.Imports;
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

    [Theory]
    [InlineData(MachineType.I386)]
    [InlineData(MachineType.Amd64)]
    public void RoundTripNativePE(MachineType machineType)
    {
        var image = PEImage.FromBytes(machineType switch
        {
            MachineType.I386 => Properties.Resources.NativeHelloWorldC_X86,
            MachineType.Amd64 => Properties.Resources.NativeHelloWorldC_X64,
            _ => throw new ArgumentOutOfRangeException(nameof(machineType))
        });

        var file = image.ToPEFile(new UnmanagedPEFileBuilder());

        _fixture.GetRunner<NativePERunner>().RebuildAndRun(
            file,
            $"NativeHelloWorldC.{machineType}.exe",
            "hello world!\n"
        );
    }

    [Theory]
    [InlineData(MachineType.I386)]
    [InlineData(MachineType.Amd64)]
    public void TrampolineImportsInCPE(MachineType machineType)
    {
        var image = PEImage.FromBytes(machineType switch
        {
            MachineType.I386 => Properties.Resources.NativeHelloWorldC_X86,
            MachineType.Amd64 => Properties.Resources.NativeHelloWorldC_X64,
            _ => throw new ArgumentOutOfRangeException(nameof(machineType))
        });

        var file = image.ToPEFile(new UnmanagedPEFileBuilder
        {
            TrampolineImports = true
        });

        _fixture.GetRunner<NativePERunner>().RebuildAndRun(
            file,
            $"NativeHelloWorldC.{machineType}.exe",
            "hello world!\n"
        );
    }

    [Theory]
    [InlineData(MachineType.I386)]
    [InlineData(MachineType.Amd64)]
    public void TrampolineImportsInCppPE(MachineType machineType)
    {
        var image = PEImage.FromBytes(machineType switch
        {
            MachineType.I386 => Properties.Resources.NativeHelloWorldCpp_X86,
            MachineType.Amd64 => Properties.Resources.NativeHelloWorldCpp_X64,
            _ => throw new ArgumentOutOfRangeException(nameof(machineType))
        });

        var file = image.ToPEFile(new UnmanagedPEFileBuilder
        {
            TrampolineImports = true,
            ImportedSymbolClassifier = new DelegatedSymbolClassifier(x => x.Name switch
            {
                "?cout@std@@3V?$basic_ostream@DU?$char_traits@D@std@@@1@A" => ImportedSymbolType.Data,
                _ => ImportedSymbolType.Function
            })
        });

        _fixture.GetRunner<NativePERunner>().RebuildAndRun(
            file,
            $"NativeHelloWorldCpp.{machineType}.exe",
            "Hello, world!\n"
        );
    }

    [Theory]
    [InlineData(MachineType.I386)]
    [InlineData(MachineType.Amd64)]
    public void ScrambleImportsNativePE(MachineType machineType)
    {
        // Load image.
        var image = PEImage.FromBytes(machineType switch
        {
            MachineType.I386 => Properties.Resources.NativeHelloWorldC_X86,
            MachineType.Amd64 => Properties.Resources.NativeHelloWorldC_X64,
            _ => throw new ArgumentOutOfRangeException(nameof(machineType))
        });

        // Reverse order of all imports
        foreach (var module in image.Imports)
        {
            var reversed = module.Symbols.Reverse().ToArray();
            module.Symbols.Clear();
            foreach (var symbol in reversed)
                module.Symbols.Add(symbol);
        }

        // Build with trampolines.
        var file = image.ToPEFile(new UnmanagedPEFileBuilder
        {
            TrampolineImports = true
        });

        _fixture.GetRunner<NativePERunner>().RebuildAndRun(
            file,
            $"NativeHelloWorldC.{machineType}.exe",
            "hello world!\n"
        );
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
    public void TrampolineVTableFixupsInMixedModeAssembly()
    {
        // Load image.
        var image = PEImage.FromBytes(Properties.Resources.MixedModeCallIntoNative);

        // Rebuild
        var file = image.ToPEFile(new UnmanagedPEFileBuilder
        {
            TrampolineVTableFixups = true
        });

        _fixture.GetRunner<NativePERunner>().RebuildAndRun(
            file,
            "MixedModeHelloWorld.exe",
            "Hello, World!\nResult: 3\n"
        );
    }

    [Fact]
    public void ScrambleVTableFixupsInMixedModeAssembly()
    {
        // Load image.
        var image = PEImage.FromBytes(Properties.Resources.MixedModeCallIntoNative);

        // Reverse all vtable tokens.
        foreach (var fixup in image.DotNetDirectory!.VTableFixups!)
        {
            var reversed = fixup.Tokens.Reverse().ToArray();
            fixup.Tokens.Clear();
            foreach (var symbol in reversed)
                fixup.Tokens.Add(symbol);
        }

        // Rebuild
        var file = image.ToPEFile(new UnmanagedPEFileBuilder
        {
            TrampolineVTableFixups = true
        });

        _fixture.GetRunner<NativePERunner>().RebuildAndRun(
            file,
            "MixedModeHelloWorld.exe",
            "Hello, World!\nResult: 3\n"
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
