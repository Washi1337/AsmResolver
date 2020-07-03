using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Cloning;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.NestedClasses;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.Win32Resources;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class ModuleDefinitionTest
    {
        private static ModuleDefinition Rebuild(ModuleDefinition module)
        {
            using var stream = new MemoryStream();
            module.Write(stream);
            return ModuleDefinition.FromReader(new ByteArrayReader(stream.ToArray()));
        }
        
        [Fact]
        public void ReadNameTest()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal("HelloWorld.exe", module.Name);
        }

        [Fact]
        public void NameIsPersistentAfterRebuild()
        {
            const string newName = "HelloMars.exe";
            
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            module.Name = newName;

            var newModule = Rebuild(module);
            Assert.Equal(newName, newModule.Name);
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
        public void LookupModuleReference()
        {
            var module = ModuleDefinition.FromFile(Path.Combine("Resources", "Manifest.exe"));
            var member = module.LookupMember(new MetadataToken(TableIndex.ModuleRef, 1));
            Assert.IsAssignableFrom<ModuleReference>(member);

            var moduleRef = (ModuleReference) member;
            Assert.Equal("MyModel.netmodule", moduleRef.Name);
            Assert.Same(module.ModuleReferences[0], moduleRef);
        }

        [Fact]
        public void EmptyModuleShouldAlwaysContainCorLibReference()
        {
            // Issue #39 (https://github.com/Washi1337/AsmResolver/issues/39)
            
            var module = new ModuleDefinition("TestModule");
            var corLib = module.CorLibTypeFactory.CorLibScope;
            
            using var stream = new MemoryStream();
            module.Write(stream);
            
            var newModule = ModuleDefinition.FromBytes(stream.ToArray());
            var comparer = new SignatureComparer();
            Assert.Contains(newModule.AssemblyReferences, reference => comparer.Equals(corLib, reference));
        }

        [Fact]
        public void CreateNewCorLibFactory()
        {
            var module = new ModuleDefinition("MySampleModule");
            Assert.NotNull(module.CorLibTypeFactory);
            Assert.NotNull(module.CorLibTypeFactory.CorLibScope);
            Assert.NotNull(module.CorLibTypeFactory.Void);
        }
        
        [Fact]
        public void AddingTypeIsPersistentAfterRebuild()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            
            var newType = new TypeDefinition("SomeNamespace", "SomeType", 
                TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);
            module.TopLevelTypes.Add(newType);

            var newModule = Rebuild(module);
            var comparer = new SignatureComparer();
            Assert.Contains(newModule.TopLevelTypes, t => comparer.Equals(newType, t));
        }

        [Fact]
        public void NewFW40ModuleShouldAlwaysContainModuleType()
        {
            var module = new ModuleDefinition("TestModule");
            Assert.NotNull(module.GetModuleType());
        }

        [Fact]
        public void NewModuleShouldAlwaysContainModuleType()
        {
            var module = new ModuleDefinition("TestModule", KnownCorLibs.NetStandard_v2_1_0_0);
            Assert.NotNull(module.GetModuleType());
        }

        [Fact]
        public void GetOrCreateModuleConstructorShouldAddNewConstructorIfNotPresent()
        {
            var module = new ModuleDefinition("TestModule", KnownCorLibs.NetStandard_v2_1_0_0);
            var cctor = module.GetOrCreateModuleConstructor();
            Assert.Contains(cctor, module.GetModuleType().Methods);
        }

        [Fact]
        public void GetOrCreateModuleConstructorShouldGetExistingConstructorIfPresent()
        {
            var module = new ModuleDefinition("TestModule", KnownCorLibs.NetStandard_v2_1_0_0);
            var cctor = module.GetOrCreateModuleConstructor();
            var cctor2 = module.GetOrCreateModuleConstructor();
            Assert.Same(cctor, cctor2);
        }

        [Fact]
        public void PersistentResources()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            
            // Add new directory.
            const string directoryName = "Test";
            var entryData = new byte[] {0, 1, 2, 3, 4};
            var directory = new ResourceDirectory(directoryName)
            {
                Entries =
                {
                    new ResourceDirectory(1)
                    {
                        Entries = {new ResourceData(1234, new DataSegment(entryData))}
                    }
                }
            };
            module.NativeResourceDirectory.Entries.Add(directory);

            // Write and rebuild.
            using var stream = new MemoryStream();
            module.Write(stream);
            var newModule = ModuleDefinition.FromReader(new ByteArrayReader(stream.ToArray()));

            // Assert contents.
            var newDirectory = (IResourceDirectory) newModule.NativeResourceDirectory.Entries
                .First(entry => entry.Name == directoryName);
            newDirectory = (IResourceDirectory) newDirectory.Entries[0];
            
            var newData = (IResourceData) newDirectory.Entries[0];
            var newContents = (IReadableSegment) newData.Contents;
            Assert.Equal(entryData, newContents.ToArray());
        }
    }
}