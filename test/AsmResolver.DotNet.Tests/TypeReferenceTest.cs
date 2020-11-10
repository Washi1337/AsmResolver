using System.Linq;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class TypeReferenceTest
    {
        [Fact]
        public void ReadAssemblyRefScope()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var typeRef = (TypeReference) module.LookupMember(new MetadataToken(TableIndex.TypeRef, 13));
            Assert.Equal("mscorlib", typeRef.Scope.Name);
        }
        
        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var typeRef = (TypeReference) module.LookupMember(new MetadataToken(TableIndex.TypeRef, 13));
            Assert.Equal("Console", typeRef.Name);
        }
        
        [Fact]
        public void ReadNamespace()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var typeRef = (TypeReference) module.LookupMember(new MetadataToken(TableIndex.TypeRef, 13));
            Assert.Equal("System", typeRef.Namespace);
        }
    }
}