using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
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
            Assert.Equal("Namespace.Type<System.Int32, Namespace.Type<System.Object>>",
                _dummyType.MakeGenericInstanceType(
                    module.CorLibTypeFactory.Int32,
                    _dummyType.MakeGenericInstanceType(module.CorLibTypeFactory.Object)).FullName);
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
    }
}
