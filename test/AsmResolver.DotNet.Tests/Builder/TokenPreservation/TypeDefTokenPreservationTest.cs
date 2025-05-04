using System;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder.TokenPreservation
{
    public class TypeDefTokenPreservationTest : TokenPreservationTestBase
    {
        [Fact]
        public void PreserveTypeDefsNoChangeShouldResultInSameTypeDefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);
            var originalTypeDefs = GetMembers<TypeDefinition>(module, TableIndex.TypeDef);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices);
            var newTypeDefs = GetMembers<TypeDefinition>(newModule, TableIndex.TypeDef);

            Assert.Equal(originalTypeDefs, newTypeDefs.Take(originalTypeDefs.Count), Comparer);
        }

        [Fact]
        public void PreserveTypeDefsAddTypeToEndShouldResultInSameTypeDefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);
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
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);
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
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);
            for (int i = 0; i < 10; i++)
                module.TopLevelTypes.Add(new TypeDefinition("Namespace", $"Type{i.ToString()}", TypeAttributes.Public));
            module = RebuildAndReloadModule(module, MetadataBuilderFlags.None);

            const int indexToRemove = 2;

            var originalTypeDefs = GetMembers<TypeDefinition>(module, TableIndex.TypeDef);
            module.TopLevelTypes.RemoveAt(indexToRemove);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices);
            var newTypeDefs = GetMembers<TypeDefinition>(newModule, TableIndex.TypeDef);

            Assert.All(Enumerable.Range(0, originalTypeDefs.Count), i =>
            {
                if (i != indexToRemove)
                    Assert.Equal(originalTypeDefs[i], newTypeDefs[i], Comparer);
            });
        }

        [Fact]
        public void PreserveTypeDefsAfterInsertingNewModuleTypeShouldThrow()
        {
            // Prepare.
            var module = new ModuleDefinition("SomeModule", KnownCorLibs.SystemRuntime_v8_0_0_0);
            module = RebuildAndReloadModule(module, MetadataBuilderFlags.None);

            // Insert new module type, but keep original module type around.
            module.GetOrCreateModuleType().Name = "<NotModule>";
            var newModuleType = module.GetOrCreateModuleType();

            // Rebuild with type preservation should conflict.
            var exception = Assert.Throws<AggregateException>(() => RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices));
            Assert.Contains(exception.InnerExceptions, ex =>
            {
                return ex is MetadataTokenConflictException conflict
                    && (conflict.Member1 == newModuleType || conflict.Member2 == newModuleType)
                    && conflict.Token.Rid == 1;
            });
        }

        [Fact]
        public void PreserveTypeDefsAfterInsertingNewModuleTypeAndRemovingOldShouldNotThrow()
        {
            // Prepare.
            var module = new ModuleDefinition("SomeModule", KnownCorLibs.SystemRuntime_v8_0_0_0);
            var moduleType = module.GetOrCreateModuleType();
            moduleType.Fields.Add(new FieldDefinition("SomeField", FieldAttributes.Static, module.CorLibTypeFactory.Int32));
            module = RebuildAndReloadModule(module, MetadataBuilderFlags.None);

            // Insert new module type, and remove original module type.
            module.TopLevelTypes.Remove(module.GetModuleType());
            moduleType = module.GetOrCreateModuleType();
            moduleType.Fields.Add(new FieldDefinition("OtherField", FieldAttributes.Static, module.CorLibTypeFactory.Int16));

            // Rebuild should not conflict.
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices);

            // Assert that the module type was replaced with the new one.
            var newModuleType = newModule.GetModuleType();
            Assert.NotNull(newModuleType);
            var fields = Assert.Single(newModuleType.Fields);
            Assert.Equal("OtherField", fields.Name);
        }
    }
}
