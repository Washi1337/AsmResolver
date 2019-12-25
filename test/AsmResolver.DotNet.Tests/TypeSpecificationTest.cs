using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class TypeSpecificationTest
    {
        [Fact]
        public void IllegalTypeSpecInTypeDefOrRef()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_IllegalTypeSpecInTypeDefOrRefSig);
            var typeSpec =  (TypeSpecification) module.LookupMember(new MetadataToken(TableIndex.TypeSpec, 1));
            Assert.NotNull(typeSpec);
        }
        
        [Fact]
        public void MaliciousTypeSpecLoop()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_MaliciousTypeSpecLoop);
            var typeSpec =  (TypeSpecification) module.LookupMember(new MetadataToken(TableIndex.TypeSpec, 1));
            Assert.NotNull(typeSpec);
        }
        
    }
}