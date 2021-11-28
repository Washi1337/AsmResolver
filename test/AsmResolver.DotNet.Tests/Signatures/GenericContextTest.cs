using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;
using Xunit.Sdk;

namespace AsmResolver.DotNet.Tests.Signatures
{
    public class GenericContextTest
    {
        private ReferenceImporter _importer;

        public GenericContextTest()
        {
            var module = new ModuleDefinition("TempModule");
            _importer = new ReferenceImporter(module);
        }

        [Theory]
        [InlineData(GenericParameterType.Type)]
        [InlineData(GenericParameterType.Method)]
        public void ResolveGenericParameterWithEmptyType(GenericParameterType parameterType)
        {
            var context = new GenericContext();

            var parameter = new GenericParameterSignature(parameterType, 0);
            Assert.Equal(parameter, context.GetTypeArgument(parameter));
        }

        [Fact]
        public void ResolveMethodGenericParameterWithMethod()
        {
            var genericInstance = new GenericInstanceMethodSignature();
            genericInstance.TypeArguments.Add(_importer.ImportTypeSignature(typeof(string)));

            var context = new GenericContext(null, genericInstance);

            var parameter = new GenericParameterSignature(GenericParameterType.Method, 0);
            Assert.Equal("System.String", context.GetTypeArgument(parameter).FullName);
        }

        [Fact]
        public void ResolveTypeGenericParameterWithType()
        {
            var genericInstance = new GenericInstanceTypeSignature(_importer.ImportType(typeof(List<>)), false);
            genericInstance.TypeArguments.Add(_importer.ImportTypeSignature(typeof(string)));

            var context = new GenericContext(genericInstance, null);

            var parameter = new GenericParameterSignature(GenericParameterType.Type, 0);
            Assert.Equal("System.String", context.GetTypeArgument(parameter).FullName);
        }

        [Fact]
        public void ResolveTypeGenericParameterWithTypeAndMethod()
        {
            var genericType = new GenericInstanceTypeSignature(_importer.ImportType(typeof(List<>)), false);
            genericType.TypeArguments.Add(_importer.ImportTypeSignature(typeof(string)));

            var genericMethod = new GenericInstanceMethodSignature();
            genericMethod.TypeArguments.Add(_importer.ImportTypeSignature(typeof(int)));

            var context = new GenericContext(genericType, genericMethod);

            var parameter = new GenericParameterSignature(GenericParameterType.Type, 0);
            Assert.Equal("System.String", context.GetTypeArgument(parameter).FullName);
        }

        [Fact]
        public void ResolveMethodGenericParameterWithTypeAndMethod()
        {
            var genericType = new GenericInstanceTypeSignature(_importer.ImportType(typeof(List<>)), false);
            genericType.TypeArguments.Add(_importer.ImportTypeSignature(typeof(string)));

            var genericMethod = new GenericInstanceMethodSignature();
            genericMethod.TypeArguments.Add(_importer.ImportTypeSignature(typeof(int)));

            var context = new GenericContext(genericType, genericMethod);

            var parameter = new GenericParameterSignature(GenericParameterType.Method, 0);
            Assert.Equal("System.Int32", context.GetTypeArgument(parameter).FullName);
        }

        [Fact]
        public void ResolveTypeGenericParameterWithOnlyMethod()
        {
            var genericInstance = new GenericInstanceMethodSignature();
            genericInstance.TypeArguments.Add(_importer.ImportTypeSignature(typeof(string)));

            var context = new GenericContext(null, genericInstance);

            var parameter = new GenericParameterSignature(GenericParameterType.Type, 0);
            Assert.Equal(parameter,context.GetTypeArgument(parameter));
        }

        [Fact]
        public void ResolveMethodGenericParameterWithOnlyType()
        {
            var genericInstance = new GenericInstanceTypeSignature(_importer.ImportType(typeof(List<>)), false);
            genericInstance.TypeArguments.Add(_importer.ImportTypeSignature(typeof(string)));

            var context = new GenericContext(genericInstance, null);

            var parameter = new GenericParameterSignature(GenericParameterType.Method, 0);
            Assert.Equal(parameter, context.GetTypeArgument(parameter));
        }

