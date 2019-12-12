using System;
using System.Linq;
using AsmResolver.DotNet.TestCases.Methods;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class MethodDefinitionTest
    {
        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleMethod).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleMethod));
            var method = type.Methods.FirstOrDefault(m => m.Name == nameof(SingleMethod.VoidParameterlessMethod));
            Assert.NotNull(method);
        }

        [Theory]
        [InlineData(".ctor", "System.Void")]
        [InlineData(nameof(MultipleMethods.VoidParameterlessMethod), "System.Void")]
        [InlineData(nameof(MultipleMethods.IntParameterlessMethod), "System.Int32")]
        [InlineData(nameof(MultipleMethods.TypeDefOrRefParameterlessMethod), "AsmResolver.DotNet.TestCases.Methods.MultipleMethods")]
        public void ReadReturnType(string methodName, string expectedReturnType)
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleMethods));
            var method = type.Methods.First(m => m.Name == methodName);
            Assert.Equal(expectedReturnType, method.Signature.ReturnType.FullName);
        }

        [Fact]
        public void ReadDeclaringType()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleMethod).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(SingleMethod).GetMethod(nameof(SingleMethod.VoidParameterlessMethod)).MetadataToken);
            Assert.NotNull(method.DeclaringType);
            Assert.Equal(nameof(SingleMethod), method.DeclaringType.Name);
        }

        [Fact]
        public void ReadEmptyParameterDefinitions()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.VoidParameterlessMethod)).MetadataToken);
            Assert.Empty(method.ParameterDefinitions);
        }

        [Fact]
        public void ReadSingleParameterDefinition()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.SingleParameterMethod)).MetadataToken);
            Assert.Single(method.ParameterDefinitions);
        }

        [Fact]
        public void ReadMultipleParameterDefinitions()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.MultipleParameterMethod)).MetadataToken);
            Assert.Equal(3, method.ParameterDefinitions.Count);
        }
    }
}