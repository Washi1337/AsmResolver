using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.Signatures.Types.Parsing;
using Xunit;

namespace AsmResolver.DotNet.Tests.Signatures
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

        [Theory]
        [InlineData("MyType")]
        [InlineData("#=abc")]
        public void SimpleTypeNoNamespace(string name)
        {
            var expected = new TypeReference(_module, null, name).ToTypeSignature();
            var actual = TypeNameParser.Parse(_module, expected.Name);
            Assert.Equal(expected, actual, _comparer);
        }

        [Theory]
        [InlineData("MyNamespace")]
        [InlineData("MyNamespace.SubNamespace")]
        [InlineData("MyNamespace.SubNamespace.SubSubNamespace")]
        [InlineData("#=abc.#=def")]
        public void SimpleTypeWithNamespace(string ns)
        {
            var expected = new TypeReference(_module, ns, "MyType").ToTypeSignature();
            var actual = TypeNameParser.Parse(_module, expected.FullName);
            Assert.Equal(expected, actual, _comparer);
        }

        [Theory]
        [InlineData("MyNamespace", "MyType", "MyNestedType")]
        [InlineData("MyNamespace", "#=abc", "#=def")]
        public void NestedType(string ns, string name, string nestedType)
        {
            var expected = new TypeReference(
                    new TypeReference(_module, ns, name),
                    null,
                    nestedType)
                .ToTypeSignature();

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}+{nestedType}");
            Assert.Equal(expected, actual, _comparer);
        }

        [Theory]
        [InlineData("MyType", "MyNestedType")]
        [InlineData("#=abc", "#=def")]
        public void NestedTypeNoNamespace(string name, string nestedType)
        {
            var expected = new TypeReference(
                    new TypeReference(_module, null, name),
                    null,
                    nestedType)
                .ToTypeSignature();

            var actual = TypeNameParser.Parse(_module, $"{name}+{nestedType}");
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

        [Fact]
        public void GenericTypeSingleBrackets()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            var elementType = new TypeReference(_module, ns, name);
            var argumentType = _module.CorLibTypeFactory.Object;

            var expected = new GenericInstanceTypeSignature(elementType, false, argumentType);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[{argumentType.Namespace}.{argumentType.Name}]");
            Assert.Equal(expected, actual, _comparer);
        }

        [Theory]
        [InlineData("System", "Object")]
        [InlineData("System", "#=abc")]
        [InlineData("#=abc", "#=def")]
        public void GenericTypeMultiBrackets(string argNs, string argName)
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            var elementType = new TypeReference(_module, ns, name);
            var argumentType = new TypeReference(_module, _module, argNs, argName)
                .ToTypeSignature();

            var expected = new GenericInstanceTypeSignature(elementType, false, argumentType);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[[{argumentType.Namespace}.{argumentType.Name}]]");
            Assert.Equal(expected, actual, _comparer);
        }

        [Fact]
        public void SpacesInAssemblySpec()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";
            const string assemblyRef = "Some Assembly";

            var scope = new AssemblyReference(assemblyRef, new Version(1, 0, 0, 0));
            var expected = new TypeReference(_module, scope, ns, name).ToTypeSignature();

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}, {assemblyRef}, Version={scope.Version}");
            Assert.Equal(expected, actual, _comparer);
        }

        [Fact]
        public void ReadEscapedTypeName()
        {
            const string ns = "MyNamespace";
            const string escapedName = "MyType\\+WithPlus";
            const string name = "MyType+WithPlus";

            var expected = new TypeReference(_module, ns, name).ToTypeSignature();

            var actual = TypeNameParser.Parse(_module, $"{ns}.{escapedName}");
            Assert.Equal(expected, actual, _comparer);
        }
    }
}
