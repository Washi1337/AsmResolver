using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
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
        public void ResolveGenericParameterWithEmptyTypeShouldThrow(GenericParameterType parameterType)
        {
            var context = new GenericContext();

            var parameter = new GenericParameterSignature(parameterType, 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => context.GetTypeArgument(parameter));
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
        public void ResolveTypeGenericParameterWithOnlyMethodShouldThrow()
        {
            var genericInstance = new GenericInstanceMethodSignature();
            genericInstance.TypeArguments.Add(_importer.ImportTypeSignature(typeof(string)));
            
            var context = new GenericContext(null, genericInstance);
            
            var parameter = new GenericParameterSignature(GenericParameterType.Type, 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => context.GetTypeArgument(parameter));
        }

        [Fact]
        public void ResolveMethodGenericParameterWithOnlyTypeShouldThrow()
        {
            var genericInstance = new GenericInstanceTypeSignature(_importer.ImportType(typeof(List<>)), false);
            genericInstance.TypeArguments.Add(_importer.ImportTypeSignature(typeof(string)));
            
            var context = new GenericContext(genericInstance, null);
            
            var parameter = new GenericParameterSignature(GenericParameterType.Method, 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => context.GetTypeArgument(parameter));
        }
    }
}