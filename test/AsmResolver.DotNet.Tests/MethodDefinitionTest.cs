using System.Linq;
using AsmResolver.DotNet.TestCases.Events;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.DotNet.TestCases.Properties;
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
        public void ReadNormalMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleMethod).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(SingleMethod).GetMethod(nameof(SingleMethod.VoidParameterlessMethod)).MetadataToken);
            
            Assert.False(method.IsGetMethod);
            Assert.False(method.IsSetMethod);
            Assert.False(method.IsAddMethod);
            Assert.False(method.IsRemoveMethod);
            Assert.False(method.IsFireMethod);
        }

        [Fact]
        public void ReadIsGetMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleProperty).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(SingleProperty).GetMethod("get_" + nameof(SingleProperty.IntProperty)).MetadataToken);
            
            Assert.True(method.IsGetMethod);
        }

        [Fact]
        public void ReadIsSetMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleProperty).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(SingleProperty).GetMethod("set_" + nameof(SingleProperty.IntProperty)).MetadataToken);
            
            Assert.True(method.IsSetMethod);
        }

        [Fact]
        public void ReadIsAddMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(SingleEvent).GetMethod("add_" + nameof(SingleEvent.SimpleEvent)).MetadataToken);
            
            Assert.True(method.IsAddMethod);
        }

        [Fact]
        public void ReadIsRemoveMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(SingleEvent).GetMethod("remove_" + nameof(SingleEvent.SimpleEvent)).MetadataToken);
            
            Assert.True(method.IsRemoveMethod);
        }

        [Fact]
        public void ReadSignatureIsReturnsValue()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = module.TopLevelTypes
                .First(t => t.Name == nameof(MultipleMethods)).Methods
                .First(m => m.Name == nameof(MultipleMethods.IntParameterlessMethod));
            Assert.True(method.Signature.ReturnsValue);
        }

        [Fact]
        public void ReadSignatureNotReturnsValue()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = module.TopLevelTypes
                .First(t => t.Name == nameof(MultipleMethods)).Methods
                .First(m => m.Name == nameof(MultipleMethods.VoidParameterlessMethod));
            Assert.False(method.Signature.ReturnsValue);
        }
    }
}
