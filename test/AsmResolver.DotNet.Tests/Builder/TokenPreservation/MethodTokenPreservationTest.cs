using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder.TokenPreservation
{
    public class MethodTokenPreservationTest : TokenPreservationTestBase
    {  
        private static ModuleDefinition CreateSampleMethodDefsModule(int typeCount, int methodsPerType)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);

            for (int i = 0; i < typeCount; i++)
            {
                var dummyType = new TypeDefinition("Namespace", $"Type{i.ToString()}",
                    TypeAttributes.Public | TypeAttributes.Abstract,
                    module.CorLibTypeFactory.Object.Type);

                module.TopLevelTypes.Add(dummyType);
                for (int j = 0; j < methodsPerType; j++)
                    dummyType.Methods.Add(CreateDummyMethod(module, $"Method{j}"));
            }

            return RebuildAndReloadModule(module, MetadataBuilderFlags.None);
        }

        private static MethodDefinition CreateDummyMethod(ModuleDefinition module, string name)
        {
            var method = new MethodDefinition(name,
                MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.Virtual
                | MethodAttributes.NewSlot,
                MethodSignature.CreateInstance(module.CorLibTypeFactory.Void));
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
    }
}