using System;
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
            var context = module.RuntimeContext!;

            context.LoadAssembly(Properties.Resources.ForwarderLibrary);
            context.LoadAssembly(Properties.Resources.ActualLibrary);

            var reference = module
                .GetImportedMemberReferences()
                .First(m => m.IsMethod && m.Name == "StaticMethod");

            _ = reference.Resolve(module.RuntimeContext);
        }

        [Fact]
        public void MemberReferenceUnimportedParentIsNotImported()
        {
            var module = new ModuleDefinition("Dummy");

            var freeFloatingTypeDef = new TypeDefinition(null, "TypeName", default);

            var genericType = module.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", "Action`1");

            var genericInstance = genericType.MakeGenericInstanceType(false, [freeFloatingTypeDef.ToTypeSignature(false)]);

            var member = genericInstance.ToTypeDefOrRef().CreateMemberReference("SomeMethod",
                MethodSignature.CreateStatic(module.CorLibTypeFactory.Void));

            Assert.False(member.IsImportedInModule(module));
        }
    }
}
