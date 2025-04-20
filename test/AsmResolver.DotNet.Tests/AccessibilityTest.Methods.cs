using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests;

public partial class AccessibilityTest
{
    private static MethodDefinition AddMethod(TypeDefinition declaringType, string name, MethodAttributes attributes)
    {
        var method = new MethodDefinition(name, attributes, MethodSignature.CreateInstance(declaringType.Module!.CorLibTypeFactory.Void));
        declaringType.Methods.Add(method);
        return method;
    }

    [Theory]
    [InlineData(MethodAttributes.Public)]
    [InlineData(MethodAttributes.Family)]
    [InlineData(MethodAttributes.Assembly)]
    [InlineData(MethodAttributes.FamilyAndAssembly)]
    [InlineData(MethodAttributes.FamilyOrAssembly)]
    [InlineData(MethodAttributes.Private)]
    public void TypeCanAccessOwnMethod(MethodAttributes attributes)
    {
        var module = CreateDummyModule("Module1");
        var type = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var method = AddMethod(type, "Method1", attributes);

        Assert.True(method.IsAccessibleFromType(type));
    }

    [Fact]
    public void TypeCanAccessPublicMethodOtherType()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var type2 = AddTopLevelType(module, "Type2", TypeAttributes.Public);
        var method = AddMethod(type2, "Method1", MethodAttributes.Public);

        Assert.True(method.IsAccessibleFromType(type1));
    }

    [Fact]
    public void TypeCanAccessAssemblyMethodOtherTypeSameAssembly()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var type2 = AddTopLevelType(module, "Type2", TypeAttributes.Public);
        var method = AddMethod(type2, "Method1", MethodAttributes.Assembly);

        Assert.True(method.IsAccessibleFromType(type1));
    }

    [Fact]
    public void TypeCannotAccessAssemblyMethodOtherTypeDifferentAssembly()
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);

        var module2 = CreateDummyModule("Module2");
        var type2 = AddTopLevelType(module2, "Type2", TypeAttributes.Public);
        var method = AddMethod(type2, "Method1", MethodAttributes.Assembly);

        Assert.False(method.IsAccessibleFromType(type1));
    }

    [Fact]
    public void TypeCannotAccessPrivateMethodOtherType()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var type2 = AddTopLevelType(module, "Type2", TypeAttributes.Public);
        var method = AddMethod(type2, "Method1", MethodAttributes.Private);

        Assert.False(method.IsAccessibleFromType(type1));
    }

    [Fact]
    public void TypeCannotAccessPublicMethodOtherNonAccessibleType()
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);

        var module2 = CreateDummyModule("Module2");
        var type2 = AddTopLevelType(module2, "Type2", TypeAttributes.NotPublic);
        var method = AddMethod(type2, "Method1", MethodAttributes.Public);

        Assert.False(method.IsAccessibleFromType(type1));
    }

    [Fact]
    public void TypeCanAccessFamilyMethodInBaseType()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var method = AddMethod(type1, "Method1", MethodAttributes.Family);
        var type2 = AddTopLevelType(module, "Type2", TypeAttributes.Public, type1);

        Assert.True(method.IsAccessibleFromType(type2));
    }

    [Fact]
    public void TypeCannotAccessFamilyMethodInNonBaseType()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var method = AddMethod(type1, "Method1", MethodAttributes.Family);
        var type2 = AddTopLevelType(module, "Type2", TypeAttributes.Public);

        Assert.False(method.IsAccessibleFromType(type2));
    }

    [Fact]
    public void TypeCanAccessFamilyMethodInFarBaseType()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var method = AddMethod(type1, "Method1", MethodAttributes.Family);
        var type2 = AddTopLevelType(module, "Type2", TypeAttributes.Public, type1);
        var type3 = AddTopLevelType(module, "Type3", TypeAttributes.Public, type2);
        var type4 = AddTopLevelType(module, "Type4", TypeAttributes.Public, type3);
        var type5 = AddTopLevelType(module, "Type5", TypeAttributes.Public, type4);

        Assert.True(method.IsAccessibleFromType(type5));
    }

    [Fact]
    public void TypeCannotAccessPrivateMethodInOwnNestedType()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var type2 = AddNestedType(type1, "Type2", TypeAttributes.NestedPrivate);
        var method = AddMethod(type2, "Method1", MethodAttributes.Private);

        Assert.False(method.IsAccessibleFromType(type1));
    }

    [Fact]
    public void NestedTypeCanAccessPrivateMethodInDeclaringType()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var method = AddMethod(type1, "Method1", MethodAttributes.Private);
        var type2 = AddNestedType(type1, "Type2", TypeAttributes.NestedPrivate);

        Assert.True(method.IsAccessibleFromType(type2));
    }

}
