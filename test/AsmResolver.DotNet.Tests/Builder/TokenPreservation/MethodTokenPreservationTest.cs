using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder.TokenPreservation
{
    public class MethodTokenPreservationTest : TokenPreservationTestBase
    {
        private static ModuleDefinition CreateSampleMethodDefsModule(int typeCount, int methodsPerType, int parametersPerMethod = 0)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);

            for (int i = 0; i < typeCount; i++)
            {
                var dummyType = new TypeDefinition("Namespace", $"Type{i.ToString()}",
                    TypeAttributes.Public | TypeAttributes.Abstract,
                    module.CorLibTypeFactory.Object.Type);

                module.TopLevelTypes.Add(dummyType);
                for (int j = 0; j < methodsPerType; j++)
                    dummyType.Methods.Add(CreateDummyMethod(module, $"Method{j}", parametersPerMethod));
            }

            return RebuildAndReloadModule(module, MetadataBuilderFlags.None);
        }

        private static MethodDefinition CreateDummyMethod(ModuleDefinition module, string name, int parameterCount = 0)
        {
            var method = new MethodDefinition(name,
                MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.Virtual
                | MethodAttributes.NewSlot,
                MethodSignature.CreateInstance(module.CorLibTypeFactory.Void));

            for (ushort i = 1; i <= parameterCount; i++)
            {
                method.Signature.ParameterTypes.Add(module.CorLibTypeFactory.Object);
                method.ParameterDefinitions.Add(new ParameterDefinition(i, $"{name}Arg{i}", 0));
            }

            method.Parameters.PullUpdatesFromMethodSignature();

            return method;
        }

        [Fact]
        public void PreserveMethodDefsNoChange()
        {
            var module = CreateSampleMethodDefsModule(10, 10);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveMethodDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Methods);
        }

        [Fact]
        public void PreserveMethodDefsChangeOrderOfTypes()
        {
            var module = CreateSampleMethodDefsModule(10, 10);

            const int swapIndex = 3;
            var type = module.TopLevelTypes[swapIndex];
            module.TopLevelTypes.RemoveAt(swapIndex);
            module.TopLevelTypes.Insert(swapIndex + 1, type);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveMethodDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Methods);
        }

        [Fact]
        public void PreserveMethodDefsChangeOrderOfMethodsInType()
        {
            var module = CreateSampleMethodDefsModule(10, 10);

            const int swapIndex = 3;
            var type = module.TopLevelTypes[2];
            var method = type.Methods[swapIndex];
            type.Methods.RemoveAt(swapIndex);
            type.Methods.Insert(swapIndex + 1, method);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveMethodDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Methods);
        }

        [Fact]
        public void PreserveMethodDefsAddExtraMethod()
        {
            var module = CreateSampleMethodDefsModule(10, 10);

            var type = module.TopLevelTypes[2];
            var method = CreateDummyMethod(module, "ExtraMethod");
            type.Methods.Insert(3, method);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveMethodDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Methods);
        }

        [Fact]
        public void PreserveMethodDefsRemoveMethod()
        {
            var module = CreateSampleMethodDefsModule(10, 10);

            var type = module.TopLevelTypes[2];
            const int indexToRemove = 3;
            var method = type.Methods[indexToRemove];
            type.Methods.RemoveAt(indexToRemove);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveMethodDefinitionIndices);

            AssertSameTokens(module, newModule, m => m.Methods, method.MetadataToken);
        }

        [Fact]
        public void PreserveMethodDefsAddMethodWithParameters()
        {
            var module = CreateSampleMethodDefsModule(10, 10, 2);

            var type = module.TopLevelTypes[2];
            var method = CreateDummyMethod(module, "ExtraMethod");
            method.Signature.ParameterTypes.Add(module.CorLibTypeFactory.Int32);
            type.Methods.Insert(3, method);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveMethodDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Methods);
            AssertSameTokens(module, newModule, t => t.Methods.SelectMany(m => m.ParameterDefinitions));
        }
    }
}
