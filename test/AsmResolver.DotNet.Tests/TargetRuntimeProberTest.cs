using System;
using System.IO;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata.Tables;
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

    [Fact]
    public void IgnoreUnsupportedCorLibReference()
    {
        // https://github.com/Washi1337/AsmResolver/issues/790

        var assembly = new AssemblyDefinition("TestAssembly", new Version(1, 0, 0, 0));
        var module = new ModuleDefinition("TestAssembly.dll", KnownCorLibs.MsCorLib_v4_0_0_0);
        assembly.Modules.Add(module);
        var invalidCorLib = new AssemblyReference(
            "mscorlib",
            new Version(ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue)
        );
        module.GetOrCreateModuleType().Fields.Add(new FieldDefinition(
            "Field",
            FieldAttributes.Static,
            invalidCorLib.CreateTypeReference("", "TestType").ToTypeSignature(false)
        ));

        using var stream = new MemoryStream();
        assembly.WriteManifest(stream);

        var image = PEImage.FromBytes(stream.ToArray(), TestReaderParameters.PEReaderParameters);

        Assert.True(TargetRuntimeProber.TryGetLikelyTargetRuntime(image, out var targetRuntime));
        Assert.Equal(DotNetRuntimeInfo.NetFramework(4, 0), targetRuntime);

        var newModule = ModuleDefinition.FromImage(image, TestReaderParameters);

        Assert.Equal(targetRuntime, newModule.OriginalTargetRuntime);
        Assert.Equal<AssemblyDescriptor>(
            KnownCorLibs.MsCorLib_v4_0_0_0,
            newModule.CorLibTypeFactory.CorLibScope.GetAssembly()!,
            SignatureComparer.Default
        );
    }
}
