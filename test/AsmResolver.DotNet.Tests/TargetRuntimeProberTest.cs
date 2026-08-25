using AsmResolver.PE;
using Xunit;

namespace AsmResolver.DotNet.Tests;

public class TargetRuntimeProberTest
{
    [Fact]
    public void DetectTargetNetFramework40()
    {
        var image = PEImage.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters.PEReaderParameters);

        Assert.True(TargetRuntimeProber.TryGetLikelyTargetRuntime(image, out var targetRuntime));
        Assert.True(targetRuntime.IsNetFramework);
        Assert.Contains(DotNetRuntimeInfo.NetFrameworkName, targetRuntime.Name);
        Assert.Equal(4, targetRuntime.Version.Major);
        Assert.Equal(0, targetRuntime.Version.Minor);
    }

    [Fact]
    public void DetectTargetNetCore()
    {
        var image = PEImage.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters.PEReaderParameters);

        Assert.True(TargetRuntimeProber.TryGetLikelyTargetRuntime(image, out var targetRuntime));
        Assert.True(targetRuntime.IsNetCoreApp);
        Assert.Contains(DotNetRuntimeInfo.NetCoreAppName, targetRuntime.Name);
        Assert.Equal(2, targetRuntime.Version.Major);
        Assert.Equal(2, targetRuntime.Version.Minor);
    }

    [Fact]
    public void DetectTargetSilverlight5()
    {
        var image = PEImage.FromBytes(
            Properties.Resources.HelloWorld_Silverlight5,
            TestReaderParameters.PEReaderParameters
        );

        Assert.True(TargetRuntimeProber.TryGetLikelyTargetRuntime(image, out var targetRuntime));
        Assert.Equal(DotNetRuntimeInfo.Silverlight(5, 0), targetRuntime);
    }

    [Fact]
    public void DetectTargetStandard()
    {
        var image = PEImage.FromFile(typeof(TestCases.Types.Class).Assembly.Location, TestReaderParameters.PEReaderParameters);

        Assert.True(TargetRuntimeProber.TryGetLikelyTargetRuntime(image, out var targetRuntime));
        Assert.True(targetRuntime.IsNetStandard);
        Assert.Contains(DotNetRuntimeInfo.NetStandardName, targetRuntime.Name);
        Assert.Equal(2, targetRuntime.Version.Major);
    }
}
