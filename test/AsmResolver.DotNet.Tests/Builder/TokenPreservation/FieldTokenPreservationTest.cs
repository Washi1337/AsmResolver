using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder.TokenPreservation
{
    public class FieldTokenPreservationTest : TokenPreservationTestBase
    {
        private static ModuleDefinition CreateSampleFieldDefsModule(int typeCount, int fieldsPerType)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);

            for (int i = 0; i < typeCount; i++)
            {
                var dummyType = new TypeDefinition("Namespace", $"Type{i.ToString()}",
                    TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);

                module.TopLevelTypes.Add(dummyType);
                for (int j = 0; j < fieldsPerType; j++)
                    dummyType.Fields.Add(CreateDummyField(module, $"Field{j}"));
            }

            return RebuildAndReloadModule(module, MetadataBuilderFlags.None);
        }

        private static FieldDefinition CreateDummyField(ModuleDefinition module, string name) => new(
            name,
            FieldAttributes.Public | FieldAttributes.Static,
            module.CorLibTypeFactory.Int32);

        [Fact]
        public void PreserveFieldDefsNoChange()
        {
            var module = CreateSampleFieldDefsModule(10, 10);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveFieldDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Fields);
        }

        [Fact]
        public void PreserveFieldDefsChangeOrderOfTypes()
        {
            var module = CreateSampleFieldDefsModule(10, 10);

            const int swapIndex = 3;
            var type = module.TopLevelTypes[swapIndex];
            module.TopLevelTypes.RemoveAt(swapIndex);
            module.TopLevelTypes.Insert(swapIndex + 1, type);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveFieldDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Fields);
        }

        [Fact]
        public void PreserveFieldDefsChangeOrderOfFieldsInType()
        {
            var module = CreateSampleFieldDefsModule(10, 10);

            const int swapIndex = 3;
            var type = module.TopLevelTypes[2];
            var field = type.Fields[swapIndex];
            type.Fields.RemoveAt(swapIndex);
            type.Fields.Insert(swapIndex + 1, field);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveFieldDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Fields);
        }

        [Fact]
        public void PreserveFieldDefsAddExtraField()
        {
            var module = CreateSampleFieldDefsModule(10, 10);

            var type = module.TopLevelTypes[2];
            var field = CreateDummyField(module, "ExtraField");
            type.Fields.Insert(3, field);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveFieldDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Fields);
        }

        [Fact]
        public void PreserveFieldDefsRemoveField()
        {
            var module = CreateSampleFieldDefsModule(10, 10);

            var type = module.TopLevelTypes[2];
            const int indexToRemove = 3;
            var field = type.Fields[indexToRemove];
            type.Fields.RemoveAt(indexToRemove);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveFieldDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Fields, field.MetadataToken);
        }
    }
}
