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
        public void ReadMaliciousNestedClassLoop()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_MaliciousNestedClassLoop);
            Assert.Equal(new[] {"<Module>", "Program"}, module.TopLevelTypes.Select(t => t.Name));
        }

        [Fact]
        public void ReadMaliciousNestedClassLoop2()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_MaliciousNestedClassLoop2);
            Assert.Equal(
                new HashSet<string> {"<Module>", "Program", "MaliciousEnclosingClass"},
                new HashSet<string>(module.TopLevelTypes.Select(t => t.Name)));

            var enclosingClass = module.TopLevelTypes.First(x => x.Name == "MaliciousEnclosingClass");
            Assert.Single(enclosingClass.NestedTypes);
            Assert.Single(enclosingClass.NestedTypes[0].NestedTypes);
            Assert.Empty(enclosingClass.NestedTypes[0].NestedTypes[0].NestedTypes);
        }
        
        [Fact]
        public void HelloWorldReadAssemblyReferences()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Single(module.AssemblyReferences);
            Assert.Equal("mscorlib", module.AssemblyReferences[0].Name);
        }

        [Fact]
        public void LookupTypeReference()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var member = module.LookupMember(new MetadataToken(TableIndex.TypeRef, 12));
            Assert.IsAssignableFrom<TypeReference>(member);

            var typeRef = (TypeReference) member;
            Assert.Equal("System", typeRef.Namespace);
            Assert.Equal("Object", typeRef.Name);
        }

        [Fact]
        public void LookupTypeDefinition()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var member = module.LookupMember(new MetadataToken(TableIndex.TypeDef, 2));
            Assert.IsAssignableFrom<TypeDefinition>(member);
            
            var typeDef = (TypeDefinition) member;
            Assert.Equal("HelloWorld", typeDef.Namespace);
            Assert.Equal("Program", typeDef.Name);
        }

        [Fact]
        public void LookupAssemblyReference()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var member = module.LookupMember(new MetadataToken(TableIndex.AssemblyRef, 1));
            Assert.IsAssignableFrom<AssemblyReference>(member);
            
            var assemblyRef = (AssemblyReference) member;
            Assert.Equal("mscorlib", assemblyRef.Name);
            Assert.Same(module.AssemblyReferences[0], assemblyRef);
        }

        [Fact]
        public void CreateNewCorLibFactory()
        {
            var module = new ModuleDefinition("MySampleModule");
            Assert.NotNull(module.CorLibTypeFactory);
            Assert.NotNull(module.CorLibTypeFactory.CorLibScope);
            Assert.NotNull(module.CorLibTypeFactory.Void);
        }
        
    }
}