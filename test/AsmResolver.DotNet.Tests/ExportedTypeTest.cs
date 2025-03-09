using Xunit;

namespace AsmResolver.DotNet.Tests;

public class ExportedTypeTest
{
    [Fact]
    public void ReadName()
    {
        var module = ModuleDefinition.FromBytes(Properties.Resources.ForwarderLibrary, TestReaderParameters);
        var type = module.ExportedTypes[0];
        Assert.Equal("IMyModel", type.Name);
    }

    [Fact]
    public void ReadAssemblyImplementation()
    {
        var module = ModuleDefinition.FromBytes(Properties.Resources.ForwarderLibrary, TestReaderParameters);
        var type = module.ExportedTypes[0];
        var implementation = Assert.IsAssignableFrom<AssemblyReference>(type.Implementation);
        Assert.Equal("ActualLibrary", implementation.Name);
    }

    [Fact]
    public void ReadDeclaringTypeImplementation()
    {
        var module = ModuleDefinition.FromBytes(Properties.Resources.ForwarderLibrary, TestReaderParameters);
        var type = module.ExportedTypes[2];
        Assert.Equal("MyNestedClass", type.Name);
        var implementation = Assert.IsAssignableFrom<ExportedType>(type.Implementation);
        Assert.Equal("MyClass", implementation.Name);
    }

    [Fact]
    public void GetFullNameNoNamespace()
    {
        var exportedType = new ExportedType(
            KnownCorLibs.SystemPrivateCoreLib_v8_0_0_0,
            null,
            "SomeType"
        );

        Assert.Equal("SomeType", exportedType.FullName);
    }

    [Fact]
    public void GetFullName()
    {
        var exportedType = new ExportedType(
            KnownCorLibs.SystemPrivateCoreLib_v8_0_0_0,
            "System.Collections.Generic",
            "List`1"
        );

        Assert.Equal("System.Collections.Generic.List`1", exportedType.FullName);
    }

    [Fact]
    public void GetFullNameNested()
    {
        var exportedType = new ExportedType(
            new ExportedType(
                KnownCorLibs.SystemPrivateCoreLib_v8_0_0_0,
                "System.Collections.Generic",
                "List`1"
            ),
            null,
            "Enumerator"
        );

        Assert.Equal("System.Collections.Generic.List`1+Enumerator", exportedType.FullName);
    }
}
