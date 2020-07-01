using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using Xunit;

namespace AsmResolver.DotNet.Tests.Signatures
{
    public class GenericTypeActivatorTest
    {
        private readonly SignatureComparer Comparer = new SignatureComparer();
        private readonly ModuleDefinition _module = new ModuleDefinition("DummyModule");

        public static IGenericArgumentsProvider GetProvider(params TypeSignature[] signatures)
        {
            return new MockGenericArgumentProvider(signatures);
        }
        
        [Fact]
        public void InstantiateCorLibShouldNotChange()
        {
            var signature = _module.CorLibTypeFactory.Boolean;
            var context = new GenericContext();
            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(_module.CorLibTypeFactory.Boolean, newSignature, Comparer);
        }

        [Fact]
        public void InstantiateTypeGenericParameter()
        {
            var signature = new GenericParameterSignature(GenericParameterType.Type, 0);
            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);
            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(_module.CorLibTypeFactory.String, newSignature, Comparer);
        }

        [Fact]
        public void InstantiateMethodGenericParameter()
        {
            var signature = new GenericParameterSignature(GenericParameterType.Method, 0);
            var context = new GenericContext(null, GetProvider(_module.CorLibTypeFactory.String));
            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(_module.CorLibTypeFactory.String, newSignature, Comparer);
        }

        [Fact]
        public void InstantiateSimpleGenericInstanceType()
        {
            var genericType = new TypeReference(
                new AssemblyReference("SomeAssembly", new Version()),
                "SomeNamespace", "SomeType");

            var signature = new GenericInstanceTypeSignature(genericType, false,
                new GenericParameterSignature(GenericParameterType.Type, 0));

            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);
            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(new GenericInstanceTypeSignature(genericType, false,
                _module.CorLibTypeFactory.String), newSignature, Comparer);
        }

        [Fact]
        public void InstantiateNestedGenericInstanceType()
        {
            var genericType = new TypeReference(
                new AssemblyReference("SomeAssembly", new Version()),
                "SomeNamespace", "SomeType");

            var signature = new GenericInstanceTypeSignature(genericType, false,
                new GenericInstanceTypeSignature(genericType, false,
                    new GenericParameterSignature(GenericParameterType.Type, 0)));

            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);
            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(new GenericInstanceTypeSignature(genericType, false,
                new GenericInstanceTypeSignature(genericType, false,
                    _module.CorLibTypeFactory.String)), newSignature, Comparer);
        }

        [Fact]
        public void InstantiateGenericInstanceTypeWithTypeAndMethodArgument()
        {
            var genericType = new TypeReference(
                new AssemblyReference("SomeAssembly", new Version()),
                "SomeNamespace", "SomeType");

            var signature = new GenericInstanceTypeSignature(genericType, false,
                new GenericParameterSignature(GenericParameterType.Type, 0),
                new GenericParameterSignature(GenericParameterType.Method, 0));

            var context = new GenericContext(
                GetProvider(_module.CorLibTypeFactory.String),
                GetProvider(_module.CorLibTypeFactory.Int32));
            
            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(new GenericInstanceTypeSignature(genericType, false,
                    _module.CorLibTypeFactory.String,
                    _module.CorLibTypeFactory.Int32), newSignature, Comparer);
        }
        
        private sealed class MockGenericArgumentProvider : IGenericArgumentsProvider
        {
            public MockGenericArgumentProvider(IList<TypeSignature> typeArguments)
            {
                TypeArguments = typeArguments ?? throw new ArgumentNullException(nameof(typeArguments));
            }
            
            public IList<TypeSignature> TypeArguments
            {
                get;
            }
        }
    }
}