using System;
using System.Collections.Generic;
using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class GenericInstantiationTest
    {
        private static readonly SignatureComparer Comparer = new SignatureComparer();
        private readonly MetadataImage _image;
        private readonly ReferenceImporter _importer;
        private readonly GenericContext _context;
        
        private readonly IList<TypeSignature> _typeArgs;
        private readonly IList<TypeSignature> _methodArgs;

        public GenericInstantiationTest()
        {
            _image = NetAssemblyFactory.CreateAssembly("SomeAssembly", true)
                .NetDirectory.MetadataHeader.LockMetadata();
            _importer = new ReferenceImporter(_image);
            
            var scope = new AssemblyReference("SomeAssemblyRef", new Version(1, 2, 3, 4));

            _typeArgs = new TypeSignature[] {_image.TypeSystem.String, _image.TypeSystem.Int32, _image.TypeSystem.Double};

            _methodArgs = new TypeSignature[] {_image.TypeSystem.Byte, _image.TypeSystem.SByte, _image.TypeSystem.Object};
            
            _context = new GenericContext(
                new GenericInstanceTypeSignature(
                    new TypeReference(scope, "A.B.C", "M"),
                    _typeArgs),
                new GenericInstanceMethodSignature(
                    _methodArgs));
        }

        private IList<TypeSignature> GetTypeArguments(GenericParameterType type)
        {
            switch (type)
            {
                case GenericParameterType.Type:
                    return _typeArgs;
                case GenericParameterType.Method:
                    return _methodArgs;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void GenericParameterSignatureType(int index)
        {
            var genericParam = new GenericParameterSignature(GenericParameterType.Type, index);
            Assert.Equal(_typeArgs[index], genericParam.InstantiateGenericTypes(_context), Comparer);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void GenericParameterSignatureMethod(int index)
        {
            var genericParam = new GenericParameterSignature(GenericParameterType.Type, index);
            Assert.Equal(_typeArgs[index], genericParam.InstantiateGenericTypes(_context), Comparer);
        }

        [Theory]
        [InlineData(GenericParameterType.Type, 0)]
        [InlineData(GenericParameterType.Type, 1)]
        [InlineData(GenericParameterType.Type, 2)]
        [InlineData(GenericParameterType.Method, 0)]
        [InlineData(GenericParameterType.Method, 1)]
        [InlineData(GenericParameterType.Method, 2)]
        public void SzArrayType(GenericParameterType parameterType, int index)
        {
            // 0![]
            var signature = new SzArrayTypeSignature(new GenericParameterSignature(parameterType, index));
            Assert.Equal(new SzArrayTypeSignature(GetTypeArguments(parameterType)[index]),
                signature.InstantiateGenericTypes(_context), Comparer);

        }

        [Theory]
        [InlineData(GenericParameterType.Type, 0)]
        [InlineData(GenericParameterType.Type, 1)]
        [InlineData(GenericParameterType.Type, 2)]
        [InlineData(GenericParameterType.Method, 0)]
        [InlineData(GenericParameterType.Method, 1)]
        [InlineData(GenericParameterType.Method, 2)]
        public void ArrayType(GenericParameterType parameterType, int index)
        {
            // 0![0..10]
            var genericParameter = new GenericParameterSignature(parameterType, index);
            var signature = new ArrayTypeSignature(genericParameter)
            {
                Dimensions = {new ArrayDimension(10, 0)}
            };
            
            Assert.Equal(new ArrayTypeSignature(GetTypeArguments(parameterType)[index])
            {
                Dimensions = {new ArrayDimension(10,0)}
            }, signature.InstantiateGenericTypes(_context), Comparer);
            
        }

        [Theory]
        [InlineData(
            new[] {GenericParameterType.Type, GenericParameterType.Type, GenericParameterType.Type},
            new[] {0, 1, 2})]
        [InlineData(
            new[] {GenericParameterType.Method, GenericParameterType.Method, GenericParameterType.Method},
            new[] {0, 1, 2})]
        [InlineData(
            new[] {GenericParameterType.Type, GenericParameterType.Method, GenericParameterType.Type},
            new[] {2, 0, 1})]
        public void GenericType(GenericParameterType[] parameterTypes, int[] parameterIndices)
        {
            // Tuple<!0, !1, !2>
            var genericInstance = new GenericInstanceTypeSignature(_importer.ImportType(typeof(Tuple<,,>)),
                new GenericParameterSignature(parameterTypes[0], parameterIndices[0]),
                new GenericParameterSignature(parameterTypes[1], parameterIndices[1]),
                new GenericParameterSignature(parameterTypes[2], parameterIndices[2])
            );

            Assert.Equal(new GenericInstanceTypeSignature(_importer.ImportType(typeof(Tuple<,,>)),
                GetTypeArguments(parameterTypes[0])[parameterIndices[0]],
                GetTypeArguments(parameterTypes[1])[parameterIndices[1]],
                GetTypeArguments(parameterTypes[2])[parameterIndices[2]]
            ), genericInstance.InstantiateGenericTypes(_context), Comparer);
        }

        [Theory]
        [InlineData(
            new[] {GenericParameterType.Type, GenericParameterType.Type, GenericParameterType.Type, GenericParameterType.Type},
            new[] {0, 1, 2, 0})]
        [InlineData(
            new[] {GenericParameterType.Method, GenericParameterType.Method, GenericParameterType.Method, GenericParameterType.Method},
            new[] {0, 1, 2, 0})]
        [InlineData(
            new[] {GenericParameterType.Type, GenericParameterType.Method, GenericParameterType.Type, GenericParameterType.Method},
            new[] {2, 0, 1, 1})]
        public void NestedGenericTypes(GenericParameterType[] parameterTypes, int[] parameterIndices)
        {
            // Tuple<0!, List<!0>, Dictionary<!0, !1>>
            var genericInstance = new GenericInstanceTypeSignature(_importer.ImportType(typeof(Tuple<,,>)),
                new GenericParameterSignature(parameterTypes[0], parameterIndices[0]),
                new GenericInstanceTypeSignature(_importer.ImportType(typeof(List<>)),
                    new GenericParameterSignature(parameterTypes[1], parameterIndices[1])),
                new GenericInstanceTypeSignature(_importer.ImportType(typeof(Dictionary<,>)),
                    new GenericParameterSignature(parameterTypes[2], parameterIndices[2]),
                    new GenericParameterSignature(parameterTypes[3], parameterIndices[3]))
            );

            Assert.Equal(new GenericInstanceTypeSignature(_importer.ImportType(typeof(Tuple<,,>)),
                GetTypeArguments(parameterTypes[0])[parameterIndices[0]],
                new GenericInstanceTypeSignature(_importer.ImportType(typeof(List<>)),
                    GetTypeArguments(parameterTypes[1])[parameterIndices[1]]),
                new GenericInstanceTypeSignature(_importer.ImportType(typeof(Dictionary<,>)),
                    GetTypeArguments(parameterTypes[2])[parameterIndices[2]],
                    GetTypeArguments(parameterTypes[3])[parameterIndices[3]])
            ), genericInstance.InstantiateGenericTypes(_context), Comparer);
        }



    }
}