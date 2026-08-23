using System;
using System.IO;
using System.Reflection;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE;
using Xunit;

namespace AsmResolver.DotNet.Tests;

public class SilverlightRuntimeTest
{
    public static TheoryData<AssemblyReference, DotNetRuntimeInfo> SilverlightCorLibs => new()
    {
        { KnownCorLibs.MsCorLib_v2_0_5_0, DotNetRuntimeInfo.Silverlight(4, 0) },
        { KnownCorLibs.MsCorLib_v5_0_5_0, DotNetRuntimeInfo.Silverlight(5, 0) },
    };

    public static TheoryData<AssemblyReference, DotNetRuntimeInfo> SilverlightCorLibHeuristics => new()
    {
        { KnownCorLibs.MsCorLib_v2_0_5_0, DotNetRuntimeInfo.NetFramework(2, 0) },
        { KnownCorLibs.MsCorLib_v5_0_5_0, DotNetRuntimeInfo.NetCoreApp(5, 0) },
    };

    [Theory]
    [InlineData("Silverlight,Version=v4.0", 4, 0)]
    [InlineData("Silverlight,Version=v5.0", 5, 0)]
    public void ParseSilverlightFrameworkName(string name, int major, int minor)
    {
        var runtime = DotNetRuntimeInfo.Parse(name);

        Assert.True(runtime.IsSilverlight);
        Assert.Equal(DotNetRuntimeInfo.SilverlightName, runtime.Name);
        Assert.Equal(new Version(major, minor), runtime.Version);
    }

    [Theory]
    [InlineData(4, 0, "mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e")]
    [InlineData(5, 0, "mscorlib, Version=5.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e")]
    public void SelectSilverlightCorLib(int major, int minor, string expectedCorLibName)
    {
        var runtime = DotNetRuntimeInfo.Silverlight(major, minor);
        var expectedCorLib = new ReflectionAssemblyDescriptor(new AssemblyName(expectedCorLibName));

        Assert.Equal<AssemblyDescriptor>(expectedCorLib, runtime.GetDefaultCorLib(), SignatureComparer.Default);
        Assert.Equal<AssemblyDescriptor>(expectedCorLib, runtime.GetAssumedImplCorLib()!, SignatureComparer.Default);
    }

    [Theory]
    [InlineData(3, 0)]
    [InlineData(6, 0)]
    public void UnsupportedSilverlightVersionHasNoKnownCorLib(int major, int minor)
    {
        var runtime = DotNetRuntimeInfo.Silverlight(major, minor);

        Assert.Throws<ArgumentException>(runtime.GetDefaultCorLib);
        Assert.Throws<ArgumentException>(runtime.GetAssumedImplCorLib);
    }

    [Theory]
    [MemberData(nameof(SilverlightCorLibHeuristics))]
    public void CorLibIdentityAloneDoesNotClassifyAsSilverlight(
        AssemblyReference corLib,
        DotNetRuntimeInfo expectedRuntime)
    {
        var image = CreateImage(corLib);
        var detectedRuntime = GetTargetRuntime(image);
        var module = new ModuleDefinition("AmbiguousCorLib.dll", corLib);

        Assert.Equal(expectedRuntime, detectedRuntime);
        Assert.False(detectedRuntime.IsSilverlight);
        Assert.False(module.CorLibTypeFactory.ExtractDotNetRuntimeInfo().IsSilverlight);
    }

    [Fact]
    public void RetargetablePortableCorLibIdentityDoesNotClassifyAsSilverlight()
    {
        var corLib = new AssemblyReference(KnownCorLibs.MsCorLib_v2_0_5_0)
        {
            IsRetargetable = true,
        };

        var detectedRuntime = GetTargetRuntime(CreateImage(corLib));

        Assert.Equal(DotNetRuntimeInfo.NetFramework(2, 0), detectedRuntime);
        Assert.False(detectedRuntime.IsSilverlight);
    }

    [Theory]
    [InlineData(4, 0)]
    [InlineData(5, 0)]
    public void SupportedSilverlightTargetFrameworkOverridesCorLibHeuristic(int major, int minor)
    {
        var runtime = DotNetRuntimeInfo.Silverlight(major, minor);
        var image = CreateImage(GetSilverlightCorLib(major), runtime.ToString());

        Assert.Equal(runtime, GetTargetRuntime(image));
    }

    [Fact]
    public void MultipleSilverlightTargetFrameworkAttributesPreferHighestSupportedVersion()
    {
        var image = CreateImage(
            KnownCorLibs.MsCorLib_v4_0_0_0,
            "Silverlight,Version=v5.0",
            "Silverlight,Version=v4.0"
        );

        Assert.Equal(DotNetRuntimeInfo.Silverlight(5, 0), GetTargetRuntime(image));
    }

    [Fact]
    public void NonSilverlightTargetFrameworkPreservesExistingCrossRuntimeHeuristic()
    {
        var image = CreateImage(KnownCorLibs.MsCorLib_v4_0_0_0, ".NETCoreApp,Version=v8.0");

        Assert.Equal(DotNetRuntimeInfo.NetFramework(4, 0), GetTargetRuntime(image));
    }

    [Theory]
    [InlineData(4, 0, "Profile=")]
    [InlineData(5, 0, "profile=   ")]
    [InlineData(4, 0, "Profile=,profile=  ")]
    public void EmptySilverlightProfileIsTreatedAsUnprofiled(int major, int minor, string profile)
    {
        var expectedRuntime = DotNetRuntimeInfo.Silverlight(major, minor);
        var image = CreateImage(
            KnownCorLibs.MsCorLib_v4_0_0_0,
            $"Silverlight,Version=v{major}.{minor},{profile}"
        );

        Assert.Equal(expectedRuntime, GetTargetRuntime(image));
    }

