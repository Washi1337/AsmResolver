using System;
using AsmResolver.DotNet.Signatures.Parsing;
using AsmResolver.PE.DotNet.Metadata.Tables;
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
            string name = TypeNameBuilder.GetAssemblyQualifiedName(type.ToTypeSignature(false));
            Assert.Contains("Class\\.Name", name);
        }

        [Fact]
        public void NameWithEqualsShouldNotBeEscaped()
        {
            var type = new TypeReference(_module, _module, "Company.ProductName", "#=abc");
            string name = TypeNameBuilder.GetAssemblyQualifiedName(type.ToTypeSignature(false));
            Assert.DoesNotContain('\\', name);
            Assert.Contains("#=abc", name);
        }

        [Fact]
        public void NamespaceShouldNotBeEscaped()
        {
            var type = new TypeReference(_module, _module, "Company.ProductName", "ClassName");
            string name = TypeNameBuilder.GetAssemblyQualifiedName(type.ToTypeSignature(false));
            Assert.DoesNotContain('\\', name);
            Assert.Contains("Company.ProductName", name);
        }

        [Fact]
        public void NamespaceWithEqualsShouldNotBeEscaped()
        {
            var type = new TypeReference(_module, _module, "#=abc", "ClassName");
            string name = TypeNameBuilder.GetAssemblyQualifiedName(type.ToTypeSignature(false));
            Assert.DoesNotContain('\\', name);
            Assert.Contains("#=abc", name);
        }

        [Theory]
        [InlineData("MyNamespace", "MyType", "MyNestedType")]
        [InlineData("MyNamespace", "#=abc", "#=def")]
        public void NestedTypeShouldContainPlus(string ns, string name, string nestedType)
        {
            var type = new TypeReference(_module, new TypeReference(_module, ns, name), null, nestedType);
            string fullname = TypeNameBuilder.GetAssemblyQualifiedName(type.ToTypeSignature(false));
            Assert.DoesNotContain('\\', fullname);
            Assert.Contains($"{ns}.{name}+{nestedType}", fullname);
        }

        [Theory]
        [InlineData("MyType", "MyNestedType")]
        [InlineData("#=abc", "#=def")]
        public void NestedTypeNoNamespaceShouldContainPlus(string name, string nestedType)
        {
            var type = new TypeReference(_module, new TypeReference(_module, null, name), null, nestedType);
            string fullname = TypeNameBuilder.GetAssemblyQualifiedName(type.ToTypeSignature(false));
            Assert.DoesNotContain('\\', fullname);
            Assert.Contains($"{name}+{nestedType}", fullname);
        }

        [Fact]
        public void CorLibWithFullScope()
        {
            var type = _module.CorLibTypeFactory.Object;
            string name = TypeNameBuilder.GetAssemblyQualifiedName(type);
            Assert.Contains(type.FullName, name);
            Assert.Contains(type.Scope!.GetAssembly()!.FullName, name);
        }

        [Fact]
        public void CorLibWithFullScopeExactly()
        {
            // Order matters for Mono
            var type = _module.CorLibTypeFactory.Object;
            string name = TypeNameBuilder.GetAssemblyQualifiedName(type);
            Assert.Equal("System.Object, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", name);
        }

        [Fact]
        public void ExternalTypeShouldIncludeAssemblySpec()
        {
            // Prepare dependency module with a type.
            var dependencyAssembly = new AssemblyDefinition("Foo", new Version(1, 0, 0, 0));
            var dependencyModule = new ModuleDefinition("Foo.dll");
            dependencyAssembly.Modules.Add(dependencyModule);
            dependencyModule.TopLevelTypes.Add(new TypeDefinition("SomeNamespace", "SomeType", TypeAttributes.Public, dependencyModule.CorLibTypeFactory.Object.Type));

            // Create a reference based on an external ModuleDefinition
            var reference = dependencyModule.CreateTypeReference("SomeNamespace", "SomeType");
            string name = TypeNameBuilder.GetAssemblyQualifiedName(reference.ToTypeSignature(false), _module);
            Assert.Equal("SomeNamespace.SomeType, Foo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", name);
        }
    }
}
