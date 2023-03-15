using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.TestCases.Generics;
using AsmResolver.DotNet.TestCases.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Signatures
{
    public class TypeSignatureTest
    {
        private readonly TypeDefinition _dummyType = new("Namespace", "Type", TypeAttributes.Class);

        [Fact]
        public void GetTypeDefOrRefFullName()
        {
            Assert.Equal("Namespace.Type", _dummyType.ToTypeSignature().FullName);
        }

        [Fact]
        public void GetArrayTypeFullName()
        {
            Assert.Equal("Namespace.Type[0...9, 0...19]", _dummyType
                .ToTypeSignature()
                .MakeArrayType(
                    new ArrayDimension(10),
                    new ArrayDimension(20))
                .FullName);
        }

        [Fact]
        public void GetByReferenceTypeFullName()
        {
            Assert.Equal("Namespace.Type&", _dummyType
                .ToTypeSignature()
                .MakeByReferenceType()
                .FullName);
        }

        [Fact]
        public void GetCorLibTypeFullName()
        {
            var module = new ModuleDefinition("Dummy");
            Assert.Equal("System.String", module.CorLibTypeFactory.String.FullName);
        }

        [Fact]
        public void GetFunctionPointerTypeFullName()
        {
            var module = new ModuleDefinition("Dummy");
            Assert.Equal("method System.String *(System.Object, System.Int32)",
                MethodSignature.CreateStatic(
                        module.CorLibTypeFactory.String,
                        module.CorLibTypeFactory.Object,
                        module.CorLibTypeFactory.Int32)
                    .MakeFunctionPointerType().FullName);
        }

        [Fact]
        public void GetInstanceFunctionPointerTypeFullName()
        {
            var module = new ModuleDefinition("Dummy");
            Assert.Equal("method instance System.String *(System.Object, System.Int32)",
                MethodSignature.CreateInstance(
                        module.CorLibTypeFactory.String,
                        module.CorLibTypeFactory.Object,
                        module.CorLibTypeFactory.Int32)
                    .MakeFunctionPointerType().FullName);
        }

        [Fact]
        public void GetGenericInstanceTypeFullName()
        {
            var module = new ModuleDefinition("Dummy");
            var genericInstance = _dummyType.MakeGenericInstanceType(
                module.CorLibTypeFactory.Int32,
                _dummyType.MakeGenericInstanceType(module.CorLibTypeFactory.Object));

            Assert.Equal("Type<System.Int32, Namespace.Type<System.Object>>", genericInstance.Name);
            Assert.Equal("Namespace.Type<System.Int32, Namespace.Type<System.Object>>", genericInstance.FullName);
        }

        [Fact]
        public void GetGenericParameterFullName()
        {
            Assert.Equal("!2", new GenericParameterSignature(GenericParameterType.Type, 2).FullName);
            Assert.Equal("!!2", new GenericParameterSignature(GenericParameterType.Method, 2).FullName);
        }

        [Fact]
        public void GetPointerTypeFullName()
        {
            Assert.Equal("Namespace.Type*", _dummyType
                .ToTypeSignature()
                .MakePointerType()
                .FullName);
        }

        [Fact]
        public void GetSzArrayTypeFullName()
        {
            Assert.Equal("Namespace.Type[]", _dummyType
                .ToTypeSignature()
                .MakeSzArrayType()
                .FullName);
        }

        [Fact]
        public void GetRequiredModifierTypeFullName()
        {
            Assert.Equal("Namespace.Type modreq(Namespace.Type)",
                _dummyType
                    .ToTypeSignature()
                    .MakeModifierType(_dummyType, true)
                    .FullName);
        }

        [Fact]
        public void GetOptionalModifierTypeFullName()
        {
            Assert.Equal("Namespace.Type modopt(Namespace.Type)",
                _dummyType
                    .ToTypeSignature()
                    .MakeModifierType(_dummyType, false)
                    .FullName);
        }

        [Theory]
        [InlineData(ElementType.I, ElementType.I)]
        [InlineData(ElementType.I1, ElementType.I1)]
        [InlineData(ElementType.I2, ElementType.I2)]
        [InlineData(ElementType.I4, ElementType.I4)]
        [InlineData(ElementType.I8, ElementType.I8)]
        [InlineData(ElementType.U, ElementType.I)]
        [InlineData(ElementType.U1, ElementType.I1)]
        [InlineData(ElementType.U2, ElementType.I2)]
        [InlineData(ElementType.U4, ElementType.I4)]
        [InlineData(ElementType.U8, ElementType.I8)]
        [InlineData(ElementType.String, ElementType.String)]
        [InlineData(ElementType.Boolean, ElementType.Boolean)]
        [InlineData(ElementType.Char, ElementType.Char)]
        public void GetReducedTypeOfPrimitive(ElementType type, ElementType expected)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal(expected, module.CorLibTypeFactory.FromElementType(type)!.GetReducedType().ElementType);
        }

        [Fact]
        public void GetReducedTypeOfNonPrimitive()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var type = module.TopLevelTypes.First(t => t.Name == "Program").ToTypeSignature(false);
            Assert.Equal(type, type.GetReducedType());
        }

        [Theory]
        [InlineData(ElementType.I, ElementType.I)]
        [InlineData(ElementType.I1, ElementType.I1)]
        [InlineData(ElementType.I2, ElementType.I2)]
        [InlineData(ElementType.I4, ElementType.I4)]
        [InlineData(ElementType.I8, ElementType.I8)]
        [InlineData(ElementType.U, ElementType.I)]
        [InlineData(ElementType.U1, ElementType.I1)]
        [InlineData(ElementType.U2, ElementType.I2)]
        [InlineData(ElementType.U4, ElementType.I4)]
        [InlineData(ElementType.U8, ElementType.I8)]
        [InlineData(ElementType.String, ElementType.String)]
        [InlineData(ElementType.Boolean, ElementType.I1)]
        [InlineData(ElementType.Char, ElementType.I2)]
        public void GetVerificationTypeOfPrimitive(ElementType type, ElementType expected)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal(expected, module.CorLibTypeFactory.FromElementType(type)!.GetVerificationType().ElementType);
        }

        [Theory]
        [InlineData(ElementType.I, ElementType.I)]
        [InlineData(ElementType.I1, ElementType.I1)]
        [InlineData(ElementType.I2, ElementType.I2)]
        [InlineData(ElementType.I4, ElementType.I4)]
        [InlineData(ElementType.I8, ElementType.I8)]
        [InlineData(ElementType.U, ElementType.I)]
        [InlineData(ElementType.U1, ElementType.I1)]
        [InlineData(ElementType.U2, ElementType.I2)]
        [InlineData(ElementType.U4, ElementType.I4)]
        [InlineData(ElementType.U8, ElementType.I8)]
        [InlineData(ElementType.String, ElementType.String)]
        [InlineData(ElementType.Boolean, ElementType.I1)]
        [InlineData(ElementType.Char, ElementType.I2)]
        public void GetVerificationTypeOfManagedPrimitivePointer(ElementType type, ElementType expected)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var pointerType = module.CorLibTypeFactory.FromElementType(type)!.MakeByReferenceType();
            var actualType = Assert.IsAssignableFrom<ByReferenceTypeSignature>(pointerType.GetVerificationType());
            Assert.Equal(expected, actualType.BaseType.ElementType);
        }

        [Fact]
        public void GetVerificationTypeOfNonPrimitive()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var type = module.TopLevelTypes.First(t => t.Name == "Program").ToTypeSignature(false);
            Assert.Equal(type, type.GetVerificationType());
        }

        [Theory]
        [InlineData(ElementType.I, ElementType.I)]
        [InlineData(ElementType.I1, ElementType.I4)]
        [InlineData(ElementType.I2, ElementType.I4)]
        [InlineData(ElementType.I4, ElementType.I4)]
        [InlineData(ElementType.I8, ElementType.I8)]
        [InlineData(ElementType.U, ElementType.I)]
        [InlineData(ElementType.U1, ElementType.I4)]
        [InlineData(ElementType.U2, ElementType.I4)]
        [InlineData(ElementType.U4, ElementType.I4)]
        [InlineData(ElementType.U8, ElementType.I8)]
        [InlineData(ElementType.String, ElementType.String)]
        [InlineData(ElementType.Boolean, ElementType.I4)]
        [InlineData(ElementType.Char, ElementType.I4)]
        [InlineData(ElementType.R4, ElementType.R8)] // Technically incorrect, as it should be F.
        [InlineData(ElementType.R8, ElementType.R8)]
        public void GetIntermediateTypeOfPrimitive(ElementType type, ElementType expected)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal(expected, module.CorLibTypeFactory.FromElementType(type)!.GetIntermediateType().ElementType);
        }

        [Fact]
        public void GetIntermediateTypeOfNonPrimitive()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var type = module.TopLevelTypes.First(t => t.Name == "Program").ToTypeSignature(false);
            Assert.Equal(type, type.GetIntermediateType());
        }

        [Fact]
        public void GetDirectBaseClassOfArrayType()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var signature = module.CorLibTypeFactory.Object.MakeArrayType(3);
            Assert.Equal("System.Array", signature.GetDirectBaseClass()!.FullName);
        }

        [Fact]
        public void GetDirectBaseClassOfSzArrayType()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var signature = module.CorLibTypeFactory.Object.MakeSzArrayType();
            Assert.Equal("System.Array", signature.GetDirectBaseClass()!.FullName);
        }

        [Fact]
        public void GetDirectBaseClassOfInterfaceType()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var interfaceDefinition = new TypeDefinition("Namespace", "IName", TypeAttributes.Interface);
            module.TopLevelTypes.Add(interfaceDefinition);

            var interfaceSignature = interfaceDefinition.ToTypeSignature(false);

            Assert.Equal("System.Object", interfaceSignature.GetDirectBaseClass()!.FullName);
        }

        [Fact]
        public void GetDirectBaseClassOfNormalType()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var type = module.TopLevelTypes.First(t => t.Name == "Program").ToTypeSignature();
            Assert.Equal("System.Object", type.GetDirectBaseClass()!.FullName);
        }

        [Fact]
        public void GetDirectBaseClassOfNormalTypeWithBaseType()
        {
            var module = ModuleDefinition.FromFile(typeof(DerivedClass).Assembly.Location);
            var type = module.LookupMember<TypeDefinition>(typeof(DerivedClass).MetadataToken);
            Assert.Equal(type.BaseType!.FullName, type.ToTypeSignature().GetDirectBaseClass()!.FullName);
        }

        [Fact]
        public void GetDirectBaseClassOfGenericTypeInstance()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericType<,,>).Assembly.Location);
            var genericInstanceType = module.LookupMember<TypeDefinition>(typeof(GenericType<,,>).MetadataToken)
                .MakeGenericInstanceType(
                    module.CorLibTypeFactory.Int32,
                    module.CorLibTypeFactory.String,
                    module.CorLibTypeFactory.Object);

            Assert.Equal("System.Object", genericInstanceType.GetDirectBaseClass()!.FullName);
        }

        [Fact]
        public void GetDirectBaseClassOfGenericTypeInstanceWithGenericBaseClass()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericDerivedType<,>).Assembly.Location);
            var genericInstanceType = module.LookupMember<TypeDefinition>(typeof(GenericDerivedType<,>).MetadataToken)
                .MakeGenericInstanceType(
                    module.CorLibTypeFactory.Int32,
                    module.CorLibTypeFactory.Object);

            var baseClass = Assert.IsAssignableFrom<GenericInstanceTypeSignature>(
                genericInstanceType.GetDirectBaseClass());

            Assert.Equal(typeof(GenericType<,,>).Namespace, baseClass.GenericType.Namespace);
            Assert.Equal(typeof(GenericType<,,>).Name, baseClass.GenericType.Name);
            Assert.Equal(new[]
            {
                "System.Int32",
                "System.Object",
                "System.String"
            }, baseClass.TypeArguments.Select(t => t.FullName));
        }
    }
}
