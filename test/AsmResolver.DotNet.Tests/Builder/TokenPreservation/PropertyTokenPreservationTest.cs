using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder.TokenPreservation
{
    public class PropertyTokenPreservationTest : TokenPreservationTestBase
    {
        private static ModuleDefinition CreateSamplePropertyDefsModule(int typeCount, int propertiesPerType)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);

            for (int i = 0; i < typeCount; i++)
            {
                var dummyType = new TypeDefinition("Namespace", $"Type{i.ToString()}",
                    TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);

                module.TopLevelTypes.Add(dummyType);
                for (int j = 0; j < propertiesPerType; j++)
                    dummyType.Properties.Add(CreateDummyProperty(dummyType, $"Property{j}"));
            }

            return RebuildAndReloadModule(module, MetadataBuilderFlags.None);
        }

        private static PropertyDefinition CreateDummyProperty(TypeDefinition dummyType, string name)
        {
            var property = new PropertyDefinition(
                name,
                PropertyAttributes.None,
                PropertySignature.CreateStatic(dummyType.DeclaringModule!.CorLibTypeFactory.Object)
            );

            var getMethod = new MethodDefinition(
                $"get_{property.Name}",
                MethodAttributes.Public | MethodAttributes.Static,
                MethodSignature.CreateStatic(dummyType.DeclaringModule.CorLibTypeFactory.Object)
            );

            getMethod.CilMethodBody = new CilMethodBody
            {
                Instructions =
                {
                    CilOpCodes.Ldnull,
                    CilOpCodes.Ret
                }
            };

            dummyType.Methods.Add(getMethod);
            property.Semantics.Add(new MethodSemantics(getMethod, MethodSemanticsAttributes.Getter));
            return property;
        }

        [Fact]
        public void PreservePropertyDefsNoChange()
        {
            var module = CreateSamplePropertyDefsModule(10, 10);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreservePropertyDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Properties);
        }

        [Fact]
        public void PreservePropertyDefsChangeOrderOfTypes()
        {
            var module = CreateSamplePropertyDefsModule(10, 10);

            const int swapIndex = 3;
            var type = module.TopLevelTypes[swapIndex];
            module.TopLevelTypes.RemoveAt(swapIndex);
            module.TopLevelTypes.Insert(swapIndex + 1, type);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreservePropertyDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Properties);
        }

        [Fact]
        public void PreservePropertyDefsChangeOrderOfPropertiesInType()
        {
            var module = CreateSamplePropertyDefsModule(10, 10);

            const int swapIndex = 3;
            var type = module.TopLevelTypes[2];
            var property = type.Properties[swapIndex];
            type.Properties.RemoveAt(swapIndex);
            type.Properties.Insert(swapIndex + 1, property);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreservePropertyDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Properties);
        }

        [Fact]
        public void PreservePropertyDefsAddExtraProperty()
        {
            var module = CreateSamplePropertyDefsModule(10, 10);

            var type = module.TopLevelTypes[2];
            var property = CreateDummyProperty(type, "ExtraProperty");
            type.Properties.Insert(3, property);

            // Rebuild and verify.
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreservePropertyDefinitionIndices);
            AssertSameTokens(module, newModule, t => t.Properties);
        }

        [Fact]
        public void PreservePropertyDefsRemoveProperty()
        {
            var module = CreateSamplePropertyDefsModule(10, 10);

            var type = module.TopLevelTypes[2];
            const int indexToRemove = 3;
            var Property = type.Properties[indexToRemove];
            type.Properties.RemoveAt(indexToRemove);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreservePropertyDefinitionIndices);

            AssertSameTokens(module, newModule, m => m.Properties, Property.MetadataToken);
        }

    }
}
