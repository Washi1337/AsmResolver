using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types.Parsing;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class TypeNameParserTest
    {
        private readonly ModuleDefinition _module;
        private readonly SignatureComparer _comparer;

        public TypeNameParserTest()
        {
            _module = new ModuleDefinition("DummyModule", KnownCorLibs.SystemPrivateCoreLib_v4_0_0_0);
            _comparer = new SignatureComparer();
        }
        
        [Fact]
        public void SimpleTypeNoNamespace()
        {
            const string name = "MyType";
            var type = TypeNameParser.Parse(_module, name);
            Assert.Equal(new TypeReference(_module, null, name).ToTypeSignature(), type, _comparer);
        }
        
        [Theory]
        [InlineData("MyNamespace")]
        [InlineData("MyNamespace.SubNamespace")]
        [InlineData("MyNamespace.SubNamespace.SubSubNamespace")]
        public void SimpleTypeWithNamespace(string ns)
        {
            const string name = "MyType";
            var type = TypeNameParser.Parse(_module, $"{ns}.{name}");
            Assert.Equal(new TypeReference(_module, ns, name).ToTypeSignature(), type, _comparer);
        }
        
        [Fact]
        public void NestedType()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";
            const string nestedType = "MyNestedType";
            var expectedTypeRef = new TypeReference(new TypeReference(_module, ns, name), null, nestedType);
            
            var type = TypeNameParser.Parse(_module, $"{ns}.{name}+{nestedType}");
            Assert.Equal(expectedTypeRef.ToTypeSignature(), type, _comparer);
        }
    }
}