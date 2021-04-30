using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures;
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

            var image = module.ToPEImage();
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

            var image = module.ToPEImage();
            var newModule = ModuleDefinition.FromImage(image);

            var newReference = (MemberReference) newModule.LookupMember(reference.MetadataToken);
            Assert.Equal(reference.Name, newReference.Name);
        }
    }
}
