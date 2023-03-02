using System;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
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

        private static MethodDefinition ObtainGenericInstanceTestMethod(string name)
        {
            var module = ModuleDefinition.FromFile(typeof(GenericInstanceMethods<,>).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == "GenericInstanceMethods`2");
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
        public void ReadEmptyParametersFromGenericInstanceMethod()
        {
            var method = ObtainGenericInstanceTestMethod(nameof(GenericInstanceMethods<int, int>.InstanceParameterlessMethod));
            Assert.Empty(method.Parameters);
            Assert.NotNull(method.Parameters.ThisParameter);
            var genericInstanceType = Assert.IsAssignableFrom<GenericInstanceTypeSignature>(method.Parameters.ThisParameter.ParameterType);
            Assert.Equal("GenericInstanceMethods`2", genericInstanceType.GenericType.Name);
            Assert.Equal(2, genericInstanceType.TypeArguments.Count);
            Assert.All(genericInstanceType.TypeArguments, (typeArgument, i) =>
            {
                var genericParameterSignature = Assert.IsAssignableFrom<GenericParameterSignature>(typeArgument);
                Assert.Equal(GenericParameterType.Type, genericParameterSignature.ParameterType);
                Assert.Equal(i, genericParameterSignature.Index);
            });
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

        [Fact]
        public void UnnamedParameterShouldResultInDummyName()
        {
            var method = ObtainInstanceTestMethod(nameof(InstanceMethods.InstanceMultipleParametersMethod));
            foreach (var param in method.ParameterDefinitions)
                param.Name = null;
            Assert.All(method.Parameters, p => Assert.Equal(p.Name, $"A_{p.MethodSignatureIndex}"));
        }

        [Fact]
        public void GetOrCreateDefinitionShouldCreateNewDefinition()
        {
            var dummyModule = new ModuleDefinition("TestModule");
            var corLibTypesFactory = dummyModule.CorLibTypeFactory;
            var method = new MethodDefinition("TestMethodNoParameterDefinitions",
                MethodAttributes.Public | MethodAttributes.Static,
                MethodSignature.CreateStatic(corLibTypesFactory.Void, corLibTypesFactory.Int32));

            var param = Assert.Single(method.Parameters);

            Assert.Null(param.Definition);
            var definition = param.GetOrCreateDefinition();

            Assert.Equal(param.Sequence, definition.Sequence);
            Assert.Equal(Utf8String.Empty, definition.Name);
            Assert.Equal((ParameterAttributes)0, definition.Attributes);
            Assert.Contains(definition, method.ParameterDefinitions);
            Assert.Same(definition, param.Definition);
        }

        [Fact]
        public void GetOrCreateDefinitionShouldReturnExistingDefinition()
        {
            var method = ObtainStaticTestMethod(nameof(MultipleMethods.SingleParameterMethod));

            var param = Assert.Single(method.Parameters);

            var existingDefinition = param.Definition;
            Assert.NotNull(existingDefinition);
            var definition = param.GetOrCreateDefinition();
            Assert.Same(existingDefinition, definition);
        }

        [Fact]
        public void GetOrCreateDefinitionThrowsOnVirtualThisParameter()
        {
            var method = ObtainInstanceTestMethod(nameof(InstanceMethods.InstanceParameterlessMethod));

            Assert.NotNull(method.Parameters.ThisParameter);
            Assert.Throws<InvalidOperationException>(() => method.Parameters.ThisParameter.GetOrCreateDefinition());
        }
    }
}
