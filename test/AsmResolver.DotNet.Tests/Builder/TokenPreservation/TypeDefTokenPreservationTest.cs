using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder.TokenPreservation
{
    public class TypeDefTokenPreservationTest : TokenPreservationTestBase
    {
        [Fact]
        public void PreserveTypeDefsNoChangeShouldResultInSameTypeDefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalTypeDefs = GetMembers<TypeDefinition>(module, TableIndex.TypeDef);
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices);
            var newTypeDefs = GetMembers<TypeDefinition>(newModule, TableIndex.TypeDef);
            
            Assert.Equal(originalTypeDefs, newTypeDefs.Take(originalTypeDefs.Count), Comparer);
        }

        [Fact]
        public void PreserveTypeDefsAddTypeToEndShouldResultInSameTypeDefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalTypeDefs = GetMembers<TypeDefinition>(module, TableIndex.TypeDef);

            var newType = new TypeDefinition("Namespace", "Name", TypeAttributes.Public);
            module.TopLevelTypes.Add(newType);
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices);
            var newTypeDefs = GetMembers<TypeDefinition>(newModule, TableIndex.TypeDef);
            
            Assert.Equal(originalTypeDefs, newTypeDefs.Take(originalTypeDefs.Count), Comparer);
            Assert.Contains(newType, newTypeDefs, Comparer);
        }

        [Fact]
        public void PreserveTypeDefsInsertTypeShouldResultInSameTypeDefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalTypeDefs = GetMembers<TypeDefinition>(module, TableIndex.TypeDef);
            
            var newType = new TypeDefinition("Namespace", "Name", TypeAttributes.Public);
            module.TopLevelTypes.Insert(1, newType);
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices);
            var newTypeDefs = GetMembers<TypeDefinition>(newModule, TableIndex.TypeDef);
            
            Assert.Equal(originalTypeDefs, newTypeDefs.Take(originalTypeDefs.Count), Comparer);
            Assert.Contains(newType, newTypeDefs, Comparer);
        }

        [Fact]
        public void PreserveTypeDefsRemoveTypeShouldStuffTypeDefSlots()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            for (int i = 0; i < 10; i++)
                module.TopLevelTypes.Add(new TypeDefinition("Namespace", $"Type{i.ToString()}", TypeAttributes.Public));
            module = RebuildAndReloadModule(module, MetadataBuilderFlags.None);

            const int indexToRemove = 2;
            
            var originalTypeDefs = GetMembers<TypeDefinition>(module, TableIndex.TypeDef);
            module.TopLevelTypes.RemoveAt(indexToRemove);
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices);
            var newTypeDefs = GetMembers<TypeDefinition>(newModule, TableIndex.TypeDef);

            for (int i = 0; i < originalTypeDefs.Count; i++)
            {
                if (i != indexToRemove)
                    Assert.Equal(originalTypeDefs[i], newTypeDefs[i], Comparer);
            }
        }

    }
}