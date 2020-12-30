using System.Linq;
using AsmResolver.DotNet.Signatures.Types.Parsing;
using Xunit;

namespace AsmResolver.DotNet.Tests.Signatures
{
    public class TypeNameBuilderTest
    {
        private readonly ModuleDefinition _module;
        
        public TypeNameBuilderTest()
        {
            _module = new ModuleDefinition("DummyModule", KnownCorLibs.SystemPrivateCoreLib_v4_0_0_0);
        }

        [Fact]
        public void NameWithDotShouldBeEscaped()
        {
            var type = new TypeReference(_module, _module, "Company.ProductName", "Class.Name");
            string name = TypeNameBuilder.GetAssemblyQualifiedName(type.ToTypeSignature());
            Assert.Contains("Class\\.Name", name);
        }

        [Fact]
        public void NamespaceShouldNotBeEscaped()
        {
            var type = new TypeReference(_module, _module, "Company.ProductName", "ClassName");
            string name = TypeNameBuilder.GetAssemblyQualifiedName(type.ToTypeSignature());
            Assert.DoesNotContain('\\', name);
            Assert.Contains("Company.ProductName", name);
        }
        
    }
}