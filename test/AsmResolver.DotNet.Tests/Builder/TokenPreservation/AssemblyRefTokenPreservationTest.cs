using System.IO;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Strings;
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

            var instructions = module.ManagedEntryPointMethod!.CilMethodBody!.Instructions;
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
            var exists = importer.ImportMethod(typeof(File).GetMethod("Exists", new[] {typeof(string)})!);

            var instructions = module.ManagedEntryPointMethod!.CilMethodBody!.Instructions;
            instructions.RemoveAt(instructions.Count - 1);
            instructions.Add(CilOpCodes.Ldstr, "file.txt");
            instructions.Add(CilOpCodes.Call, exists);
            instructions.Add(CilOpCodes.Pop);
            instructions.Add(CilOpCodes.Ret);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveAssemblyReferenceIndices);
            var newAssemblyRefs = GetMembers<AssemblyReference>(newModule, TableIndex.AssemblyRef);

            Assert.Equal(originalAssemblyRefs, newAssemblyRefs.Take(originalAssemblyRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveDuplicatedAssemblyRefs()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var metadata = image.DotNetDirectory!.Metadata!;
            var strings = metadata.GetStream<StringsStream>();
            var table = metadata
                .GetStream<TablesStream>()
                .GetTable<AssemblyReferenceRow>();

            // Duplicate mscorlib row.
            var corlibRow = table.First(a => strings.GetStringByIndex(a.Name) == "mscorlib");
            table.Add(corlibRow);

            // Open module from modified image.
            var module = ModuleDefinition.FromImage(image);

            // Obtain references to mscorlib.
            var references = module.AssemblyReferences
                .Where(t => t.Name == "mscorlib")
                .ToArray();

            Assert.Equal(2, references.Length);

            // Rebuild with preservation.
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveAssemblyReferenceIndices);

            var newReferences = newModule
                .AssemblyReferences
                .Where(t => t.Name == "mscorlib")
                .ToArray();

            Assert.Equal(
                references.Select(r => r.MetadataToken).ToHashSet(),
                newReferences.Select(r => r.MetadataToken).ToHashSet());
        }
    }
}
