using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class TypeSpecificationTest
    {
        [Fact]
        public void MaliciousTypeSpecLoop()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorl_MaliciousTypeSpecLoop);
            var typeSpec =  (TypeSpecification) module.LookupMember(new MetadataToken(TableIndex.TypeSpec, 1));
            Assert.NotNull(typeSpec);
        }
        
    }
}