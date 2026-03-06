using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using Xunit;

namespace AsmResolver.DotNet.Tests.Signatures
{
    public class GenericTypeActivatorTest
    {
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
            Assert.Equal(_module.CorLibTypeFactory.Boolean, newSignature, SignatureComparer.Default);
        }

        [Fact]
        public void InstantiateTypeGenericParameter()
        {
            var signature = new GenericParameterSignature(GenericParameterType.Type, 0);
            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);
            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(_module.CorLibTypeFactory.String, newSignature, SignatureComparer.Default);
        }

        [Fact]
        public void InstantiateMethodGenericParameter()
        {
            var signature = new GenericParameterSignature(GenericParameterType.Method, 0);
            var context = new GenericContext(null, GetProvider(_module.CorLibTypeFactory.String));
            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(_module.CorLibTypeFactory.String, newSignature, SignatureComparer.Default);
        }

        [Fact]
        public void InstantiateSimpleGenericInstanceType()
        {
            var signature = _dummyGenericType.MakeGenericInstanceType(
                isValueType: false,
                typeArguments: [new GenericParameterSignature(GenericParameterType.Type, 0)]
            );

            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);
            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(
                _dummyGenericType.MakeGenericInstanceType(false, [_module.CorLibTypeFactory.String]),
                newSignature,
                SignatureComparer.Default
            );
        }

        [Fact]
        public void InstantiateNestedGenericInstanceType()
        {
            var signature = _dummyGenericType.MakeGenericInstanceType(
                isValueType: false,
                typeArguments: [
                    _dummyGenericType.MakeGenericInstanceType(false, [new GenericParameterSignature(GenericParameterType.Type, 0)])
                ]
            );

            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);
            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(
                _dummyGenericType.MakeGenericInstanceType(
                    isValueType: false,
                    typeArguments: [_dummyGenericType.MakeGenericInstanceType(false, [_module.CorLibTypeFactory.String])]
                ),
                newSignature,
                SignatureComparer.Default
            );
        }

        [Fact]
        public void InstantiateGenericInstanceTypeWithTypeAndMethodArgument()
        {
            var signature = _dummyGenericType.MakeGenericInstanceType(
                isValueType: false,
                typeArguments:
                [
                    new GenericParameterSignature(GenericParameterType.Type, 0),
                    new GenericParameterSignature(GenericParameterType.Method, 0)
                ]
            );

            var context = new GenericContext(
                GetProvider(_module.CorLibTypeFactory.String),
                GetProvider(_module.CorLibTypeFactory.Int32));

            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(
                _dummyGenericType.MakeGenericInstanceType(
                    isValueType: false,
                    typeArguments: [_module.CorLibTypeFactory.String, _module.CorLibTypeFactory.Int32]
                ),
                newSignature,
                SignatureComparer.Default
            );
        }

        [Fact]
        public void InstantiateMethodSignatureWithGenericReturnType()
        {
            var signature = MethodSignature.CreateStatic(new GenericParameterSignature(GenericParameterType.Type, 0));

            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);

            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(
                MethodSignature.CreateStatic(_module.CorLibTypeFactory.String),
                newSignature,
                SignatureComparer.Default
            );
        }

        [Fact]
        public void InstantiateMethodSignatureWithGenericParameterType()
        {
            var signature = MethodSignature.CreateStatic(
                _module.CorLibTypeFactory.Void,
                [new GenericParameterSignature(GenericParameterType.Type, 0)]
            );

            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);

            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(
                MethodSignature.CreateStatic(
                    _module.CorLibTypeFactory.Void,
                    [_module.CorLibTypeFactory.String]
                ),
                newSignature,
                SignatureComparer.Default
            );
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

            Assert.Equal(expected, newSignature, SignatureComparer.Default);
        }

        [Fact]
        public void InstantiateFieldSignature()
        {
            var signature = new FieldSignature(new GenericParameterSignature(GenericParameterType.Type, 0));
            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);

            var newSignature = signature.InstantiateGenericTypes(context);
            Assert.Equal(new FieldSignature(_module.CorLibTypeFactory.String), newSignature, SignatureComparer.Default);
        }

        [Fact]
        public void InstantiatePropertySignatureWithGenericPropertyType()
        {
            var signature = PropertySignature.CreateStatic(new GenericParameterSignature(GenericParameterType.Type, 0));
            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);

            var newSignature = signature.InstantiateGenericTypes(context);

            var expected = PropertySignature.CreateStatic(_module.CorLibTypeFactory.String);
            Assert.Equal(expected, newSignature, SignatureComparer.Default);
        }

        [Fact]
        public void InstantiatePropertySignatureWithGenericParameterType()
        {
            var signature = PropertySignature.CreateStatic(
                _module.CorLibTypeFactory.String,
                [new GenericParameterSignature(GenericParameterType.Type, 0)]
            );

            var context = new GenericContext(GetProvider(_module.CorLibTypeFactory.String), null);

            var newSignature = signature.InstantiateGenericTypes(context);

            var expected = PropertySignature.CreateStatic(
                _module.CorLibTypeFactory.String,
                [_module.CorLibTypeFactory.String]
            );

            Assert.Equal(expected, newSignature, SignatureComparer.Default);
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
