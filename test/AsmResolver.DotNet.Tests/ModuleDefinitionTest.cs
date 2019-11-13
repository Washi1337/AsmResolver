using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.TestCases.NestedClasses;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class ModuleDefinitionTest
    {
        [Fact]
        public void ReadNameTest()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal("HelloWorld.exe", module.Name);
        }

        [Fact]
        public void ReadManifestModule()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.NotNull(module.Assembly);
            Assert.Same(module, module.Assembly.ManifestModule);
        }

        [Fact]
        public void ReadTypesNoNested()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal(new[] {"<Module>", "Program"}, module.TopLevelTypes.Select(t => t.Name));
        }

        [Fact]
        public void ReadTypesNested()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location);
            Assert.Equal(new HashSet<string>
            {
                "<Module>",
                nameof(TopLevelClass1),
                nameof(TopLevelClass2)
            }, module.TopLevelTypes.Select(t => t.Name));
        }

        [Fact]
        public void ResolveTypeReference()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var member = module.LookupMember(new MetadataToken(TableIndex.TypeRef, 12));
            Assert.IsAssignableFrom<TypeReference>(member);

            var typeRef = (TypeReference) member;
            Assert.Equal("System", typeRef.Namespace);
            Assert.Equal("Object", typeRef.Name);
        }
    }
}