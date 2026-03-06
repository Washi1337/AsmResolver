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

        public TypeNameParserTest()
        {
            var context = new RuntimeContext(DotNetRuntimeInfo.NetCoreApp(10, 0));
            var assembly = new AssemblyDefinition("DummyModule", new Version());
            context.AddAssembly(assembly);
            _module = new ModuleDefinition("DummyModule", context.TargetRuntime)
            {
                TopLevelTypes = { new TypeDefinition("System", "Action", TypeAttributes.Class) }
            };
            assembly.Modules.Add(_module);
        }

        private TypeDefinition CreateAndAddTypeDef(string? ns, string name)
        {
            var type = new TypeDefinition(ns, name, TypeAttributes.Class, _module.CorLibTypeFactory.Object.Type);
            _module.TopLevelTypes.Add(type);
            return type;
        }

        private TypeDefinition CreateAndAddTypeDef(TypeDefinition declaringType, string name)
        {
            var type = new TypeDefinition(null, name, TypeAttributes.Class | TypeAttributes.NestedPublic, _module.CorLibTypeFactory.Object.Type);
            declaringType.NestedTypes.Add(type);
            return type;
        }

        [Theory]
        [InlineData("MyType")]
        [InlineData("#=abc")]
        [InlineData("\u0002\u2007\u2007")]
        public void SimpleTypeNoNamespace(string name)
        {
            var expected = CreateAndAddTypeDef(default(string), name).ToTypeSignature(false);
            var actual = TypeNameParser.Parse(_module, expected.Name!);
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Theory]
        [InlineData("MyNamespace")]
        [InlineData("MyNamespace.SubNamespace")]
        [InlineData("MyNamespace.SubNamespace.SubSubNamespace")]
        [InlineData("#=abc.#=def")]
        [InlineData("\u0002\u2007\u2007.\u0002\u2007\u2007")]
        public void SimpleTypeWithNamespace(string ns)
        {
            var expected = CreateAndAddTypeDef(ns, "MyType").ToTypeSignature(false);
            var actual = TypeNameParser.Parse(_module, expected.FullName);
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Theory]
        [InlineData("MyNamespace", "MyType", "MyNestedType")]
        [InlineData("MyNamespace", "#=abc", "#=def")]
        [InlineData("\u0002\u2007\u2007", "\u0002\u2007\u2007", "\u0002\u2007\u2007")]
        public void NestedType(string ns, string name, string nestedType)
        {
            var expected = CreateAndAddTypeDef(
                    CreateAndAddTypeDef(ns, name),
                    nestedType)
                .ToTypeSignature(false);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}+{nestedType}, {_module}");
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Theory]
        [InlineData("MyType", "MyNestedType")]
        [InlineData("#=abc", "#=def")]
        [InlineData("\u0002\u2007\u2007", "\u0002\u2007\u2007")]
        public void NestedTypeNoNamespace(string name, string nestedType)
        {
            var expected = CreateAndAddTypeDef(
                    CreateAndAddTypeDef(default(string), name),
                    nestedType)
                .ToTypeSignature(false);

            var actual = TypeNameParser.Parse(_module, $"{name}+{nestedType}, {_module}");
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Fact]
        public void TypeWithAssemblyName()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";
            var assemblyRef = new AssemblyReference("MyAssembly", new Version(1, 2, 3, 4));
            var expected = new TypeReference(assemblyRef, ns, name).ToTypeSignature(false);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}, {assemblyRef.FullName}");
            Assert.Equal(expected, actual, SignatureComparer.Default);
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
                [1, 2, 3, 4, 5, 6, 7, 8]);

            var expected = new TypeReference(assemblyRef, ns, name).ToTypeSignature(false);

            var actual = TypeNameParser.Parse(_module,
                $"{ns}.{name}, {assemblyRef.FullName}");
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Fact]
        public void ValueType()
        {
            const string ns = "System";
            const string name = "Nullable`1";

            var corlib = new AssemblyReference(_module.CorLibTypeFactory.CorLibScope.GetAssembly()!);
            var expected = new TypeReference(_module, corlib, ns, name).ToTypeSignature(true);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}, {corlib.FullName}");
            Assert.Equal(expected, actual, SignatureComparer.Default);
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
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Fact]
        public void SimpleArrayType()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            var elementType = CreateAndAddTypeDef(ns, name).ToTypeSignature(false);
            var expected = new SzArrayTypeSignature(elementType);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[], {elementType.Scope}");
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Fact]
        public void MultidimensionalArray()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            var elementType = CreateAndAddTypeDef(ns, name).ToTypeSignature(false);
            var expected = new ArrayTypeSignature(elementType, 4);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[,,,], {elementType.Scope}");
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Fact]
        public void GenericTypeSingleBrackets()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            ITypeDefOrRef elementType = CreateAndAddTypeDef(ns, name);
            var argumentType = _module.CorLibTypeFactory.Object;

            var expected = elementType.MakeGenericInstanceType(false, [argumentType]);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[{argumentType.Namespace}.{argumentType.Name}], {elementType.Scope}");
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Fact]
        public void GenericTypeSingleBracketsMultiElements()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            ITypeDefOrRef elementType = CreateAndAddTypeDef(ns, name);
            var argumentType = _module.CorLibTypeFactory.Object;
            var argumentType2 = _module.CorLibTypeFactory.Int32;

            var expected = elementType.MakeGenericInstanceType(false, [argumentType, argumentType2]);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[{argumentType.Namespace}.{argumentType.Name},{argumentType2.Namespace}.{argumentType2.Name}], {elementType.Scope}");
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Fact]
        public void GenericTypeSingleBracketsMultiElementsWithPlus()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            const string escapedPName = "MyType\\+WithPlus";
            const string pname = "MyType+WithPlus";


            ITypeDefOrRef elementType = CreateAndAddTypeDef(ns, name);
            var argumentType = _module.CorLibTypeFactory.Object;
            // needs to be null scope for the comparison to work
            var argumentType2 = CreateAndAddTypeDef(ns, pname).ToTypeSignature(false);

            var expected = elementType.MakeGenericInstanceType(false, [argumentType, argumentType2]);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[{argumentType.Namespace}.{argumentType.Name},{ns}.{escapedPName}], {elementType.Scope}");
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Theory]
        [InlineData("System", "Object")]
        [InlineData("System", "#=abc")]
        [InlineData("#=abc", "#=def")]
        public void GenericTypeMultiBrackets(string argNs, string argName)
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            ITypeDefOrRef elementType = CreateAndAddTypeDef(ns, name);
            var argumentType = CreateAndAddTypeDef(argNs, argName).ToTypeSignature(false);

            var expected = elementType.MakeGenericInstanceType(false, [argumentType]);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[[{argumentType.Namespace}.{argumentType.Name}, {argumentType.Scope}]], {elementType.Scope}");
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Fact]
        public void GenericTypeMultiBracketsMultiElementsVersion()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";

            ITypeDefOrRef elementType = CreateAndAddTypeDef(ns, name);
            var argumentType = _module.CorLibTypeFactory.Object;
            var argumentType2 = _module.CorLibTypeFactory.Int32;
            string fullName = _module.CorLibTypeFactory.CorLibScope.GetAssembly()!.FullName;

            var expected = elementType.MakeGenericInstanceType(false, [argumentType, argumentType2]);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[[{argumentType.Namespace}.{argumentType.Name}, {fullName}],[{argumentType2.Namespace}.{argumentType2.Name}, {fullName}]], {elementType.Scope}");
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Fact]
        public void GenericValueType()
        {
            const string ns = "System";
            const string name = "Nullable`1";

            var corlib = new AssemblyReference(_module.CorLibTypeFactory.CorLibScope.GetAssembly()!);
            var elementType = new TypeReference(_module, corlib, ns, name);
            var argumentType = _module.CorLibTypeFactory.Int32;

            var expected = elementType.MakeGenericInstanceType(true, [argumentType]);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}[[{argumentType.Namespace}.{argumentType.Name}, {corlib.FullName}]], {corlib.FullName}");
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Fact]
        public void SpacesInAssemblySpec()
        {
            const string ns = "MyNamespace";
            const string name = "MyType";
            const string assemblyRef = "Some Assembly";

            var scope = new AssemblyReference(assemblyRef, new Version(1, 0, 0, 0));
            var expected = new TypeReference(_module, scope, ns, name).ToTypeSignature(false);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}, {assemblyRef}, Version={scope.Version}");
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Fact]
        public void ReadEscapedTypeName()
        {
            const string ns = "MyNamespace";
            const string escapedName = "MyType\\+WithPlus";
            const string name = "MyType+WithPlus";

            var expected = CreateAndAddTypeDef(ns, name).ToTypeSignature(false);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{escapedName}, {_module}");
            Assert.Equal(expected, actual, SignatureComparer.Default);
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
            Assert.Equal(expected, actual, SignatureComparer.Default);
        }

        [Fact]
        public void ReadTypeInCorLibAssemblyWithoutScope()
        {
            const string ns = "System";
            const string name = "Array";

            var expected = _module.RuntimeContext!.RuntimeCorLib!.ToAssemblyReference()
                .CreateTypeReference(ns, name)
                .ToTypeSignature(false);

            var actual = TypeNameParser.Parse(_module, $"{ns}.{name}");

            Assert.Equal(expected, actual, SignatureComparer.Default);
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
                "System.Array, System.Runtime, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
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

            Assert.True(type.TryResolve(module.RuntimeContext, out _));
            Assert.True(type.ImportWith(module.DefaultImporter).TryResolve(module.RuntimeContext, out _));
        }

        [Fact]
        public void ReadTypeWithoutFullyQualifiedNameShouldParseToRuntimeCorLib()
        {
            var type = TypeNameParser.Parse(_module, "System.Array");

            var runtimeCorLib = _module.RuntimeContext!.RuntimeCorLib;
            Assert.NotEqual(runtimeCorLib, _module.CorLibTypeFactory.CorLibScope.GetAssembly(), SignatureComparer.Default!);
            Assert.Equal(runtimeCorLib, type.Scope?.GetAssembly(), SignatureComparer.Default!);
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

            Assert.Equal(_module, type.Scope, SignatureComparer.Default!);
        }

        [Fact]
        public void GenericTypeNameWithInnerFQNRoundtrips()
        {
            // https://github.com/Washi1337/AsmResolver/pull/647
            // to hit the original bug of this issue, the assembly's PublicKeyToken needs to begin with a digit, which this corelib does
            var context = new RuntimeContext(DotNetRuntimeInfo.NetCoreApp(10, 0));
            var assembly = new AssemblyDefinition("Dummy", new Version(1, 0, 0, 0));
            context.AddAssembly(assembly);

            var module = new ModuleDefinition("Dummy", context.TargetRuntime);
            assembly.Modules.Add(module);

            var expectedType = module.CorLibTypeFactory.CorLibScope
                .CreateTypeReference("System", "Action`1")
                .MakeGenericInstanceType(isValueType: false, [module.CorLibTypeFactory.Int32])
                .ImportWith(module.DefaultImporter);
            string typeName = TypeNameBuilder.GetAssemblyQualifiedName(expectedType);
            var actualType = TypeNameParser.Parse(module, typeName);

            Assert.Equal(expectedType, actualType, SignatureComparer.Default);
        }
    }
}
