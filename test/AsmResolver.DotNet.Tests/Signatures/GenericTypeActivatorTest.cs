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
        private TypeReference _dummyGenericType;

        public GenericTypeActivatorTest()
        {
            _dummyGenericType = new TypeReference(
                new AssemblyReference("SomeAssembly", new Version()),
                "SomeNamespace", "SomeType");
        }
        
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

            var signature = new GenericInstanceTypeSignature(_dummyGenericType, false,
                new GenericParameterSignature(GenericParameterType.Type, 0));

            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);
            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(new GenericInstanceTypeSignature(_dummyGenericType, false,
                _module.CorLibTypeFactory.String), newSignature, Comparer);
        }

        [Fact]
        public void InstantiateNestedGenericInstanceType()
        {
            var signature = new GenericInstanceTypeSignature(_dummyGenericType, false,
                new GenericInstanceTypeSignature(_dummyGenericType, false,
                    new GenericParameterSignature(GenericParameterType.Type, 0)));

            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);
            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(new GenericInstanceTypeSignature(_dummyGenericType, false,
                new GenericInstanceTypeSignature(_dummyGenericType, false,
                    _module.CorLibTypeFactory.String)), newSignature, Comparer);
        }

        [Fact]
        public void InstantiateGenericInstanceTypeWithTypeAndMethodArgument()
        {
            var signature = new GenericInstanceTypeSignature(_dummyGenericType, false,
                new GenericParameterSignature(GenericParameterType.Type, 0),
                new GenericParameterSignature(GenericParameterType.Method, 0));

            var context = new GenericContext(
                GetProvider(_module.CorLibTypeFactory.String),
                GetProvider(_module.CorLibTypeFactory.Int32));
            
            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(new GenericInstanceTypeSignature(_dummyGenericType, false,
                    _module.CorLibTypeFactory.String,
                    _module.CorLibTypeFactory.Int32), newSignature, Comparer);
        }

        [Fact]
        public void InstantiateMethodSignatureWithGenericReturnType()
        {
            var signature = MethodSignature.CreateStatic(
                    new GenericParameterSignature(GenericParameterType.Type, 0));
            
            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);
            
            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(MethodSignature.CreateStatic(
                _module.CorLibTypeFactory.String
            ), newSignature, Comparer);
        } 

        [Fact]
        public void InstantiateMethodSignatureWithGenericParameterType()
        {
            var signature = MethodSignature.CreateStatic(
                _module.CorLibTypeFactory.Void,
                    new GenericParameterSignature(GenericParameterType.Type, 0));
            
            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);
            
            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(MethodSignature.CreateStatic(
                _module.CorLibTypeFactory.Void,
                _module.CorLibTypeFactory.String
            ), newSignature, Comparer);
        } 

        [Fact]
        public void InstantiateMethodSignatureWithGenericSentinelParameterType()
        {
            var signature = MethodSignature.CreateStatic(_module.CorLibTypeFactory.Void);
            signature.IncludeSentinel = true;
            signature.SentinelParameterTypes.Add(new GenericParameterSignature(GenericParameterType.Type, 0));
            
            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);
            
            var newSignature = signature.InstantiateGenericTypes(context);
            
            var expected = MethodSignature.CreateStatic(_module.CorLibTypeFactory.Void);
            expected.IncludeSentinel = true;
            expected.SentinelParameterTypes.Add(_module.CorLibTypeFactory.String);

            Assert.Equal(expected, newSignature, Comparer);
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