using System.Linq;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class MemberReferenceTest
    {
        [Fact]
        public void ResolveForwardedMethod()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.ForwarderRefTest);
            var forwarder = ModuleDefinition.FromBytes(Properties.Resources.ForwarderLibrary).Assembly!;
            var library = ModuleDefinition.FromBytes(Properties.Resources.ActualLibrary).Assembly!;

            module.MetadataResolver.AssemblyResolver.AddToCache(forwarder, forwarder);
            module.MetadataResolver.AssemblyResolver.AddToCache(library, library);
            forwarder.ManifestModule!.MetadataResolver.AssemblyResolver.AddToCache(library, library);

            var reference = module
                .GetImportedMemberReferences()
                .First(m => m.IsMethod && m.Name == "StaticMethod");

            var definition = reference.Resolve();
            Assert.NotNull(definition);
        }
    }
}
