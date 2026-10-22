using System.IO;
using System.Linq;
using AsmResolver.PE.Builder;
using AsmResolver.PE.Configuration;
using Xunit;

namespace AsmResolver.PE.Tests.Configuration;

public class LoadConfigurationTest
{
    private static LoadConfiguration GetLoadConfiguration(byte[] imageBytes, bool rebuild)
    {
        var image = PEImage.FromBytes(imageBytes, TestReaderParameters);
        if (rebuild)
        {
            using var stream = new MemoryStream();
            new TemplatedPEFileBuilder().CreateFile(image).Write(stream);
            stream.Position = 0;
            image = PEImage.FromBytes(stream.ToArray());
        }

        Assert.NotNull(image.LoadConfiguration);
        return image.LoadConfiguration;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SecurityCookie32Bit(bool rebuild)
    {
        var config = GetLoadConfiguration(Properties.Resources.ControlFlowGuardTest_X86, rebuild);

        Assert.NotNull(config.SecurityCookie);
        Assert.Equal(4u, config.SecurityCookie.GetPhysicalSize());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SecurityCookie64Bit(bool rebuild)
    {
        var config = GetLoadConfiguration(Properties.Resources.ControlFlowGuardTest_X64, rebuild);

        Assert.NotNull(config.SecurityCookie);
        Assert.Equal(8u, config.SecurityCookie.GetPhysicalSize());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SehHandlerTable32Bit(bool rebuild)
    {
        var config = GetLoadConfiguration(Properties.Resources.ControlFlowGuardTest_X86, rebuild);

        Assert.Equal([0x00004320u, 0x00004D50u], config.SEHandlerTable.Select(x => x.Rva));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GuardFunctionTable32Bit(bool rebuild)
    {
        var config = GetLoadConfiguration(Properties.Resources.ControlFlowGuardTest_X86, rebuild);

        Assert.Equal(
            [
                0x00001130u,
                0x00001220u,
                0x00001280u,
                0x000013E0u,
                0x000014D0u,
                0x00001550u,
                0x000015A0u,
                0x000015C0u,
                0x00001650u,
                0x00001770u,
                0x000027A0u,
                0x00002890u,
                0x000028A0u,
                0x00005420u,
                0x00005430u,
            ],
            config.GuardCFFunctionTable.Select(x => x.EntryPoint.Rva)
        );
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GuardFunctionTable32BitWithFlags(bool rebuild)
    {
        var config = GetLoadConfiguration(Properties.Resources.ControlFlowGuardTest_X86_Flags, rebuild);

        Assert.Equal(
            [
                (0x00001130u, 0),
                (0x00001220u, ControlFlowGuardFunctionFlags.FidSuppressed),
                (0x00001280u, 0),
                (0x000013E0u, 0),
                (0x000014D0u, 0),
                (0x00001550u, 0),
                (0x000015A0u, 0),
                (0x000015C0u, 0),
                (0x00001650u, 0),
                (0x00001770u, 0),
                (0x000027A0u, 0),
                (0x00002890u, 0),
                (0x000028A0u, 0),
                (0x00005420u, 0),
                (0x00005430u, 0),
            ],
            config.GuardCFFunctionTable.Select(x => (x.EntryPoint.Rva, x.Flags))
        );
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GuardFunctionTable64Bit(bool rebuild)
    {
        var config = GetLoadConfiguration(Properties.Resources.ControlFlowGuardTest_X64, rebuild);

        Assert.Equal(
            [
                0x00001070u,
                0x000010E0u,
                0x00001170u,
                0x00001210u,
                0x00001350u,
                0x000015B0u,
                0x00001670u,
                0x00001770u,
                0x000017E0u,
                0x00001800u,
                0x00003410u,
                0x00003500u,
                0x00003520u,
                0x00006A30u,
                0x00006A40u,
                0x00006BE0u,
            ],
            config.GuardCFFunctionTable.Select(x => x.EntryPoint.Rva)
        );
    }

    [Fact]
    public void HasField32BitAlignedSize()
    {
        var config = new LoadConfiguration(
            size: 0x80,
            is32Bit: true
        );

        Assert.True(config.HasField(LoadConfigurationField.ChpeMetadataPointer));
        Assert.False(config.HasField(LoadConfigurationField.GuardRFFailureRoutine));
    }

    [Fact]
    public void HasField32BitUnalignedSize()
    {
        var config = new LoadConfiguration(
            size: 0x80 + 3,
            is32Bit: true
        );

        Assert.True(config.HasField(LoadConfigurationField.ChpeMetadataPointer));
        Assert.False(config.HasField(LoadConfigurationField.GuardRFFailureRoutine));
    }

    [Fact]
    public void HasField64BitAlignedSize()
    {
        var config = new LoadConfiguration(
            size: 0xd0,
            is32Bit: false
        );

        Assert.True(config.HasField(LoadConfigurationField.ChpeMetadataPointer));
        Assert.False(config.HasField(LoadConfigurationField.GuardRFFailureRoutine));
    }

    [Fact]
    public void HasField64BitUnalignedSize()
    {
        var config = new LoadConfiguration(
            size: 0xd0 + 3,
            is32Bit: false
        );

        Assert.True(config.HasField(LoadConfigurationField.ChpeMetadataPointer));
        Assert.False(config.HasField(LoadConfigurationField.GuardRFFailureRoutine));
    }

    [Theory]
    [InlineData(0x40, LoadConfigurationField.SecurityCookie, true)]
    [InlineData(0xC4u, LoadConfigurationField.UmaFunctionPointers, true)]
    [InlineData(0x60, LoadConfigurationField.SecurityCookie, false)]
    [InlineData(0x148, LoadConfigurationField.UmaFunctionPointers, false)]
    public void LastIncludedFieldShouldAllocateAppropriateSize(uint expected, LoadConfigurationField field, bool is32Bit)
    {
        Assert.Equal(expected, new LoadConfiguration(field, is32Bit).Size);
    }

    [Fact]
    public void ConvertFrom32To64BitsShouldAdjustSize()
    {
        var config = new LoadConfiguration(
            lastIncludedField: LoadConfigurationField.SecurityCookie,
            is32Bit: true
        );

        Assert.Equal(0x40u, config.Size);
        config.UpdateOffsets(new RelocationParameters(0x1400_0000, 0x200, 0x2000, false));
        Assert.Equal(0x60u, config.Size);
    }

    [Fact]
    public void ConvertFrom32To64BitsWithPaddingShouldAdjustSize()
    {
        var config = new LoadConfiguration(
            lastIncludedField: LoadConfigurationField.SecurityCookie,
            is32Bit: true
        );

        Assert.Equal(0x40u, config.Size);
        config.Size += 3u;

        config.UpdateOffsets(new RelocationParameters(0x1400_0000, 0x200, 0x2000, false));

        Assert.Equal(0x63u, config.Size);
    }

    [Fact]
    public void ConvertFrom64To32BitsShouldAdjustSize()
    {
        var config = new LoadConfiguration(
            lastIncludedField: LoadConfigurationField.SecurityCookie,
            is32Bit: false
        );

        Assert.Equal(0x60u, config.Size);
        config.UpdateOffsets(new RelocationParameters(0x1400_0000, 0x200, 0x2000, true));
        Assert.Equal(0x40u, config.Size);
    }

    [Fact]
    public void ConvertFrom64To32BitsWithPaddingShouldAdjustSize()
    {
        var config = new LoadConfiguration(
            lastIncludedField: LoadConfigurationField.SecurityCookie,
            is32Bit: false
        );

        Assert.Equal(0x60u, config.Size);
        config.Size += 3u;

        config.UpdateOffsets(new RelocationParameters(0x1400_0000, 0x200, 0x2000, true));

        Assert.Equal(0x43u, config.Size);
    }

    [Fact]
    public void IncludeFieldInSmallerDirectoryShouldAdjustSize()
    {
        var config = new LoadConfiguration(
            lastIncludedField: LoadConfigurationField.SecurityCookie,
            is32Bit: false
        );

        Assert.Equal(0x60u, config.Size);
        Assert.False(config.HasField(LoadConfigurationField.GuardCFFunctionTable));

        config.EnsureFieldIsIncluded(LoadConfigurationField.GuardCFFunctionTable);

        Assert.Equal(0x88u, config.Size);
        Assert.True(config.HasField(LoadConfigurationField.GuardCFFunctionTable));
    }

    [Fact]
    public void IncludeFieldInLargerDirectoryShouldRetainSize()
    {
        var config = new LoadConfiguration(
            lastIncludedField: LoadConfigurationField.UmaFunctionPointers,
            is32Bit: false
        );

        Assert.Equal(0x148u, config.Size);
        Assert.True(config.HasField(LoadConfigurationField.GuardCFFunctionTable));

        config.EnsureFieldIsIncluded(LoadConfigurationField.GuardCFFunctionTable);

        Assert.Equal(0x148u, config.Size);
        Assert.True(config.HasField(LoadConfigurationField.GuardCFFunctionTable));
    }

    [Fact]
    public void ResizeLargerDirectoryThanNecessaryShouldTruncate()
    {
        var config = new LoadConfiguration(
            lastIncludedField: LoadConfigurationField.UmaFunctionPointers,
            is32Bit: false
        );

        Assert.Equal(0x148u, config.Size);
        Assert.True(config.HasField(LoadConfigurationField.SecurityCookie));
        Assert.True(config.HasField(LoadConfigurationField.UmaFunctionPointers));

        config.Resize(LoadConfigurationField.SecurityCookie);

        Assert.Equal(0x60u, config.Size);
        Assert.True(config.HasField(LoadConfigurationField.SecurityCookie));
        Assert.False(config.HasField(LoadConfigurationField.UmaFunctionPointers));
    }
}
