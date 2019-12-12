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

        [Fact]
        public void ReadEmptyParametersStatic()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.VoidParameterlessMethod)).MetadataToken);
            Assert.Empty(method.Parameters);
        }

        [Fact]
        public void ReadSingleParameterStatic()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.SingleParameterMethod)).MetadataToken);
            Assert.Single(method.Parameters);
            Assert.Equal("intParameter", method.Parameters[0].Name);
            Assert.True(method.Parameters[0].ParameterType.IsTypeOf("System", "Int32"));
        }

        [Fact]
        public void ReadMultipleParameterStatic()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.MultipleParameterMethod)).MetadataToken);
            Assert.Equal(3, method.Parameters.Count);
            
            Assert.Equal("intParameter", method.Parameters[0].Name);
            Assert.True(method.Parameters[0].ParameterType.IsTypeOf("System", "Int32"), 
                "Expected first parameter to be of type System.Int32.");
            
            Assert.Equal("stringParameter", method.Parameters[1].Name);
            Assert.True(method.Parameters[1].ParameterType.IsTypeOf("System", "String"), 
                "Expected second parameter to be of type System.String.");
            
            Assert.Equal("typeDefOrRefParameter", method.Parameters[2].Name);
            Assert.True(method.Parameters[2].ParameterType.IsTypeOf("AsmResolver.DotNet.TestCases.Methods", "MultipleMethods"), 
                "Expected third parameter to be of type AsmResolver.TestCases.DotNet.MultipleMethods.");
        }

        [Fact]
        public void ReadEmptyParametersInstance()
        {
            var module = ModuleDefinition.FromFile(typeof(InstanceMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(InstanceMethods).GetMethod(nameof(InstanceMethods.InstanceParameterlessMethod)).MetadataToken);
            Assert.Single(method.Parameters);
            Assert.True(method.Parameters[0].ParameterType.IsTypeOf("AsmResolver.DotNet.TestCases.Methods", "InstanceMethods"),
                "Expected this parameter to be of type AsmResolver.DotNet.TestCases.Methods.InstanceMethods.");
        }

        [Fact]
        public void ReadSingleParameterInstance()
        {
            var module = ModuleDefinition.FromFile(typeof(InstanceMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(InstanceMethods).GetMethod(nameof(InstanceMethods.InstanceSingleParameterMethod)).MetadataToken);
            
            Assert.Equal(2, method.Parameters.Count);
            
            Assert.True(method.Parameters[0].ParameterType.IsTypeOf("AsmResolver.DotNet.TestCases.Methods", "InstanceMethods"),
                "Expected this parameter to be of type AsmResolver.DotNet.TestCases.Methods.InstanceMethods.");
            
            Assert.Equal("intParameter", method.Parameters[1].Name);
            Assert.True(method.Parameters[1].ParameterType.IsTypeOf("System", "Int32"));
        }

        [Fact]
        public void ReadMultipleParameterInstance()
        {
            var module = ModuleDefinition.FromFile(typeof(InstanceMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(InstanceMethods).GetMethod(nameof(InstanceMethods.InstanceMultipleParametersMethod)).MetadataToken);
            Assert.Equal(4, method.Parameters.Count);
            
            Assert.True(method.Parameters[0].ParameterType.IsTypeOf("AsmResolver.DotNet.TestCases.Methods", "InstanceMethods"),
                "Expected this parameter to be of type AsmResolver.DotNet.TestCases.Methods.InstanceMethods.");
            
            Assert.Equal("intParameter", method.Parameters[1].Name);
            Assert.True(method.Parameters[1].ParameterType.IsTypeOf("System", "Int32"), 
                "Expected first parameter to be of type System.Int32.");
            
            Assert.Equal("stringParameter", method.Parameters[2].Name);
            Assert.True(method.Parameters[2].ParameterType.IsTypeOf("System", "String"), 
                "Expected second parameter to be of type System.String.");
            
            Assert.Equal("boolParameter", method.Parameters[3].Name);
            Assert.True(method.Parameters[3].ParameterType.IsTypeOf("System", "Boolean"), 
                "Expected third parameter to be of type System.Boolean");
        }
    }
}