using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class TokenAllocatorTest
    {
        [Fact]
        public void AsigningAvailableTokenShouldSetMetadataToken()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var typeRef = (TypeReference) module.LookupMember(new MetadataToken(TableIndex.TypeRef, 13));
            var nextToken = module.TokenAllocator.GetNextAvailableToken(TableIndex.TypeRef);
            module.TokenAllocator.AssignNextAvailableToken(typeRef);

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
        public void NextAvailableTokenShouldChangeAfterAsigning()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var typeRef = (TypeReference)module.LookupMember(new MetadataToken(TableIndex.TypeRef, 13));
            var nextToken = module.TokenAllocator.GetNextAvailableToken(TableIndex.TypeRef);
            module.TokenAllocator.AssignNextAvailableToken(typeRef);
            module.TokenAllocator.AssignNextAvailableToken(typeRef);

            Assert.Equal(nextToken.Rid + 1, typeRef.MetadataToken.Rid);
        }
    }
}