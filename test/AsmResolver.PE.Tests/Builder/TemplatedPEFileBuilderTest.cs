using System;
using System.IO;
using System.Linq;
using AsmResolver.PE.Builder;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.Exports;
using AsmResolver.PE.File;
using AsmResolver.PE.Imports;
using AsmResolver.Tests;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.PE.Tests.Builder;

public class TemplatedPEFileBuilderTest : IClassFixture<TemporaryDirectoryFixture>
{
    private readonly TemporaryDirectoryFixture _fixture;

    public TemplatedPEFileBuilderTest(TemporaryDirectoryFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableTheory]
    [InlineData(MachineType.I386)]
    [InlineData(MachineType.Amd64)]
    [InlineData(MachineType.Arm64)]
    public void RoundTripNativePE(MachineType machineType)
    {
        XunitHelpers.SkipIfNotMachine(machineType);

        var image = PEImage.FromBytes(
            machineType switch
            {
                MachineType.I386 => Properties.Resources.NativeHelloWorldC_X86,
                MachineType.Amd64 => Properties.Resources.NativeHelloWorldC_X64,
                MachineType.Arm64 => Properties.Resources.NativeHelloWorldC_Arm64,
                _ => throw new ArgumentOutOfRangeException(nameof(machineType))
            },
            TestReaderParameters
        );

        var file = image.ToPEFile(new TemplatedPEFileBuilder());

        _fixture.GetRunner<NativePERunner>().RebuildAndRun(
            file,
            $"NativeHelloWorldC.{machineType}.exe",
            "hello world!\n"
        );
    }

    [SkippableTheory]
    [InlineData(MachineType.I386)]
    [InlineData(MachineType.Amd64)]
    [InlineData(MachineType.Arm64)]
    public void TrampolineImportsInCPE(MachineType machineType)
    {
        XunitHelpers.SkipIfNotMachine(machineType);

        var image = PEImage.FromBytes(
            machineType switch
            {
                MachineType.I386 => Properties.Resources.NativeHelloWorldC_X86,
                MachineType.Amd64 => Properties.Resources.NativeHelloWorldC_X64,
                MachineType.Arm64 => Properties.Resources.NativeHelloWorldC_Arm64,
                _ => throw new ArgumentOutOfRangeException(nameof(machineType))
            },
            TestReaderParameters
        );

        var file = image.ToPEFile(new TemplatedPEFileBuilder
        {
            TrampolineImports = true
        });

        _fixture.GetRunner<NativePERunner>().RebuildAndRun(
            file,
            $"NativeHelloWorldC.{machineType}.exe",
            "hello world!\n"
        );
    }

