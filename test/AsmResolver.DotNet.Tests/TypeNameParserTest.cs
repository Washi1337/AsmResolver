using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
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
            var expected = new TypeReference(_module, null, "MyType").ToTypeSignature();
            var actual = TypeNameParser.Parse(_module, expected.Name);
            Assert.Equal(expected, actual, _comparer);
        }
        
        [Theory]
        [InlineData("MyNamespace")]
        [InlineData("MyNamespace.SubNamespace")]
        [InlineData("MyNamespace.SubNamespace.SubSubNamespace")]
        public void SimpleTypeWithNamespace(string ns)
        {
            var expected = new TypeReference(_module, ns, "MyType").ToTypeSignature();
            var actual = TypeNameParser.Parse(_module, expected.FullName);
            Assert.Equal(expected, actual, _comparer);
        }
        
        [Fact]
        public void NestedType()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";
            const string nestedType = "MyNestedType";
            var expected = new TypeReference(
                    new TypeReference(_module, ns, name),
                    null,
                    nestedType)
                .ToTypeSignature();
            
            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}+{nestedType}");
            Assert.Equal(expected, actual, _comparer);
        }

        [Fact]
        public void TypeWithAssemblyName()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";
            var assemblyRef = new AssemblyReference("MyAssembly", new Version(1, 2, 3, 4));
            var expected = new TypeReference(assemblyRef, ns, name).ToTypeSignature();
            
            
            var actual = TypeNameParser.Parse(_module, 
                $"{ns}.{name}, {assemblyRef.FullName}");
            Assert.Equal(expected, actual, _comparer);
        }

        [Fact]
        public void TypeWithAssemblyNameWithPublicKey()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";
            var assemblyRef = new AssemblyReference(
                "MyAssembly",
                new Version(1, 2, 3, 4),
                false,
                new byte[] {1, 2, 3, 4, 5, 6, 7, 8});
            
            var expected = new TypeReference(assemblyRef, ns, name).ToTypeSignature();

            var actual = TypeNameParser.Parse(_module, 
                $"{ns}.{name}, {assemblyRef.FullName}");
            Assert.Equal(expected, actual, _comparer);
        }

        [Fact]
        public void SimpleArrayType()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";
            
            var elementType = new TypeReference(_module, ns, name).ToTypeSignature();
            var expected = new SzArrayTypeSignature(elementType);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[]");
            Assert.Equal(expected, actual, _comparer);
        }

        [Fact]
        public void MultidimensionalArray()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";
            
            var elementType = new TypeReference(_module, ns, name).ToTypeSignature();
            var expected = new ArrayTypeSignature(elementType, 4);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[,,,]");
            Assert.Equal(expected, actual, _comparer);
        }
    }
}