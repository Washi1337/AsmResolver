using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.Signatures.Types.Parsing;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Signatures
{
    public class TypeNameParserTest
    {
        private readonly ModuleDefinition _module;
        private readonly SignatureComparer _comparer;

        public TypeNameParserTest()
        {
            _module = new ModuleDefinition("DummyModule", KnownCorLibs.SystemRuntime_v4_2_2_0);
            _comparer = new SignatureComparer();
        }

        [Theory]
        [InlineData("MyType")]
        [InlineData("#=abc")]
        [InlineData("\u0002\u2007\u2007")]
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
        [InlineData("\u0002\u2007\u2007.\u0002\u2007\u2007")]
        public void SimpleTypeWithNamespace(string ns)
        {
            var expected = new TypeReference(_module, ns, "MyType").ToTypeSignature();
            var actual = TypeNameParser.Parse(_module, expected.FullName);
            Assert.Equal(expected, actual, _comparer);
        }

        [Theory]
        [InlineData("MyNamespace", "MyType", "MyNestedType")]
        [InlineData("MyNamespace", "#=abc", "#=def")]
        [InlineData("\u0002\u2007\u2007", "\u0002\u2007\u2007", "\u0002\u2007\u2007")]
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
        [InlineData("\u0002\u2007\u2007", "\u0002\u2007\u2007")]
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

        [Fact]
        public void GenericTypeSingleBracketsMultiElements()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            var elementType = new TypeReference(_module, ns, name);
            var argumentType = _module.CorLibTypeFactory.Object;
            var argumentType2 = _module.CorLibTypeFactory.Int32;

            var expected = new GenericInstanceTypeSignature(elementType, false, argumentType, argumentType2);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[{argumentType.Namespace}.{argumentType.Name},{argumentType2.Namespace}.{argumentType2.Name}]");
            Assert.Equal(expected, actual, _comparer);
        }


        [Fact]
        public void GenericTypeSingleBracketsMultiElementsWithPlus()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            const string escapedPName = "MyType\\+WithPlus";
            const string pname = "MyType+WithPlus";


            var elementType = new TypeReference(_module, ns, name);
            var argumentType = _module.CorLibTypeFactory.Object;
            var argumentType2 = new TypeReference(_module, ns, pname).ToTypeSignature(); ;

            var expected = new GenericInstanceTypeSignature(elementType, false, argumentType, argumentType2);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[{argumentType.Namespace}.{argumentType.Name},{ns}.{escapedPName}]");
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
        public void GenericTypeMultiBracketsMultiElementsVersion()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            var elementType = new TypeReference(_module, ns, name);
            var argumentType = _module.CorLibTypeFactory.Object;
            var argumentType2 = _module.CorLibTypeFactory.Int32;
            var fullName = _module.CorLibTypeFactory.CorLibScope.GetAssembly().FullName;

            var expected = new GenericInstanceTypeSignature(elementType, false, argumentType, argumentType2);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[[{argumentType.Namespace}.{argumentType.Name}, {fullName}],[{argumentType2.Namespace}.{argumentType2.Name}, {fullName}]]");
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

        [Fact]
        public void ReadTypeInSameAssemblyWithoutScope()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            var definition = new TypeDefinition(ns, name, TypeAttributes.Public, _module.CorLibTypeFactory.Object.Type);
            _module.TopLevelTypes.Add(definition);

            var expected = definition.ToTypeSignature();
            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}");
            Assert.Equal(expected, actual, _comparer);
        }

        [Fact]
        public void ReadTypeInCorLibAssemblyWithoutScope()
        {
            const string ns = "System";
            const string name = "Array";

            var expected = new TypeReference(_module.CorLibTypeFactory.CorLibScope, ns, name).ToTypeSignature();
            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}");
            Assert.Equal(expected, actual, _comparer);
        }

        [Fact]
        public void ReadCorLibTypeShouldNotUpdateScopesOfUnderlyingTypes()
        {
            // https://github.com/Washi1337/AsmResolver/issues/263

            var scope = _module.CorLibTypeFactory.Object.Type.Scope;
            TypeNameParser.Parse(_module,
                "System.Object, System.Runtime, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            Assert.Same(scope, _module.CorLibTypeFactory.Object.Type.Scope);
        }

        [Fact]
        public void ReadTypeShouldReuseScopeInstanceWhenAvailable()
        {
            var type = TypeNameParser.Parse(_module,
                "System.Array, System.Runtime, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            Assert.Contains(type.Scope!.GetAssembly(), _module.AssemblyReferences);
        }

        [Fact]
        public void ReadTypeShouldUseNewScopeInstanceIfNotImportedYet()
        {
            var type = TypeNameParser.Parse(_module,
                "SomeNamespace.SomeType, SomeAssembly, Version=1.2.3.4, Culture=neutral, PublicKeyToken=0123456789abcdef");
            Assert.DoesNotContain(type.Scope!.GetAssembly(), _module.AssemblyReferences);
        }
    }
}