    [SkippableTheory]
    [InlineData(MachineType.I386)]
    [InlineData(MachineType.Amd64)]
    public void TrampolineImportsInCppPE(MachineType machineType)
    {
        XunitHelpers.SkipIfNotMachine(machineType);

        var image = PEImage.FromBytes(
            machineType switch
            {
                MachineType.I386 => Properties.Resources.NativeHelloWorldCpp_X86,
                MachineType.Amd64 => Properties.Resources.NativeHelloWorldCpp_X64,
                _ => throw new ArgumentOutOfRangeException(nameof(machineType))
            },
            TestReaderParameters
        );

        var file = image.ToPEFile(new TemplatedPEFileBuilder
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

    [SkippableTheory]
    [InlineData(MachineType.I386)]
    [InlineData(MachineType.Amd64)]
    [InlineData(MachineType.Arm64)]
    public void ScrambleImportsNativePE(MachineType machineType)
    {
        XunitHelpers.SkipIfNotMachine(machineType);

        // Load image.
        var image = PEImage.FromBytes(
            machineType switch
            {
                MachineType.I386 => Properties.Resources.NativeHelloWorldC_X86,
                MachineType.Amd64 => Properties.Resources.NativeHelloWorldC_X64,
                MachineType.Arm64 => Properties.Resources.NativeHelloWorldC_Arm64,
                _ => throw new ArgumentOutOfRangeException(nameof(machineType))
            },
            TestReaderParameters
        );

        // Reverse order of all imports
        foreach (var module in image.Imports)
        {
            var reversed = module.Symbols.Reverse().ToArray();
            module.Symbols.Clear();
            foreach (var symbol in reversed)
                module.Symbols.Add(symbol);
        }

        // Build with trampolines.
        var file = image.ToPEFile(new TemplatedPEFileBuilder
        {
            TrampolineImports = true
        });

        _fixture.GetRunner<NativePERunner>().RebuildAndRun(
            file,
            $"NativeHelloWorldC.{machineType}.exe",
            "hello world!\n"
        );
    }

    [SkippableTheory]
    [InlineData(MachineType.Amd64)]
    [InlineData(MachineType.Arm64)]
    public void RoundTripMixedModeAssembly(MachineType machineType)
    {
        XunitHelpers.SkipIfNotMachine(machineType);

        var image = PEImage.FromBytes(
            machineType switch
            {
                MachineType.Amd64 => Properties.Resources.MixedModeHelloWorld_X64,
                MachineType.Arm64 => Properties.Resources.MixedModeHelloWorld_Arm64,
                _ => throw new ArgumentOutOfRangeException(nameof(machineType))
            },
            TestReaderParameters
        );

        var file = image.ToPEFile(new TemplatedPEFileBuilder());

        _fixture.GetRunner<NativePERunner>().RebuildAndRun(
            file,
            "MixedModeHelloWorld.exe",
            "Hello\n1 + 2 = 3\n"
        );
    }

    [SkippableFact]
    public void TrampolineVTableFixupsInMixedModeAssembly()
    {
        XunitHelpers.SkipIfNotX86OrX64();

        // Load image.
        var image = PEImage.FromBytes(Properties.Resources.MixedModeCallIntoNative, TestReaderParameters);

        // Rebuild
        var file = image.ToPEFile(new TemplatedPEFileBuilder
        {
            TrampolineVTableFixups = true
        });

        _fixture.GetRunner<NativePERunner>().RebuildAndRun(
            file,
            "MixedModeHelloWorld.exe",
            "Hello, World!\nResult: 3\n"
        );
    }

    [SkippableFact]
    public void ScrambleVTableFixupsInMixedModeAssembly()
    {
        XunitHelpers.SkipIfNotX86OrX64();

        // Load image.
        var image = PEImage.FromBytes(Properties.Resources.MixedModeCallIntoNative, TestReaderParameters);

        // Reverse all vtable tokens.
        foreach (var fixup in image.DotNetDirectory!.VTableFixups!)
        {
            var reversed = fixup.Tokens.Reverse().ToArray();
            fixup.Tokens.Clear();
            foreach (var symbol in reversed)
                fixup.Tokens.Add(symbol);
        }

        // Rebuild
        var file = image.ToPEFile(new TemplatedPEFileBuilder
        {
            TrampolineVTableFixups = true
        });

        _fixture.GetRunner<NativePERunner>().RebuildAndRun(
            file,
            "MixedModeHelloWorld.exe",
            "Hello, World!\nResult: 3\n"
        );
    }

    [SkippableFact]
    public void AddMetadataToMixedModeAssembly()
    {
        XunitHelpers.SkipIfNotX86OrX64();

        const string name = "#Test";
        byte[] data = [1, 2, 3, 4];

        var image = PEImage.FromBytes(Properties.Resources.MixedModeHelloWorld_X64, TestReaderParameters);
        image.DotNetDirectory!.Metadata!.Streams.Add(new CustomMetadataStream(
            name, new DataSegment(data)
        ));

        var file = image.ToPEFile(new TemplatedPEFileBuilder());
        using var stream = new MemoryStream();
        file.Write(stream);

        var newImage = PEImage.FromStream(stream, TestReaderParameters);
        var metadataStream = Assert.IsAssignableFrom<CustomMetadataStream>(
            newImage.DotNetDirectory!.Metadata!.Streams.First(x => x.Name == name)
        );

        Assert.Equal(data, metadataStream.Contents.WriteIntoArray());

        _fixture.GetRunner<NativePERunner>().RebuildAndRun(
            file,
            "MixedModeHelloWorld.exe",
            [
                $"Unknown heap type: {name}\n\nHello\n1 + 2 = 3\n",
                "Hello\n1 + 2 = 3\n"
            ]
        );
    }

    [Fact]
    public void AddExportToExistingDirectory()
    {
        var image = PEImage.FromBytes(Properties.Resources.SimpleDll_Exports, TestReaderParameters);
        image.Exports!.Entries.Add(new ExportedSymbol(new VirtualAddress(0x13371337), "MySymbol"));

        var file = image.ToPEFile(new TemplatedPEFileBuilder());
        using var stream = new MemoryStream();
        file.Write(stream);

        var newImage = PEImage.FromStream(stream, TestReaderParameters);
        Assert.NotNull(newImage.Exports);
        Assert.Equal(image.Exports.Entries.Select(x => x.Name), newImage.Exports.Entries.Select(x => x.Name));
    }

    [Fact]
    public void PatchLoadConfigurationSehTable()
    {
        var image = PEImage.FromBytes(Properties.Resources.ControlFlowGuardTest_X86, TestReaderParameters);
        var config = image.LoadConfiguration!;
        var originalRva = config.SEHandlerTable.Rva;
        config.SEHandlerTable[0] = new VirtualAddress(0x1234);
        var expected = config.SEHandlerTable.Select(entry => entry.Rva).ToArray();

        var file = image.ToPEFile(new TemplatedPEFileBuilder());
        using var stream = new MemoryStream();
        file.Write(stream);

        var newImage = PEImage.FromStream(stream, TestReaderParameters);
        Assert.NotNull(newImage.LoadConfiguration);
        Assert.Equal(originalRva, newImage.LoadConfiguration.SEHandlerTable.Rva);
        Assert.Equal(expected, newImage.LoadConfiguration.SEHandlerTable.Select(entry => entry.Rva));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PatchLoadConfigurationSecurityCookie(bool is32Bit)
    {
        var image = PEImage.FromBytes(
            is32Bit ? Properties.Resources.ControlFlowGuardTest_X86 : Properties.Resources.ControlFlowGuardTest_X64,
            TestReaderParameters);
        var config = image.LoadConfiguration!;
        var originalRva = config.SecurityCookie!.Rva;
        var data = Enumerable.Repeat((byte) 0xA5, is32Bit ? 4 : 8).ToArray();
        config.SecurityCookie = new DataSegment(data);

        var file = image.ToPEFile(new TemplatedPEFileBuilder());
        using var stream = new MemoryStream();
        file.Write(stream);

        var newImage = PEImage.FromStream(stream, TestReaderParameters);
        Assert.NotNull(newImage.LoadConfiguration?.SecurityCookie);
        Assert.Equal(originalRva, newImage.LoadConfiguration.SecurityCookie.Rva);
        Assert.Equal(data, newImage.LoadConfiguration.SecurityCookie.WriteIntoArray());
    }

    [Fact]
    public void PatchTlsDirectoryPreservesItsSize()
    {
        var image = PEImage.FromBytes(Properties.Resources.TlsTest_x64, TestReaderParameters);
        var expectedSize = image.TlsDirectory!.GetPhysicalSize();

        var file = image.ToPEFile(new TemplatedPEFileBuilder());

        Assert.Equal(expectedSize, file.OptionalHeader.DataDirectories[(int) DataDirectoryIndex.TlsDirectory].Size);
    }

    [Fact]
    public void RoundTripTlsShouldPreserveContents()
    {
        // https://github.com/Washi1337/AsmResolver/issues/706

        var image = PEImage.FromBytes(Properties.Resources.TlsTest_x64, TestReaderParameters);
        Assert.NotNull(image.TlsDirectory);

        var file = image.ToPEFile(new TemplatedPEFileBuilder());
        var runner = _fixture.GetRunner<PERunner>();
        string path = runner.Rebuild(file, "TlsTest.exe");

        var newImage = PEImage.FromFile(path, TestReaderParameters);
        Assert.NotNull(newImage.TlsDirectory);
        Assert.Equal(
            image.TlsDirectory.TemplateData?.ToArray(),
            newImage.TlsDirectory.TemplateData?.ToArray()
        );
        Assert.Equal(
            image.TlsDirectory.CallbackFunctions.Select(x => x.Rva),
            newImage.TlsDirectory.CallbackFunctions.Select(x => x.Rva)
        );
        Assert.Equal(
            image.Relocations.Select(x => x.Location.Rva),
            newImage.Relocations.Select(x => x.Location.Rva)
        );

        string output = runner.RunAndCaptureOutput(path, ["1"]);
        Assert.Contains("TLS callback 1 (Reason: 1)", output);
        Assert.Contains("TLS callback 1 (Reason: 2)", output);
        Assert.Contains("_threadLocalInt = 305419896", output);
        Assert.Contains("_threadLocalInt = 305419897", output);
        Assert.Contains("_threadLocalArray = Hello World!", output);
        Assert.Contains("TLS callback 1 (Reason: 3)", output);
        Assert.Contains("Done", output);
    }
}
