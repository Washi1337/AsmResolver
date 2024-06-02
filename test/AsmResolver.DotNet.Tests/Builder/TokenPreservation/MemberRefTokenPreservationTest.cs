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
    public class MemberRefTokenPreservationTest : TokenPreservationTestBase
    {
        [Fact]
        public void PreserveMemberRefsNoChangeShouldAtLeastHaveOriginalMemberRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalMemberRefs = GetMembers<MemberReference>(module, TableIndex.MemberRef);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveMemberReferenceIndices);
            var newMemberRefs = GetMembers<MemberReference>(newModule, TableIndex.MemberRef);

            Assert.Equal(originalMemberRefs, newMemberRefs.Take(originalMemberRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveMemberRefsWithTypeRefRemovedShouldAtLeastHaveOriginalMemberRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalMemberRefs = GetMembers<MemberReference>(module, TableIndex.MemberRef);

            var instructions = module.ManagedEntryPointMethod!.CilMethodBody!.Instructions;
            instructions.Clear();
            instructions.Add(CilOpCodes.Ret);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveMemberReferenceIndices);
            var newMemberRefs = GetMembers<MemberReference>(newModule, TableIndex.MemberRef);

            Assert.Equal(originalMemberRefs, newMemberRefs.Take(originalMemberRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveMemberRefsWithExtraImportShouldAtLeastHaveOriginalMemberRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalMemberRefs = GetMembers<MemberReference>(module, TableIndex.MemberRef);

            var importer = new ReferenceImporter(module);
            var readKey = importer.ImportMethod(typeof(Console).GetMethod("ReadKey", Type.EmptyTypes)!);

            var instructions = module.ManagedEntryPointMethod!.CilMethodBody!.Instructions;
            instructions.RemoveAt(instructions.Count - 1);
            instructions.Add(CilOpCodes.Call, readKey);
            instructions.Add(CilOpCodes.Pop);
            instructions.Add(CilOpCodes.Ret);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveMemberReferenceIndices);
            var newMemberRefs = GetMembers<MemberReference>(newModule, TableIndex.MemberRef);

            Assert.Equal(originalMemberRefs, newMemberRefs.Take(originalMemberRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveDuplicatedTypeRefs()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var metadata = image.DotNetDirectory!.Metadata!;
            var strings = metadata.GetStream<StringsStream>();
            var table = metadata
                .GetStream<TablesStream>()
                .GetTable<MemberReferenceRow>();

            // Duplicate WriteLine row.
            var writeLineRow = table.First(m => strings.GetStringByIndex(m.Name) == "WriteLine");
            table.Add(writeLineRow);

            // Open module from modified image.
            var module = ModuleDefinition.FromImage(image);

            // Obtain references to Object.
            var references = module
                .GetImportedMemberReferences()
                .Where(t => t.Name == "WriteLine")
                .ToArray();

            Assert.Equal(2, references.Length);

            // Rebuild with preservation.
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveMemberReferenceIndices);

            var newReferences = newModule
                .GetImportedMemberReferences()
                .Where(m => m.Name == "WriteLine")
                .ToArray();

            Assert.Equal(
                references.Select(r => r.MetadataToken).ToHashSet(),
                newReferences.Select(r => r.MetadataToken).ToHashSet());
        }

        [Fact]
        public void PreserveMethodDefinitionParents()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.ArgListTest);
            var reference = (MemberReference) module.ManagedEntryPointMethod!.CilMethodBody!
                .Instructions.First(i => i.OpCode.Code == CilCode.Call)
                .Operand!;


            var newModule = RebuildAndReloadModule(module,
                MetadataBuilderFlags.PreserveMemberReferenceIndices
                | MetadataBuilderFlags.PreserveMethodDefinitionIndices);
            var newReference = (MemberReference) newModule.ManagedEntryPointMethod!.CilMethodBody!
                .Instructions.First(i => i.OpCode.Code == CilCode.Call)
                .Operand!;

            Assert.IsAssignableFrom<MethodDefinition>(reference.Parent);
            Assert.IsAssignableFrom<MethodDefinition>(newReference.Parent);
            Assert.Equal(reference.Parent.Name, newReference.Parent.Name);
        }

    }
}
