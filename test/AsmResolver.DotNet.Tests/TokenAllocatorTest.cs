using System;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class TokenAllocatorTest
    {
        [Fact]
        public void AssigningAvailableTokenShouldSetMetadataToken()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var typeRef = new TypeReference(module, "", "");
            var nextToken = module.TokenAllocator.GetNextAvailableToken(TableIndex.TypeRef);
            module.TokenAllocator.AssignNextAvailableToken(typeRef);

            Assert.NotEqual((uint)0,nextToken.Rid);
            Assert.Equal(nextToken, typeRef.MetadataToken);
        }

        [Theory]
        [InlineData(TableIndex.TypeRef)]
        [InlineData(TableIndex.TypeDef)]
        [InlineData(TableIndex.Field)]
        [InlineData(TableIndex.MemberRef)]
        [InlineData(TableIndex.Property)]
        [InlineData(TableIndex.Method)]
        [InlineData(TableIndex.AssemblyRef)]
        public void GetNextAvailableTokenShouldReturnTableSize(TableIndex index)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var tableStream = module.DotNetDirectory.Metadata.GetStream<TablesStream>();
            var table = tableStream.GetTable(index);
            var count = (uint)table.Count;

            var nextToken = module.TokenAllocator.GetNextAvailableToken(index);
            Assert.Equal(count + 1, nextToken.Rid);
        }

        [Fact]
        public void NextAvailableTokenShouldChangeAfterAssigning()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var typeRef = new TypeReference(module, "", "");
            var nextToken = module.TokenAllocator.GetNextAvailableToken(TableIndex.TypeRef);
            module.TokenAllocator.AssignNextAvailableToken(typeRef);
            var typeRef2 = new TypeReference(module, "", "");
            module.TokenAllocator.AssignNextAvailableToken(typeRef2);

            Assert.Equal(nextToken.Rid + 1, typeRef2.MetadataToken.Rid);
        }

        [Fact]
        public void AssignTokenToNewUnusedTypeReferenceShouldPreserveAfterBuild()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var reference = new TypeReference(module, module, "SomeNamespace", "SomeType");
            module.TokenAllocator.AssignNextAvailableToken(reference);

            var image = module.ToPEImage(new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveTypeReferenceIndices));
            var newModule = ModuleDefinition.FromImage(image);

            var newReference = (TypeReference) newModule.LookupMember(reference.MetadataToken);
            Assert.Equal(reference.Namespace, newReference.Namespace);
            Assert.Equal(reference.Name, newReference.Name);
        }

        [Fact]
        public void AssignTokenToNewUnusedMemberReferenceShouldPreserveAfterBuild()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var reference = new MemberReference(
                new TypeReference(module, module, "SomeNamespace", "SomeType"),
                "SomeMethod",
                MethodSignature.CreateStatic(module.CorLibTypeFactory.Void));

            module.TokenAllocator.AssignNextAvailableToken(reference);

            var image = module.ToPEImage(new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveMemberReferenceIndices));
            var newModule = ModuleDefinition.FromImage(image);

            var newReference = (MemberReference) newModule.LookupMember(reference.MetadataToken);
            Assert.Equal(reference.Name, newReference.Name);
        }

        [Fact]
        public void AssignTokenOfNextMemberShouldPreserve()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            // Create two dummy fields.
            var fieldType = module.CorLibTypeFactory.Object;
            var field1 = new FieldDefinition("NonAssignedField", FieldAttributes.Static, fieldType);
            var field2 = new FieldDefinition("AssignedField", FieldAttributes.Static, fieldType);

            // Add both.
            var moduleType = module.GetOrCreateModuleType();
            moduleType.Fields.Add(field1);
            moduleType.Fields.Add(field2);

            // Only assign token to the second one, but leave the first one floating.
            module.TokenAllocator.AssignNextAvailableToken(field2);

            // Rebuild.
            var image = module.ToPEImage(new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveAll));
            var newModule = ModuleDefinition.FromImage(image);

            // Verify.
            Assert.Equal(field2.Name, ((FieldDefinition) newModule.LookupMember(field2.MetadataToken)).Name);
        }

        [Fact]
        public void Issue187()
        {
            // https://github.com/Washi1337/AsmResolver/issues/187

            var targetModule = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            var method = new MethodDefinition("test",
                MethodAttributes.Public | MethodAttributes.Static,
                MethodSignature.CreateStatic(targetModule.CorLibTypeFactory.Boolean));

            var body = new CilMethodBody(method);
            body.Instructions.Add(CilOpCodes.Ldc_I4, 0);
            body.Instructions.Add(CilOpCodes.Ret);
            method.CilMethodBody = body;

            targetModule.TopLevelTypes.First().Methods.Add(method);

            var allocator = targetModule.TokenAllocator;
            allocator.AssignNextAvailableToken(method);

            targetModule.GetOrCreateModuleConstructor();
            _ = targetModule.ToPEImage(new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveAll));
        }

        [Fact]
        public void Issue252()
        {
            // https://github.com/Washi1337/AsmResolver/issues/252

            var module = ModuleDefinition.FromFile(typeof(TokenAllocatorTest).Assembly.Location);
            var asmResRef = module.AssemblyReferences.First(a => a.Name == "AsmResolver.DotNet");

            var reference  = new TypeReference(module, asmResRef, "AsmResolver.DotNet", "MethodDefinition");
            var reference2 = new TypeReference(module, asmResRef, "AsmResolver.DotNet", "ModuleDefinition");

            module.TokenAllocator.AssignNextAvailableToken(reference);
            module.TokenAllocator.AssignNextAvailableToken(reference2);

            var image = module.ToPEImage(new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveAll));
            var newModule = ModuleDefinition.FromImage(image);

            var newReference = (TypeReference) newModule.LookupMember(reference.MetadataToken);
            var newReference2 = (TypeReference) newModule.LookupMember(reference2.MetadataToken);

            Assert.Equal(reference.Namespace, newReference.Namespace);
            Assert.Equal(reference.Name, newReference.Name);
            Assert.Equal(reference2.Namespace, newReference2.Namespace);
            Assert.Equal(reference2.Name, newReference2.Name);
        }

    }
}
