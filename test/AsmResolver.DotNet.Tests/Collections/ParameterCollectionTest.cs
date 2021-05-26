using System;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.TestCases.Methods;
using Xunit;

namespace AsmResolver.DotNet.Tests.Collections
{
    public class ParameterCollectionTest
    {
        private static MethodDefinition ObtainStaticTestMethod(string name)
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleMethods));
            return type.Methods.First(m => m.Name == name);
        }

        private static MethodDefinition ObtainInstanceTestMethod(string name)
        {
            var module = ModuleDefinition.FromFile(typeof(InstanceMethods).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(InstanceMethods));
            return type.Methods.First(m => m.Name == name);
        }

        [Fact]
        public void ReadEmptyParametersFromStaticMethod()
        {
            var method = ObtainStaticTestMethod(nameof(MultipleMethods.VoidParameterlessMethod));

            Assert.Empty(method.Parameters);
            Assert.Null(method.Parameters.ThisParameter);
        }

        [Fact]
        public void ReadSingleParameterFromStaticMethod()
        {
            var method = ObtainStaticTestMethod(nameof(MultipleMethods.SingleParameterMethod));

            Assert.Single(method.Parameters);
            Assert.Equal("intParameter", method.Parameters[0].Name);
            Assert.True(method.Parameters[0].ParameterType.IsTypeOf("System", "Int32"));
            Assert.Null(method.Parameters.ThisParameter);
        }

        [Fact]
        public void ReadMultipleParametersFromStaticMethod()
        {
            var method = ObtainStaticTestMethod(nameof(MultipleMethods.MultipleParameterMethod));

            Assert.Equal(new[]
            {
                "intParameter",
                "stringParameter",
                "typeDefOrRefParameter"
            }, method.Parameters.Select(p => p.Name));

            Assert.Equal(new[]
            {
                "System.Int32",
                "System.String",
                typeof(MultipleMethods).FullName
            }, method.Parameters.Select(p => p.ParameterType.FullName));

            Assert.Null(method.Parameters.ThisParameter);
        }

        [Fact]
        public void ReadEmptyParametersFromInstanceMethod()
        {
            var method = ObtainInstanceTestMethod(nameof(InstanceMethods.InstanceParameterlessMethod));
            Assert.Empty(method.Parameters);
            Assert.NotNull(method.Parameters.ThisParameter);
            Assert.Equal(nameof(InstanceMethods), method.Parameters.ThisParameter.ParameterType.Name);
        }

        [Fact]
        public void ReadSingleParameterFromInstanceMethod()
        {
            var method = ObtainInstanceTestMethod(nameof(InstanceMethods.InstanceSingleParameterMethod));
            Assert.Single(method.Parameters);
            Assert.Equal(new[]
            {
                "intParameter"
            }, method.Parameters.Select(p => p.Name));
            Assert.NotNull(method.Parameters.ThisParameter);
            Assert.Equal(nameof(InstanceMethods), method.Parameters.ThisParameter.ParameterType.Name);
        }

        [Fact]
        public void ReadMultipleParametersFromInstanceMethod()
        {
            var method = ObtainInstanceTestMethod(nameof(InstanceMethods.InstanceMultipleParametersMethod));
            Assert.Equal(new[]
            {
                "intParameter",
                "stringParameter",
                "boolParameter"

            }, method.Parameters.Select(p => p.Name));

            Assert.Equal(new[]
            {
                "System.Int32",
                "System.String",
                "System.Boolean",
            }, method.Parameters.Select(p => p.ParameterType.FullName));

            Assert.NotNull(method.Parameters.ThisParameter);
            Assert.Equal(nameof(InstanceMethods), method.Parameters.ThisParameter.ParameterType.Name);
        }

        [Fact]
        public void ReadReturnTypeFromStaticParameterlessMethod()
        {
            var method = ObtainStaticTestMethod(nameof(MultipleMethods.VoidParameterlessMethod));
            Assert.True(method.Parameters.ReturnParameter.ParameterType.IsTypeOf("System", "Void"));
        }

        [Fact]
        public void UpdateReturnTypeFromStaticParameterlessMethodShouldThrow()
        {
            var method = ObtainStaticTestMethod(nameof(MultipleMethods.VoidParameterlessMethod));
            Assert.Throws<InvalidOperationException>(() => method.Parameters.ReturnParameter.ParameterType = method.Module.CorLibTypeFactory.Int32);
        }

        [Fact]
        public void UpdateThisParameterParameterTypeShouldThrow()
        {
            var method = ObtainInstanceTestMethod(nameof(InstanceMethods.InstanceParameterlessMethod));
            Assert.Throws<InvalidOperationException>(() => method.Parameters.ThisParameter.ParameterType = method.Module.CorLibTypeFactory.Int32);
        }

        [Fact]
        public void MoveMethodToOtherTypeShouldUpdateThisParameter()
        {
            var method = ObtainInstanceTestMethod(nameof(InstanceMethods.InstanceParameterlessMethod));
            var newType = method.Module.TopLevelTypes.First(t => t.Name == nameof(MultipleMethods));
            method.DeclaringType.Methods.Remove(method);
            newType.Methods.Add(method);

            method.Parameters.PullUpdatesFromMethodSignature();

            Assert.Equal(nameof(MultipleMethods), method.Parameters.ThisParameter.ParameterType.Name);
        }

        [Fact]
        public void TurnInstanceMethodIntoStaticMethodShouldRemoveThisParameter()
        {
            var method = ObtainInstanceTestMethod(nameof(InstanceMethods.InstanceParameterlessMethod));

            method.IsStatic = true;
            method.Signature.HasThis = false;

            method.Parameters.PullUpdatesFromMethodSignature();

            Assert.Null(method.Parameters.ThisParameter);
        }

        [Fact]
        public void TurnStaticMethodIntoInstanceMethodShouldAddThisParameter()
        {
            var method = ObtainStaticTestMethod(nameof(MultipleMethods.VoidParameterlessMethod));

            method.IsStatic = false;
            method.Signature.HasThis = true;

            method.Parameters.PullUpdatesFromMethodSignature();

            Assert.NotNull(method.Parameters.ThisParameter);
            Assert.Equal(nameof(MultipleMethods), method.Parameters.ThisParameter.ParameterType.Name);
        }

        [Fact]
        public void ThisParameterOfCorLibShouldResultInCorLibTypeSignature()
        {
            var module = ModuleDefinition.FromFile(typeof(object).Assembly.Location);
            var type = module.CorLibTypeFactory.Object.Type.Resolve();
            var instanceMethod = type.Methods.First(t => !t.IsStatic);
            var signature = Assert.IsAssignableFrom<CorLibTypeSignature>(instanceMethod.Parameters.ThisParameter.ParameterType);
            Assert.Same(module.CorLibTypeFactory.Object, signature);
        }
    }
}
