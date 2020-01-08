using System;
using System.IO;
using AsmResolver.DotNet.Blob;
using AsmResolver.DotNet.TestCases.NestedClasses;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class MetadataResolverTest
    {
        private readonly DefaultMetadataResolver _resolver;
        private readonly SignatureComparer _comparer;
        
        public MetadataResolverTest()
        {
            _resolver = new DefaultMetadataResolver(new DefaultAssemblyResolver()
            {
                SearchDirectories =
                {
                    Path.GetDirectoryName(typeof(MetadataResolverTest).Assembly.Location)
                }
            });
            _comparer = new SignatureComparer();
        }
        
        [Fact]
        public void ResolveSystemObjectFramework()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            var reference = new TypeReference(module.CorLibTypeFactory.CorLibScope, "System", "Object");
            var definition = _resolver.ResolveType(reference);

            Assert.Equal((ITypeDefOrRef) reference, definition, _comparer);
        }
        
        [Fact]
        public void ResolveType()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location);

            var topLevelClass1 = new TypeReference(new AssemblyReference(module.Assembly),
                typeof(TopLevelClass1).Namespace, typeof(TopLevelClass1).Name);
            
            var definition = _resolver.ResolveType(topLevelClass1);
            Assert.Equal((ITypeDefOrRef) topLevelClass1, definition, _comparer);
        }
        
        [Fact]
        public void ResolveNestedType()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location);

            var topLevelClass1 = new TypeReference(new AssemblyReference(module.Assembly),
                typeof(TopLevelClass1).Namespace, typeof(TopLevelClass1).Name);
            var nested1 = new TypeReference(topLevelClass1,null, typeof(TopLevelClass1.Nested1).Name);
            
            var definition = _resolver.ResolveType(nested1);

            Assert.Equal((ITypeDefOrRef) nested1, definition, _comparer);
        }
        
        [Fact]
        public void ResolveNestedNestedType()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location);

            var topLevelClass1 = new TypeReference(new AssemblyReference(module.Assembly),
                typeof(TopLevelClass1).Namespace, typeof(TopLevelClass1).Name);
            var nested1 = new TypeReference(topLevelClass1,null, typeof(TopLevelClass1.Nested1).Name);
            var nested1nested1 = new TypeReference(nested1,null, typeof(TopLevelClass1.Nested1.Nested1Nested1).Name);
            
            var definition = _resolver.ResolveType(nested1nested1);

            Assert.Equal((ITypeDefOrRef) nested1nested1, definition, _comparer);
        }
        
    }
}