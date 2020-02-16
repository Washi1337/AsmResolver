using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class ReferenceImporterTest
    {
        private static readonly SignatureComparer _comparer = new SignatureComparer();

        private readonly AssemblyReference _dummyAssembly = new AssemblyReference("SomeAssembly", new Version(1, 2, 3, 4));
        private readonly ModuleDefinition _module;
        private readonly ReferenceImporter _importer;
        
        public ReferenceImporterTest()
        {
            _module = new ModuleDefinition("SomeModule.dll");
            _importer = new ReferenceImporter(_module);
        }
        
        [Fact]
        public void ImportNewAssemblyShouldAddToModule()
        {
            var result = _importer.ImportScope(_dummyAssembly);
            
            Assert.Equal(_dummyAssembly, result, _comparer);
            Assert.Contains(result, _module.AssemblyReferences);
        }

        [Fact]
        public void ImportExistingAssemblyShouldUseExistingAssembly()
        {
            _module.AssemblyReferences.Add(_dummyAssembly);

            int count = _module.AssemblyReferences.Count;
            
            var copy = new AssemblyReference(_dummyAssembly);
            var result = _importer.ImportScope(copy);
            
            Assert.Same(_dummyAssembly, result);
            Assert.Equal(count, _module.AssemblyReferences.Count);
        }

        [Fact]
        public void ImportNewTypeShouldCreateNewReference()
        {
            var type = new TypeReference(_dummyAssembly, "SomeNamespace", "SomeName");
            var result = _importer.ImportType(type);

            Assert.Equal(type, result, _comparer);
            Assert.Equal(_module, result.Module);
        }

        [Fact]
        public void ImportAlreadyImportedTypeShouldUseSameInstance()
        {
            var type = new TypeReference(_dummyAssembly, "SomeNamespace", "SomeName");
            var importedType = _importer.ImportType(type);
            
            var result = _importer.ImportType(importedType);

            Assert.Same(importedType, result);
        }

        [Fact]
        public void ImportTypeDefFromDifferentModuleShouldReturnTypeRef()
        {
            var assembly = new AssemblyDefinition("ExternalAssembly", new Version(1, 2, 3, 4));
            assembly.Modules.Add(new ModuleDefinition("ExternalAssembly.dll"));
            var definition = new TypeDefinition("SomeNamespace", "SomeName", TypeAttributes.Public);
            assembly.ManifestModule.TopLevelTypes.Add(definition);

            var result = _importer.ImportType(definition);

            Assert.IsAssignableFrom<TypeReference>(result);
            Assert.Equal(definition, result, _comparer);
        }
        
        [Fact]
        public void ImportTypeDefInSameModuleShouldReturnSameInstance()
        {
            var definition = new TypeDefinition("SomeNamespace", "SomeName", TypeAttributes.Public);
            _module.TopLevelTypes.Add(definition);
            
            var importedType = _importer.ImportType(definition);

            Assert.Same(definition, importedType);
        }
    }
}