    [Theory]
    [InlineData(3, 0)]
    [InlineData(6, 0)]
    public void UnsupportedSilverlightTargetFrameworkDoesNotOverrideCorLibHeuristic(int major, int minor)
    {
        var image = CreateImage(
            KnownCorLibs.MsCorLib_v4_0_0_0,
            $"Silverlight,Version=v{major}.{minor}"
        );

        Assert.Equal(DotNetRuntimeInfo.NetFramework(4, 0), GetTargetRuntime(image));
    }

    [Theory]
    [InlineData(3, 0)]
    [InlineData(6, 0)]
    public void UnsupportedSilverlightRuntimeContextIsRejected(int major, int minor)
    {
        var runtime = DotNetRuntimeInfo.Silverlight(major, minor);

        Assert.Throws<ArgumentException>(() => new RuntimeContext(runtime));
    }

    [Theory]
    [MemberData(nameof(SilverlightCorLibs))]
    public void RecognizeSilverlightCorLibForRuntime(AssemblyReference corLib, DotNetRuntimeInfo runtime)
    {
        Assert.True(corLib.IsReferenceCorLib(runtime));
        Assert.True(corLib.IsImplementationCorLib(runtime));
    }

    [Theory]
    [InlineData(4, 0)]
    [InlineData(5, 0)]
    public void ExplicitSilverlightModuleRuntimeIsPreserved(int major, int minor)
    {
        var runtime = DotNetRuntimeInfo.Silverlight(major, minor);
        var module = new ModuleDefinition("Silverlight.dll", runtime);
        var actualCorLib = module.CorLibTypeFactory.CorLibScope.GetAssembly();

        Assert.Equal(runtime, module.OriginalTargetRuntime);
        Assert.Equal(runtime.GetDefaultCorLib(), actualCorLib!, SignatureComparer.Default);
    }

    [Theory]
    [InlineData(4, 0)]
    [InlineData(5, 0)]
    public void RuntimeContextUsesSilverlightResolver(int major, int minor)
    {
        var runtime = DotNetRuntimeInfo.Silverlight(major, minor);
        var context = new RuntimeContext(runtime);

        Assert.Equal(runtime, context.TargetRuntime);
        Assert.IsType<SilverlightAssemblyResolver>(context.AssemblyResolver);
        Assert.Equal(runtime.GetAssumedImplCorLib()!, context.RuntimeCorLib!, SignatureComparer.Default);
    }

    [Theory]
    [MemberData(nameof(SilverlightCorLibs))]
    public void RuntimeContextFromImageUsesSilverlightRuntime(
        AssemblyReference corLib,
        DotNetRuntimeInfo expectedRuntime)
    {
        var context = new RuntimeContext(CreateImage(corLib, expectedRuntime.ToString()));

        Assert.Equal(expectedRuntime, context.TargetRuntime);
        Assert.IsType<SilverlightAssemblyResolver>(context.AssemblyResolver);
        Assert.Equal(expectedRuntime.GetAssumedImplCorLib()!, context.RuntimeCorLib!, SignatureComparer.Default);
    }

    [Theory]
    [MemberData(nameof(SilverlightCorLibs))]
    public void LoadingSilverlightImageCreatesSilverlightRuntimeContext(
        AssemblyReference corLib,
        DotNetRuntimeInfo expectedRuntime)
    {
        var image = CreateImage(corLib, expectedRuntime.ToString());
        var module = ModuleDefinition.FromImage(image, TestReaderParameters);
        var context = module.RuntimeContext;

        Assert.Equal(expectedRuntime, module.OriginalTargetRuntime);
        Assert.NotNull(context);
        Assert.Equal(expectedRuntime, context.TargetRuntime);
        Assert.IsType<SilverlightAssemblyResolver>(context.AssemblyResolver);
        Assert.Same(context, module.Assembly!.RuntimeContext);
    }

    private static AssemblyReference GetSilverlightCorLib(int major)
    {
        return major == 4
            ? KnownCorLibs.MsCorLib_v2_0_5_0
            : KnownCorLibs.MsCorLib_v5_0_5_0;
    }

    private static DotNetRuntimeInfo GetTargetRuntime(PEImage image)
    {
        Assert.True(TargetRuntimeProber.TryGetLikelyTargetRuntime(image, out var runtime));

        return runtime;
    }

    private static PEImage CreateImage(AssemblyReference? corLib, params string[] targetFrameworks)
    {
        var assembly = new AssemblyDefinition("SilverlightTest", new Version(1, 0, 0, 0));
        var module = new ModuleDefinition("SilverlightTest.dll", corLib);
        assembly.Modules.Add(module);

        for (int i = 0; i < targetFrameworks.Length; i++)
            AddTargetFrameworkAttribute(assembly, module, targetFrameworks[i]);

        using var stream = new MemoryStream();
        assembly.WriteManifest(stream);

        return PEImage.FromBytes(stream.ToArray(), TestReaderParameters.PEReaderParameters);
    }

    private static void AddTargetFrameworkAttribute(
        AssemblyDefinition assembly,
        ModuleDefinition module,
        string targetFramework)
    {
        var attributeType = module.CorLibTypeFactory.CorLibScope.CreateTypeReference(
            "System.Runtime.Versioning",
            "TargetFrameworkAttribute"
        );

        var constructor = new MemberReference(
            attributeType,
            ".ctor",
            MethodSignature.CreateInstance(module.CorLibTypeFactory.Void, [module.CorLibTypeFactory.String])
        );

        var signature = new CustomAttributeSignature(
            new CustomAttributeArgument(module.CorLibTypeFactory.String, targetFramework)
        );

        assembly.CustomAttributes.Add(new CustomAttribute(constructor, signature));
    }
}
