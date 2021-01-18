using System.IO;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder.TokenPreservation
{
    public class AssemblyRefTokenPreservationTest : TokenPreservationTestBase
    {
        [Fact]
        public void PreserveAssemblyRefsNoChangeShouldAtLeastHaveOriginalAssemblyRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalAssemblyRefs = GetMembers<AssemblyReference>(module, TableIndex.AssemblyRef);
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveAssemblyReferenceIndices);
            var newAssemblyRefs = GetMembers<AssemblyReference>(newModule, TableIndex.AssemblyRef);
            
            Assert.Equal(originalAssemblyRefs, newAssemblyRefs.Take(originalAssemblyRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveAssemblyRefsWithTypeRefRemovedShouldAtLeastHaveOriginalAssemblyRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalAssemblyRefs = GetMembers<AssemblyReference>(module, TableIndex.AssemblyRef);
            
            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.Clear();
            instructions.Add(CilOpCodes.Ret);
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveAssemblyReferenceIndices);
            var newAssemblyRefs = GetMembers<AssemblyReference>(newModule, TableIndex.AssemblyRef);
            
            Assert.Equal(originalAssemblyRefs, newAssemblyRefs.Take(originalAssemblyRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveAssemblyRefsWithExtraImportShouldAtLeastHaveOriginalAssemblyRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalAssemblyRefs = GetMembers<AssemblyReference>(module, TableIndex.AssemblyRef);
            
            var importer = new ReferenceImporter(module);
            var exists = importer.ImportMethod(typeof(File).GetMethod("Exists", new[] {typeof(string)}));

            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.RemoveAt(instructions.Count - 1);
            instructions.Add(CilOpCodes.Ldstr, "file.txt");
            instructions.Add(CilOpCodes.Call, exists);
            instructions.Add(CilOpCodes.Pop);
            instructions.Add(CilOpCodes.Ret);
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveAssemblyReferenceIndices);
            var newAssemblyRefs = GetMembers<AssemblyReference>(newModule, TableIndex.AssemblyRef);
            
            Assert.Equal(originalAssemblyRefs, newAssemblyRefs.Take(originalAssemblyRefs.Count), Comparer);
        }

    }
}