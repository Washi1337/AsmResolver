using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests;

public partial class AccessibilityTest
{
    private static FieldDefinition AddField(TypeDefinition declaringType, string name, FieldAttributes attributes)
    {
        var field = new FieldDefinition(name, attributes, declaringType.Module!.CorLibTypeFactory.Object);
        declaringType.Fields.Add(field);
        return field;
    }

    [Theory]
    [InlineData(FieldAttributes.Public)]
    [InlineData(FieldAttributes.Family)]
    [InlineData(FieldAttributes.Assembly)]
    [InlineData(FieldAttributes.FamilyAndAssembly)]
    [InlineData(FieldAttributes.FamilyOrAssembly)]
    [InlineData(FieldAttributes.Private)]
    public void TypeCanAccessOwnField(FieldAttributes attributes)
    {
        var module = CreateDummyModule("Module1");
        var type = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var field = AddField(type, "Field1", attributes);

        Assert.True(field.IsAccessibleFromType(type));
    }

    [Fact]
    public void TypeCanAccessPublicFieldOtherType()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var type2 = AddTopLevelType(module, "Type2", TypeAttributes.Public);
        var field = AddField(type2, "Field1", FieldAttributes.Public);

        Assert.True(field.IsAccessibleFromType(type1));
    }

    [Fact]
    public void TypeCanAccessAssemblyFieldOtherTypeSameAssembly()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var type2 = AddTopLevelType(module, "Type2", TypeAttributes.Public);
        var field = AddField(type2, "Field1", FieldAttributes.Assembly);

        Assert.True(field.IsAccessibleFromType(type1));
    }

    [Fact]
    public void TypeCannotAccessAssemblyFieldOtherTypeDifferentAssembly()
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);

        var module2 = CreateDummyModule("Module2");
        var type2 = AddTopLevelType(module2, "Type2", TypeAttributes.Public);
        var field = AddField(type2, "Field1", FieldAttributes.Assembly);

        Assert.False(field.IsAccessibleFromType(type1));
    }

    [Fact]
    public void TypeCannotAccessPrivateFieldOtherType()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var type2 = AddTopLevelType(module, "Type2", TypeAttributes.Public);
        var field = AddField(type2, "Field1", FieldAttributes.Private);

        Assert.False(field.IsAccessibleFromType(type1));
    }

    [Fact]
    public void TypeCannotAccessPublicFieldOtherNonAccessibleType()
    {
        var module1 = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module1, "Type1", TypeAttributes.Public);

        var module2 = CreateDummyModule("Module2");
        var type2 = AddTopLevelType(module2, "Type2", TypeAttributes.NotPublic);
        var field = AddField(type2, "Field1", FieldAttributes.Public);

        Assert.False(field.IsAccessibleFromType(type1));
    }

    [Fact]
    public void TypeCanAccessFamilyFieldInBaseType()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var field = AddField(type1, "Field1", FieldAttributes.Family);
        var type2 = AddTopLevelType(module, "Type2", TypeAttributes.Public, type1);

        Assert.True(field.IsAccessibleFromType(type2));
    }

    [Fact]
    public void TypeCannotAccessFamilyFieldInNonBaseType()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var field = AddField(type1, "Field1", FieldAttributes.Family);
        var type2 = AddTopLevelType(module, "Type2", TypeAttributes.Public);

        Assert.False(field.IsAccessibleFromType(type2));
    }

    [Fact]
    public void TypeCanAccessFamilyFieldInFarBaseType()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var field = AddField(type1, "Field1", FieldAttributes.Family);
        var type2 = AddTopLevelType(module, "Type2", TypeAttributes.Public, type1);
        var type3 = AddTopLevelType(module, "Type3", TypeAttributes.Public, type2);
        var type4 = AddTopLevelType(module, "Type4", TypeAttributes.Public, type3);
        var type5 = AddTopLevelType(module, "Type5", TypeAttributes.Public, type4);

        Assert.True(field.IsAccessibleFromType(type5));
    }

    [Fact]
    public void TypeCannotAccessPrivateFieldInOwnNestedType()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var type2 = AddNestedType(type1, "Type2", TypeAttributes.NestedPrivate);
        var field = AddField(type2, "Field1", FieldAttributes.Private);

        Assert.False(field.IsAccessibleFromType(type1));
    }

    [Fact]
    public void NestedTypeCanAccessPrivateFieldInDeclaringType()
    {
        var module = CreateDummyModule("Module1");
        var type1 = AddTopLevelType(module, "Type1", TypeAttributes.Public);
        var field = AddField(type1, "Field1", FieldAttributes.Private);
        var type2 = AddNestedType(type1, "Type2", TypeAttributes.NestedPrivate);

        Assert.True(field.IsAccessibleFromType(type2));
    }
}
