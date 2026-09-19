using System.Linq;
using AsmResolver.PE.Configuration;
using Xunit;

namespace AsmResolver.PE.Tests.Configuration;

public class LoadConfigurationTest
{
    [Fact]
    public void SecurityCookie32Bit()
    {
        var image = PEImage.FromBytes(Properties.Resources.ControlFlowGuardTest_X86);
        var config = image.LoadConfiguration;

        Assert.NotNull(config);
        Assert.NotNull(config.SecurityCookie);
        Assert.Equal(4u, config.SecurityCookie.GetPhysicalSize());
    }

    [Fact]
    public void SecurityCookie64Bit()
    {
        var image = PEImage.FromBytes(Properties.Resources.ControlFlowGuardTest_X64);
        var config = image.LoadConfiguration;

        Assert.NotNull(config);
        Assert.NotNull(config.SecurityCookie);
        Assert.Equal(8u, config.SecurityCookie.GetPhysicalSize());
    }

    [Fact]
    public void SehHandlerTable32Bit()
    {
        var image = PEImage.FromBytes(Properties.Resources.ControlFlowGuardTest_X86);
        var config = image.LoadConfiguration;

        Assert.NotNull(config);
        Assert.Equal([0x00004320u, 0x00004D50u], config.SEHandlerTable.Select(x => x.Rva));
    }

    [Fact]
    public void GuardFunctionTable32Bit()
    {
        var image = PEImage.FromBytes(Properties.Resources.ControlFlowGuardTest_X86);
        var config = image.LoadConfiguration;

        Assert.NotNull(config?.GuardCFFunctionTable);
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

    [Fact]
    public void GuardFunctionTable32BitWithFlags()
    {
        var image = PEImage.FromBytes(Properties.Resources.ControlFlowGuardTest_X86_Flags);
        var config = image.LoadConfiguration;

        Assert.NotNull(config?.GuardCFFunctionTable);
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

    [Fact]
    public void GuardFunctionTable64Bit()
    {
        var image = PEImage.FromBytes(Properties.Resources.ControlFlowGuardTest_X64);
        var config = image.LoadConfiguration;

        Assert.NotNull(config?.GuardCFFunctionTable);
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
}
