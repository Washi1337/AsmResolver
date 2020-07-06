using System;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE.DotNet.Cil;
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
            
            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.Clear();
            instructions.Add(new CilInstruction(CilOpCodes.Ret));
            
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
            var readKey = importer.ImportMethod(typeof(Console).GetMethod("ReadKey", Type.EmptyTypes));

            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.RemoveAt(instructions.Count-1);
            instructions.AddRange(new[]
            {
                new CilInstruction(CilOpCodes.Call, readKey),
                new CilInstruction(CilOpCodes.Pop),
                new CilInstruction(CilOpCodes.Ret),
            });
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveMemberReferenceIndices);
            var newMemberRefs = GetMembers<MemberReference>(newModule, TableIndex.MemberRef);
            
            Assert.Equal(originalMemberRefs, newMemberRefs.Take(originalMemberRefs.Count), Comparer);
        }

    }
}