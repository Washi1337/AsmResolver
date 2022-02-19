using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder.TokenPreservation
{
    public class TypeRefTokenPreservationTest : TokenPreservationTestBase
    {
        [Fact]
        public void PreserveTypeRefsNoChangeShouldAtLeastHaveOriginalTypeRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalTypeRefs = GetMembers<TypeReference>(module, TableIndex.TypeRef);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeReferenceIndices);
            var newTypeRefs = GetMembers<TypeReference>(newModule, TableIndex.TypeRef);

            Assert.Equal(originalTypeRefs, newTypeRefs.Take(originalTypeRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveTypeRefsWithTypeRefRemovedShouldAtLeastHaveOriginalTypeRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalTypeRefs = GetMembers<TypeReference>(module, TableIndex.TypeRef);

            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.Clear();
            instructions.Add(CilOpCodes.Ret);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeReferenceIndices);
            var newTypeRefs = GetMembers<TypeReference>(newModule, TableIndex.TypeRef);

            Assert.Equal(originalTypeRefs, newTypeRefs.Take(originalTypeRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveTypeRefsWithExtraImportShouldAtLeastHaveOriginalTypeRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalTypeRefs = GetMembers<TypeReference>(module, TableIndex.TypeRef);

            var importer = new ReferenceImporter(module);
            var readKey = importer.ImportMethod(typeof(Console).GetMethod("ReadKey", Type.EmptyTypes));

            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.RemoveAt(instructions.Count - 1);
            instructions.Add(CilOpCodes.Call, readKey);
            instructions.Add(CilOpCodes.Pop);
            instructions.Add(CilOpCodes.Ret);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeReferenceIndices);
            var newTypeRefs = GetMembers<TypeReference>(newModule, TableIndex.TypeRef);

            Assert.Equal(originalTypeRefs, newTypeRefs.Take(originalTypeRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveDuplicatedTypeRefs()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var metadata = image.DotNetDirectory!.Metadata!;
            var strings = metadata.GetStream<StringsStream>();
            var table = metadata
                .GetStream<TablesStream>()
                .GetTable<TypeReferenceRow>();

            // Duplicate Object row.
            var objectRow = table.First(t => strings.GetStringByIndex(t.Name) == "Object");
            table.Add(objectRow);

            // Open module from modified image.
            var module = ModuleDefinition.FromImage(image);

            // Obtain references to Object.
            var objectReferences = module
                .GetImportedTypeReferences()
                .Where(t => t.Name == "Object")
                .ToArray();

            Assert.Equal(2, objectReferences.Length);

            // Rebuild with preservation.
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeReferenceIndices);

            var newObjectReferences = newModule
                .GetImportedTypeReferences()
                .Where(t => t.Name == "Object")
                .ToArray();

            Assert.Equal(
                objectReferences.Select(r => r.MetadataToken).ToHashSet(),
                newObjectReferences.Select(r => r.MetadataToken).ToHashSet());
        }
    }
}