        [Fact]
        public void ParseGenericFromTypeSpecification()
        {
            var genericInstance = new GenericInstanceTypeSignature(_importer.ImportType(typeof(List<>)), false);
            genericInstance.TypeArguments.Add(_importer.ImportTypeSignature(typeof(string)));
            var typeSpecification = new TypeSpecification(genericInstance);

            var context = GenericContext.FromTypeSpecification(typeSpecification);

            Assert.True(context.HasValue);
            Assert.Equal(genericInstance, context.Value.Type);
            Assert.Null(context.Value.Method);
        }

        [Fact]
        public void ParseGenericFromMethodSpecification()
        {
            var genericParameter = new GenericParameterSignature(GenericParameterType.Method, 0);
            var method = new MethodDefinition("TestMethod", MethodAttributes.Private,
                MethodSignature.CreateStatic(genericParameter));
            var genericInstance = new GenericInstanceMethodSignature();
            genericInstance.TypeArguments.Add(_importer.ImportTypeSignature(typeof(int)));
            var methodSpecification = new MethodSpecification(method, genericInstance);

            var context = GenericContext.FromMethodSpecification(methodSpecification);

            Assert.True(context.HasValue);
            Assert.Null(context.Value.Type);
            Assert.Equal(genericInstance, context.Value.Method);
        }

        [Fact]
        public void ParseGenericFromMethodSpecificationWithTypeSpecification()
        {
            var genericTypeInstance = new GenericInstanceTypeSignature(_importer.ImportType(typeof(List<>)), false);
            genericTypeInstance.TypeArguments.Add(_importer.ImportTypeSignature(typeof(string)));
            var typeSpecification = new TypeSpecification(genericTypeInstance);

            var genericParameter = new GenericParameterSignature(GenericParameterType.Method, 0);
            var method = new MemberReference(typeSpecification, "TestMethod",
                MethodSignature.CreateStatic(genericParameter));

            var genericMethodInstance = new GenericInstanceMethodSignature();
            genericMethodInstance.TypeArguments.Add(_importer.ImportTypeSignature(typeof(int)));
            var methodSpecification = new MethodSpecification(method, genericMethodInstance);


            var context = GenericContext.FromMethodSpecification(methodSpecification);

            Assert.True(context.HasValue);
            Assert.Equal(genericTypeInstance, context.Value.Type);
            Assert.Equal(genericMethodInstance, context.Value.Method);
        }

        [Fact]
        public void ParseGenericFromNotGenericTypeSpecification()
        {
            var type = new TypeDefinition("","Test type", TypeAttributes.Public);
            var notGenericSignature = new TypeDefOrRefSignature(type);

            var typeSpecification = new TypeSpecification(notGenericSignature);

            var context = GenericContext.FromTypeSpecification(typeSpecification);

            Assert.False(context.HasValue);
        }

        [Fact]
        public void ParseGenericFromNotGenericMethodSpecification()
        {
            var type = new TypeDefinition("","Test type", TypeAttributes.Public);
            var notGenericSignature = new TypeDefOrRefSignature(type);

            var method = new MethodDefinition("TestMethod", MethodAttributes.Private,
                MethodSignature.CreateStatic(notGenericSignature));
            var methodSpecification = new MethodSpecification(method, null);

            var context = GenericContext.FromMethodSpecification(methodSpecification);

            Assert.False(context.HasValue);
        }

        [Fact]
        public void ParseGenericFromNotGenericMethodSpecificationWithTypeSpecification()
        {
            var genericTypeInstance = new GenericInstanceTypeSignature(_importer.ImportType(typeof(List<>)), false);
            genericTypeInstance.TypeArguments.Add(_importer.ImportTypeSignature(typeof(string)));
            var typeSpecification = new TypeSpecification(genericTypeInstance);

            var type = new TypeDefinition("","Test type", TypeAttributes.Public);
            var notGenericSignature = new TypeDefOrRefSignature(type);

            var method = new MemberReference(typeSpecification, "TestMethod",
                MethodSignature.CreateStatic(notGenericSignature));
            var methodSpecification = new MethodSpecification(method, null);

            var context = GenericContext.FromMethodSpecification(methodSpecification);

            Assert.True(context.HasValue);
            Assert.Equal(genericTypeInstance, context.Value.Type);
            Assert.Null(context.Value.Method);
        }
    }
}
