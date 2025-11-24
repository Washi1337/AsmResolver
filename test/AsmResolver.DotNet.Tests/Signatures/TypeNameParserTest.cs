using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Parsing;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.Signatures
{
    public class TypeNameParserTest
    {
        private readonly ModuleDefinition _module;
        private readonly SignatureComparer _comparer;

        public TypeNameParserTest()
        {
            _module = new ModuleDefinition("DummyModule", KnownCorLibs.SystemRuntime_v4_2_2_0)
            {
                TopLevelTypes = { new TypeDefinition("System", "Action", TypeAttributes.Class) }
            };
            new AssemblyDefinition("DummyModule", new Version()).Modules.Add(_module);
            _comparer = new SignatureComparer();
        }

        [Theory]
        [InlineData("MyType")]
        [InlineData("#=abc")]
        [InlineData("\u0002\u2007\u2007")]
        public void SimpleTypeNoNamespace(string name)
        {
            var expected = new TypeReference(null, null, name).ToTypeSignature();
            var actual = TypeNameParser.Parse(_module, expected.Name!);
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
            var expected = new TypeReference(null, ns, "MyType").ToTypeSignature();
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

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}+{nestedType}, {_module}");
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

            var actual = TypeNameParser.Parse(_module, $"{name}+{nestedType}, {_module}");
            Assert.Equal(expected, actual, _comparer);
        }

        [Fact]
        public void TypeWithAssemblyName()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";
            var assemblyRef = new AssemblyReference("MyAssembly", new Version(1, 2, 3, 4));
            var expected = new TypeReference(assemblyRef, ns, name).ToTypeSignature();

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}, {assemblyRef.FullName}");
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
        public void ValueType()
        {
            const string ns = "System";
            const string name = "Nullable`1";

            var corlib = new AssemblyReference(_module.CorLibTypeFactory.CorLibScope.GetAssembly()!);
            var expected = new TypeReference(_module, corlib, ns, name).ToTypeSignature(true);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}, {corlib.FullName}");
            Assert.Equal(expected, actual, _comparer);
        }

        [Fact]
        public void NestedValueType()
        {
            const string ns = "System.Collections.Generic";
            const string name = "List`1";
            const string nestedTypeName = "Enumerator";

            var corlib = KnownCorLibs.NetStandard_v2_0_0_0;
            var expected = corlib.CreateTypeReference(ns, name)
                .CreateTypeReference(nestedTypeName)
                .ToTypeSignature(true)
                .ImportWith(_module.DefaultImporter);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}+{nestedTypeName}, {corlib.FullName}");
            Assert.Equal(expected, actual, _comparer);
        }

        [Fact]
        public void SimpleArrayType()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            var elementType = new TypeReference(_module, ns, name).ToTypeSignature();
            var expected = new SzArrayTypeSignature(elementType);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[], {elementType.Scope}");
            Assert.Equal(expected, actual, _comparer);
        }

        [Fact]
        public void MultidimensionalArray()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            var elementType = new TypeReference(_module, ns, name).ToTypeSignature();
            var expected = new ArrayTypeSignature(elementType, 4);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[,,,], {elementType.Scope}");
            Assert.Equal(expected, actual, _comparer);
        }

        [Fact]
        public void GenericTypeSingleBrackets()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            var elementType = new TypeReference(_module, ns, name);
            var argumentType = _module.CorLibTypeFactory.Object;

            var expected = elementType.MakeGenericInstanceType(false, argumentType);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[{argumentType.Namespace}.{argumentType.Name}], {elementType.Scope}");
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

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[{argumentType.Namespace}.{argumentType.Name},{argumentType2.Namespace}.{argumentType2.Name}], {elementType.Scope}");
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
            // needs to be null scope for the comparison to work
            var argumentType2 = new TypeReference(null, ns, pname).ToTypeSignature(); ;

            var expected = new GenericInstanceTypeSignature(elementType, false, argumentType, argumentType2);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[{argumentType.Namespace}.{argumentType.Name},{ns}.{escapedPName}], {elementType.Scope}");
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

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[[{argumentType.Namespace}.{argumentType.Name}, {argumentType.Scope}]], {elementType.Scope}");
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

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[[{argumentType.Namespace}.{argumentType.Name}, {fullName}],[{argumentType2.Namespace}.{argumentType2.Name}, {fullName}]], {elementType.Scope}");
            Assert.Equal(expected, actual, _comparer);
        }

        [Fact]
        public void GenericValueType()
        {
            const string ns = "System";
            const string name = "Nullable`1";

            var corlib = new AssemblyReference(_module.CorLibTypeFactory.CorLibScope.GetAssembly()!);
            var elementType = new TypeReference(_module, corlib, ns, name);
            var argumentType = _module.CorLibTypeFactory.Int32;

            var expected = elementType.MakeGenericInstanceType(true, argumentType);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[[{argumentType.Namespace}.{argumentType.Name}, {corlib.FullName}]], {corlib.FullName}");
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

            var actual = TypeNameParser.Parse(_module, $"{ns}.{escapedName}, {_module}");
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

            var expected = new TypeReference(new AssemblyReference(_module.RuntimeContext.RuntimeCorLib!), ns, name).ToTypeSignature();
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

        [Fact]
        public void ReadTypeNameFromLocalModuleShouldResultInResolvableType()
        {
            var module = ModuleDefinition.FromFile(typeof(TypeNameParserTest).Assembly.Location, TestReaderParameters);
            var type = TypeNameParser
                    .Parse(module, typeof(TypeNameParserTest).AssemblyQualifiedName!)
                    .GetUnderlyingTypeDefOrRef()!;

            Assert.NotNull(type.Resolve());
            Assert.NotNull(type.ImportWith(module.DefaultImporter).Resolve());
        }

        [Fact]
        public void ReadTypeWithoutFullyQualifiedNameShouldParseToRuntimeCorLib()
        {
            var type = TypeNameParser.Parse(_module, "System.Array");

            Assert.NotEqual(_module.RuntimeContext.RuntimeCorLib, _module.CorLibTypeFactory.CorLibScope.GetAssembly(), _comparer);
            Assert.Equal(_module.RuntimeContext.RuntimeCorLib, type.Scope.GetAssembly(), _comparer);
        }

        [Fact]
        public void ReadTypeWithoutFullyQualifiedNameNotInCorLibShouldHaveNullScope()
        {
            var type = TypeNameParser.Parse(_module, "System.Uri");

            Assert.Null(type.Scope);
        }

        [Fact]
        public void ReadTypeWithoutFullyQualifiedNameResolvesToContextModuleFirst()
        {
            var type = TypeNameParser.Parse(_module, "System.Action");

            Assert.Equal(_module, type.Scope, _comparer);
        }

        [Fact]
        public void GenericTypeNameWithInnerFQNRoundtrips()
        {
            // https://github.com/Washi1337/AsmResolver/pull/647
            // to hit the original bug of this issue, the assembly's PublicKeyToken needs to begin with a digit, which this corelib does
            var module = new ModuleDefinition("DummyModule", KnownCorLibs.SystemPrivateCoreLib_v8_0_0_0);
            var expectedType = module.CorLibTypeFactory.CorLibScope
                .CreateTypeReference("System", "Action`1")
                .MakeGenericInstanceType(isValueType: false, module.CorLibTypeFactory.Int32)
                .ImportWith(module.DefaultImporter);
            string typeName = TypeNameBuilder.GetAssemblyQualifiedName(expectedType);
            var actualType = TypeNameParser.Parse(module, typeName);

            Assert.Equal(expectedType, actualType, SignatureComparer.Default);
        }
    }
}
