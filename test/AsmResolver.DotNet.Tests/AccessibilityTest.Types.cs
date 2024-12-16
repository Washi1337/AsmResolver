using System;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests;

public partial class AccessibilityTest
{
    private static ModuleDefinition CreateDummyModule(string name)
    {
        var module = new ModuleDefinition(name + ".dll");
        var assembly = new AssemblyDefinition(name, new Version(1, 0));
        assembly.Modules.Add(module);
        return module;
    }

    private static TypeDefinition AddTopLevelType(ModuleDefinition module, string name, TypeAttributes attributes, ITypeDefOrRef baseType = null)
    {
        var type = new TypeDefinition(null, name, attributes, baseType ?? module.CorLibTypeFactory.Object.Type);
        module.TopLevelTypes.Add(type);
        return type;
    }

    private static TypeDefinition AddNestedType(TypeDefinition declaringType, string name, TypeAttributes attributes, ITypeDefOrRef baseType = null)
    {
        var type = new TypeDefinition(null, name, attributes, baseType ?? declaringType.Module!.CorLibTypeFactory.Object.Type);
        declaringType.NestedTypes.Add(type);
        return type;
    }

    [Fact]
    public void TypeCanAccessPublicTypeSameAssembly()
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);
        var type2 = AddTopLevelType(module1, "Type2", TypeAttributes.Public);

        Assert.True(type2.IsAccessibleFromType(type1));
        Assert.True(type1.IsAccessibleFromType(type2));
    }

    [Fact]
    public void TypeCanAccessNonPublicTypeSameAssembly()
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);
        var type2 = AddTopLevelType(module1, "Type2", TypeAttributes.NotPublic);

        Assert.True(type2.IsAccessibleFromType(type1));
        Assert.True(type1.IsAccessibleFromType(type2));
    }

    [Fact]
    public void TypeCanAccessPublicTypeDifferentAssembly()
    {
        var module1 = CreateDummyModule("Module1");
        var module2 = CreateDummyModule("Module2");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);
        var type2 = AddTopLevelType(module2, "Type2", TypeAttributes.Public);

        Assert.True(type2.IsAccessibleFromType(type1));
        Assert.True(type1.IsAccessibleFromType(type2));
    }

    [Fact]
    public void TypeCannotAccessNonPublicTypeDifferentAssembly()
    {
        var module1 = CreateDummyModule("Module1");
        var module2 = CreateDummyModule("Module2");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);
        var type2 = AddTopLevelType(module2, "Type2", TypeAttributes.NotPublic);

        Assert.False(type2.IsAccessibleFromType(type1));
        Assert.True(type1.IsAccessibleFromType(type2));
    }

    [Theory]
    [InlineData(TypeAttributes.NestedPublic)]
    [InlineData(TypeAttributes.NestedAssembly)]
    [InlineData(TypeAttributes.NestedFamily)]
    [InlineData(TypeAttributes.NestedFamilyAndAssembly)]
    [InlineData(TypeAttributes.NestedFamilyOrAssembly)]
    [InlineData(TypeAttributes.NestedPrivate)]
    public void TypeCanAccessOwnNestedType(TypeAttributes attributes)
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);
        var type2 = AddNestedType(type1, "Type2", attributes);

        Assert.True(type2.IsAccessibleFromType(type1));
        Assert.True(type1.IsAccessibleFromType(type2));
    }

    [Fact]
    public void TypeCanAccessNestedPublicTypeDifferentAssembly()
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);

        var module2 = CreateDummyModule("Module2");
        var type2 = AddTopLevelType(module2, "Type2", TypeAttributes.Public);
        var type3 = AddNestedType(type2, "Type3", TypeAttributes.NestedPublic);

        Assert.True(type3.IsAccessibleFromType(type1));
        Assert.True(type1.IsAccessibleFromType(type3));
    }

    [Fact]
    public void TypeCannotAccessNestedPublicTypeWithNonPublicDeclaringType()
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);

        var module2 = CreateDummyModule("Module2");
        var type2 = AddTopLevelType(module2, "Type2", TypeAttributes.NotPublic);
        var type3 = AddNestedType(type2, "Type3", TypeAttributes.NestedPublic);

        Assert.False(type3.IsAccessibleFromType(type1));
        Assert.True(type1.IsAccessibleFromType(type3));
    }

    [Fact]
    public void TypeCannotAccessNestedPrivateDifferentAssembly()
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);

        var module2 = CreateDummyModule("Module2");
        var type2 = AddTopLevelType(module2, "Type2", TypeAttributes.Public);
        var type3 = AddNestedType(type2, "Type3", TypeAttributes.NestedPrivate);

        Assert.False(type3.IsAccessibleFromType(type1));
        Assert.True(type1.IsAccessibleFromType(type3));
    }

    [Fact]
    public void TypeCannotAccessNestedPrivateDifferentType()
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);
        var type2 = AddTopLevelType(module1, "Type2", TypeAttributes.Public);
        var type3 = AddNestedType(type2, "Type3", TypeAttributes.NestedPrivate);

        Assert.False(type3.IsAccessibleFromType(type1));
        Assert.True(type1.IsAccessibleFromType(type3));
    }

    [Fact]
    public void TypeCanAccessNestedAssemblyDifferentType()
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);
        var type2 = AddTopLevelType(module1, "Type2", TypeAttributes.Public);
        var type3 = AddNestedType(type2, "Type3", TypeAttributes.NestedAssembly);

        Assert.True(type3.IsAccessibleFromType(type1));
        Assert.True(type1.IsAccessibleFromType(type3));
    }

    [Fact]
    public void TypeCannotAccessNestedAssemblyDifferentTypeDifferentAssembly()
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);

        var module2 = CreateDummyModule("Module2");
        var type2 = AddTopLevelType(module2, "Type2", TypeAttributes.Public);
        var type3 = AddNestedType(type2, "Type3", TypeAttributes.NestedAssembly);

        Assert.False(type3.IsAccessibleFromType(type1));
        Assert.True(type1.IsAccessibleFromType(type3));
    }

    [Fact]
    public void TypeCanAccessNestedFamilyInBaseType()
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);
        var type2 = AddNestedType(type1, "Type2", TypeAttributes.NestedFamily);
        var type3 = AddTopLevelType(module1, "Type3", TypeAttributes.Public, type1);

        Assert.True(type2.IsAccessibleFromType(type3));
    }

    [Fact]
    public void TypeCanAccessNestedFamilyInFarBaseType()
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);
        var type2 = AddNestedType(type1, "Type2", TypeAttributes.NestedFamily);
        var type3 = AddTopLevelType(module1, "Type3", TypeAttributes.Public, type1);
        var type4 = AddTopLevelType(module1, "Type4", TypeAttributes.Public, type3);
        var type5 = AddTopLevelType(module1, "Type5", TypeAttributes.Public, type4);

        Assert.True(type2.IsAccessibleFromType(type5));
    }

    [Fact]
    public void TypeCannotAccessNestedFamilyInNonBaseType()
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);
        var type2 = AddNestedType(type1, "Type2", TypeAttributes.NestedFamily);
        var type3 = AddTopLevelType(module1, "Type3", TypeAttributes.Public);

        Assert.False(type2.IsAccessibleFromType(type3));
    }
}
