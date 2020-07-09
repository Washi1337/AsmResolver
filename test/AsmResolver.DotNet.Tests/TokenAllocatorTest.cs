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
        public void AsigningAvailableTokenShouldSetMetadataToken()
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
    }
}