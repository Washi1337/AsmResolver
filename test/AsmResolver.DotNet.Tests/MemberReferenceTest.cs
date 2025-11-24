using System.Linq;
using AsmResolver.DotNet.Signatures;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class MemberReferenceTest
    {
        [Fact]
        public void ResolveForwardedMethod()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.ForwarderRefTest, TestReaderParameters);
            var forwarder = ModuleDefinition.FromBytes(Properties.Resources.ForwarderLibrary, TestReaderParameters).Assembly!;
            var library = ModuleDefinition.FromBytes(Properties.Resources.ActualLibrary, TestReaderParameters).Assembly!;

            module.MetadataResolver.AssemblyResolver.AddToCache(forwarder, forwarder);
            module.MetadataResolver.AssemblyResolver.AddToCache(library, library);
            forwarder.ManifestModule!.MetadataResolver.AssemblyResolver.AddToCache(library, library);

            var reference = module
                .GetImportedMemberReferences()
                .First(m => m.IsMethod && m.Name == "StaticMethod");

            var definition = reference.Resolve();
            Assert.NotNull(definition);
        }

        [Fact]
        public void MemberReferenceUnimportedParentIsNotImported()
        {
            var module = new ModuleDefinition("Dummy");

            var freeFloatingTypeDef = new TypeDefinition(null, "TypeName", default);

            var genericType = module.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", "Action`1");

            var genericInstance = genericType.MakeGenericInstanceType(false, freeFloatingTypeDef.ToTypeSignature(false));

            var member = genericInstance.ToTypeDefOrRef().CreateMemberReference("SomeMethod",
                MethodSignature.CreateStatic(module.CorLibTypeFactory.Void));

            Assert.False(member.IsImportedInModule(module));
        }
    }
}
