using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class AccessibilityTest
    {
        private const string DummyAssemblyName = "SomeAssemblyName";
        private readonly SignatureComparer _comparer = new SignatureComparer();

        private static ModuleDefinition CreateDummyModule()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var module = assembly.NetDirectory.MetadataHeader.LockMetadata().Assembly.Modules[0];
            return module;
        }

        [Fact]
        public void TypeAccessItself()
        {
            var module = CreateDummyModule();

            var type = new TypeDefinition(null, "T1", TypeAttributes.NotPublic);            
            module.TopLevelTypes.Add(type);
            
            Assert.True(type.IsAccessibleFromType(type));
        }

        [Fact]
        public void NestedTypeAccessItself()
        {
            var module = CreateDummyModule();

            var type = new TypeDefinition(null, "T1", TypeAttributes.NotPublic);
            module.TopLevelTypes.Add(type);
            
            var nestedType = new TypeDefinition(null, "T2", TypeAttributes.NestedPublic);
            type.NestedClasses.Add(new NestedClass(nestedType));
            
            Assert.True(nestedType.IsAccessibleFromType(nestedType));
            nestedType.IsNestedAssembly = true;
            Assert.True(nestedType.IsAccessibleFromType(nestedType));
            nestedType.IsNestedFamily = true;
            Assert.True(nestedType.IsAccessibleFromType(nestedType));
            nestedType.IsNestedFamilyAndAssembly = true;
            Assert.True(nestedType.IsAccessibleFromType(nestedType));
            nestedType.IsNestedFamilyOrAssembly = true;
            Assert.True(nestedType.IsAccessibleFromType(nestedType));
            nestedType.IsNestedPrivate = true;
            Assert.True(nestedType.IsAccessibleFromType(nestedType));
        }

        [Fact]
        public void PrivateNestedType()
        {
            var module = CreateDummyModule();

            var type = new TypeDefinition(null, "T1", TypeAttributes.NotPublic);
            module.TopLevelTypes.Add(type);
            
            var nestedType = new TypeDefinition(null, "T2", TypeAttributes.NestedPrivate);
            type.NestedClasses.Add(new NestedClass(nestedType));
            
            var other = new TypeDefinition(null, "T3", TypeAttributes.NotPublic);
            module.TopLevelTypes.Add(other);
            
            Assert.True(nestedType.IsAccessibleFromType(type));
            Assert.True(type.IsAccessibleFromType(nestedType));
            Assert.False(nestedType.IsAccessibleFromType(other));
        }

        [Fact]
        public void AssemblyNestedType()
        {
            var module = CreateDummyModule();

            var type = new TypeDefinition(null, "T1", TypeAttributes.NotPublic);
            module.TopLevelTypes.Add(type);
            
            var nestedType = new TypeDefinition(null, "T2", TypeAttributes.NestedAssembly);
            type.NestedClasses.Add(new NestedClass(nestedType));
            
            var other = new TypeDefinition(null, "T3", TypeAttributes.NotPublic);
            module.TopLevelTypes.Add(other);
            
            Assert.True(nestedType.IsAccessibleFromType(type));
            Assert.True(type.IsAccessibleFromType(nestedType));
            Assert.True(nestedType.IsAccessibleFromType(other));
        }

        [Fact]
        public void DoubleNestedTypes()
        {
            var module = CreateDummyModule();

            var type = new TypeDefinition(null, "T1", TypeAttributes.NotPublic);
            module.TopLevelTypes.Add(type);
            
            var nestedType1 = new TypeDefinition(null, "T2", TypeAttributes.NestedPrivate);
            type.NestedClasses.Add(new NestedClass(nestedType1));
            
            var nestedType2 = new TypeDefinition(null, "T3", TypeAttributes.NestedPrivate);
            nestedType1.NestedClasses.Add(new NestedClass(nestedType2));
            
            Assert.True(nestedType1.IsAccessibleFromType(type));
            Assert.True(nestedType2.IsAccessibleFromType(nestedType1));
            Assert.True(type.IsAccessibleFromType(nestedType2));
            Assert.False(nestedType2.IsAccessibleFromType(type));
        }

        [Fact]
        public void TwoNonPublicTypesSameAssembly()
        {            
            var module = CreateDummyModule();
            
            var type1 = new TypeDefinition(null, "T1", TypeAttributes.NotPublic);
            module.TopLevelTypes.Add(type1);
            var type2 = new TypeDefinition(null, "T2", TypeAttributes.NotPublic);
            module.TopLevelTypes.Add(type2);
            
            Assert.True(type1.IsAccessibleFromType(type2));
            Assert.True(type2.IsAccessibleFromType(type1));
        }

        [Fact]
        public void TwoNonPublicTypesDifferentAssembly()
        {
            var module1 = CreateDummyModule();
            var module2 = CreateDummyModule();
            
            var type1 = new TypeDefinition(null, "T1", TypeAttributes.NotPublic);
            module1.TopLevelTypes.Add(type1);
            var type2 = new TypeDefinition(null, "T2", TypeAttributes.NotPublic);
            module2.TopLevelTypes.Add(type2);
            
            Assert.False(type1.IsAccessibleFromType(type2));
            Assert.False(type2.IsAccessibleFromType(type1));
        }

        [Fact]
        public void PublicAndNonPublicTypeDifferentAssembly()
        {
            var module1 = CreateDummyModule();
            var module2 = CreateDummyModule();
            
            var type1 = new TypeDefinition(null, "T1", TypeAttributes.Public);
            module1.TopLevelTypes.Add(type1);
            var type2 = new TypeDefinition(null, "T2", TypeAttributes.NotPublic);
            module2.TopLevelTypes.Add(type2);
            
            Assert.True(type1.IsAccessibleFromType(type2));
            Assert.False(type2.IsAccessibleFromType(type1));
        }

        [Fact]
        public void PrivateField()
        {
            var module = CreateDummyModule();   
            
            var type1 = new TypeDefinition(null, "T1", TypeAttributes.Public);
            module.TopLevelTypes.Add(type1);
            var type2 = new TypeDefinition(null, "T2", TypeAttributes.Public);
            module.TopLevelTypes.Add(type2);

            var field = new FieldDefinition("F1", FieldAttributes.Private,
                new FieldSignature(module.Image.TypeSystem.Int32));
            type1.Fields.Add(field);

            Assert.True(field.IsAccessibleFromType(type1));
            Assert.False(field.IsAccessibleFromType(type2));
        }

        [Fact]
        public void AssemblyField()
        {
            var module1 = CreateDummyModule();
            var module2 = CreateDummyModule();
            
            var type1 = new TypeDefinition(null, "T1", TypeAttributes.Public);
            module1.TopLevelTypes.Add(type1);
            var type2 = new TypeDefinition(null, "T2", TypeAttributes.Public);
            module1.TopLevelTypes.Add(type2);
            var type3 = new TypeDefinition(null, "T3", TypeAttributes.Public);
            module2.TopLevelTypes.Add(type3);

            var field = new FieldDefinition("F1", FieldAttributes.Assembly,
                new FieldSignature(module1.Image.TypeSystem.Int32));
            type1.Fields.Add(field);

            Assert.True(field.IsAccessibleFromType(type1));
            Assert.True(field.IsAccessibleFromType(type2));
            Assert.False(field.IsAccessibleFromType(type3));
        }

        [Fact]
        public void PublicField()
        {
            var module1 = CreateDummyModule();
            var module2 = CreateDummyModule();
            
            var type1 = new TypeDefinition(null, "T1", TypeAttributes.Public);
            module1.TopLevelTypes.Add(type1);
            var type2 = new TypeDefinition(null, "T2", TypeAttributes.Public);
            module1.TopLevelTypes.Add(type2);
            var type3 = new TypeDefinition(null, "T3", TypeAttributes.Public);
            module2.TopLevelTypes.Add(type3);

            var field = new FieldDefinition("F1", FieldAttributes.Public,
                new FieldSignature(module1.Image.TypeSystem.Int32));
            type1.Fields.Add(field);

            Assert.True(field.IsAccessibleFromType(type1));
            Assert.True(field.IsAccessibleFromType(type2));
            Assert.True(field.IsAccessibleFromType(type3));
        }

        [Fact]
        public void PublicFieldPrivateNestedType()
        {
            var module1 = CreateDummyModule();
            
            var type1 = new TypeDefinition(null, "T1", TypeAttributes.Public);
            module1.TopLevelTypes.Add(type1);
            var type2 = new TypeDefinition(null, "T2", TypeAttributes.NestedPrivate);
            type1.NestedClasses.Add(new NestedClass(type2));
            var type3 = new TypeDefinition(null, "T3", TypeAttributes.Public);
            module1.TopLevelTypes.Add(type3);

            var field = new FieldDefinition("F1", FieldAttributes.Public,
                new FieldSignature(module1.Image.TypeSystem.Int32));
            type2.Fields.Add(field);

            Assert.True(field.IsAccessibleFromType(type1));
            Assert.True(field.IsAccessibleFromType(type2));
            Assert.False(field.IsAccessibleFromType(type3));
        }

        [Fact]
        public void PrivateMethod()
        {
            var module = CreateDummyModule();   
            
            var type1 = new TypeDefinition(null, "T1", TypeAttributes.Public);
            module.TopLevelTypes.Add(type1);
            var type2 = new TypeDefinition(null, "T2", TypeAttributes.Public);
            module.TopLevelTypes.Add(type2);

            var method = new MethodDefinition("M1", MethodAttributes.Private,
                new MethodSignature(module.Image.TypeSystem.Void));
            type1.Methods.Add(method);

            Assert.True(method.IsAccessibleFromType(type1));
            Assert.False(method.IsAccessibleFromType(type2));
        }
        
        [Fact]
        public void AssemblyMethod()
        {
            var module1 = CreateDummyModule();
            var module2 = CreateDummyModule();
            
            var type1 = new TypeDefinition(null, "T1", TypeAttributes.Public);
            module1.TopLevelTypes.Add(type1);
            var type2 = new TypeDefinition(null, "T2", TypeAttributes.Public);
            module1.TopLevelTypes.Add(type2);
            var type3 = new TypeDefinition(null, "T3", TypeAttributes.Public);
            module2.TopLevelTypes.Add(type3);

            var method = new MethodDefinition("M1", MethodAttributes.Assembly,
                new MethodSignature(module1.Image.TypeSystem.Void));
            type1.Methods.Add(method);

            Assert.True(method.IsAccessibleFromType(type1));
            Assert.True(method.IsAccessibleFromType(type2));
            Assert.False(method.IsAccessibleFromType(type3));
        }

        [Fact]
        public void PublicMethod()
        {
            var module1 = CreateDummyModule();
            var module2 = CreateDummyModule();
            
            var type1 = new TypeDefinition(null, "T1", TypeAttributes.Public);
            module1.TopLevelTypes.Add(type1);
            var type2 = new TypeDefinition(null, "T2", TypeAttributes.Public);
            module1.TopLevelTypes.Add(type2);
            var type3 = new TypeDefinition(null, "T3", TypeAttributes.Public);
            module2.TopLevelTypes.Add(type3);

            var method = new MethodDefinition("M1", MethodAttributes.Public,
                new MethodSignature(module1.Image.TypeSystem.Void));
            type1.Methods.Add(method);

            Assert.True(method.IsAccessibleFromType(type1));
            Assert.True(method.IsAccessibleFromType(type2));
            Assert.True(method.IsAccessibleFromType(type3));
        }

        [Fact]
        public void PublicMethodPrivateNestedType()
        {
            var module1 = CreateDummyModule();
            
            var type1 = new TypeDefinition(null, "T1", TypeAttributes.Public);
            module1.TopLevelTypes.Add(type1);
            var type2 = new TypeDefinition(null, "T2", TypeAttributes.NestedPrivate);
            type1.NestedClasses.Add(new NestedClass(type2));
            var type3 = new TypeDefinition(null, "T3", TypeAttributes.Public);
            module1.TopLevelTypes.Add(type3);

            var method = new MethodDefinition("M1", MethodAttributes.Public,
                new MethodSignature(module1.Image.TypeSystem.Void));
            type2.Methods.Add(method);

            Assert.True(method.IsAccessibleFromType(type1));
            Assert.True(method.IsAccessibleFromType(type2));
            Assert.False(method.IsAccessibleFromType(type3));
        }

    }
}