using System;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder.TokenPreservation
{
    public class TypeRefTokenPreservationTest : TokenPreservationTestBase
    {
        [Fact]
        public void PreserveTypeRefsNoChangeShouldAtLeastHaveOriginalTypeRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);
            var originalTypeRefs = GetMembers<TypeReference>(module, TableIndex.TypeRef);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeReferenceIndices);
            var newTypeRefs = GetMembers<TypeReference>(newModule, TableIndex.TypeRef);

            Assert.Equal(originalTypeRefs, newTypeRefs.Take(originalTypeRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveTypeRefsWithTypeRefRemovedShouldAtLeastHaveOriginalTypeRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);
            var originalTypeRefs = GetMembers<TypeReference>(module, TableIndex.TypeRef);

            var instructions = module.ManagedEntryPointMethod!.CilMethodBody!.Instructions;
            instructions.Clear();
            instructions.Add(CilOpCodes.Ret);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeReferenceIndices);
            var newTypeRefs = GetMembers<TypeReference>(newModule, TableIndex.TypeRef);

            Assert.Equal(originalTypeRefs, newTypeRefs.Take(originalTypeRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveTypeRefsWithExtraImportShouldAtLeastHaveOriginalTypeRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);
            var originalTypeRefs = GetMembers<TypeReference>(module, TableIndex.TypeRef);

            var importer = new ReferenceImporter(module);
            var readKey = importer.ImportMethod(typeof(Console).GetMethod("ReadKey", Type.EmptyTypes)!);

            var instructions = module.ManagedEntryPointMethod!.CilMethodBody!.Instructions;
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
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters.PEReaderParameters);
            var metadata = image.DotNetDirectory!.Metadata!;
            var strings = metadata.GetStream<StringsStream>();
            var table = metadata
                .GetStream<TablesStream>()
                .GetTable<TypeReferenceRow>();

            // Duplicate Object row.
            var objectRow = table.First(t => strings.GetStringByIndex(t.Name) == "Object");
            table.Add(objectRow);

            // Open module from modified image.
            var module = ModuleDefinition.FromImage(image, TestReaderParameters);

            // Obtain references to Object.
            var references = module
                .GetImportedTypeReferences()
                .Where(t => t.Name == "Object")
                .ToArray();

            Assert.Equal(2, references.Length);

            // Rebuild with preservation.
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeReferenceIndices);

            var newObjectReferences = newModule
                .GetImportedTypeReferences()
                .Where(t => t.Name == "Object")
                .ToArray();

            Assert.Equal(
                references.Select(r => r.MetadataToken).ToHashSet(),
                newObjectReferences.Select(r => r.MetadataToken).ToHashSet());
        }

        [Fact]
        public void PreserveDuplicatedTypeRefsInBaseType()
        {
            // Prepare temp module with two references to System.Object
            var module = new ModuleDefinition("Test");
            var assembly = new AssemblyDefinition("Test", new Version(1, 0, 0, 0));
            assembly.Modules.Add(module);

            var ref1 = module.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", "Object");
            var ref2 = module.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", "Object");

            // Force assign new tokens to instruct builder that both type references need to be added.
            module.TokenAllocator.AssignNextAvailableToken(ref1);
            module.TokenAllocator.AssignNextAvailableToken(ref2);

            module.TopLevelTypes.Add(new TypeDefinition(null, "A", TypeAttributes.Public, ref1));
            module.TopLevelTypes.Add(new TypeDefinition(null, "B", TypeAttributes.Public, ref2));

            // Rebuild.
            var image = module.ToPEImage(new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveTypeReferenceIndices));

            // Verify that both object references are still there.
            var newModule = ModuleDefinition.FromImage(image, TestReaderParameters);
            Assert.Equal(2, newModule.GetImportedTypeReferences().Count(t => t.Name == "Object"));
        }

        [Fact]
        public void PreserveNestedTypeRefOrdering()
        {
            // https://github.com/Washi1337/AsmResolver/issues/329

            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_UnusualNestedTypeRefOrder, TestReaderParameters);
            var originalTypeRefs = GetMembers<TypeReference>(module, TableIndex.TypeRef);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeReferenceIndices);
            var newTypeRefs = GetMembers<TypeReference>(newModule, TableIndex.TypeRef);

            Assert.Equal(originalTypeRefs, newTypeRefs.Take(originalTypeRefs.Count), Comparer);
        }

    }
}
