using System.IO;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.NestedClasses;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class AssemblyResolverTest
    {
        private readonly SignatureComparer _comparer = new SignatureComparer();

        [Fact]
        public void ResolveCorLib()
        {
            var assemblyName = typeof(object).Assembly.GetName();
            var assemblyRef = new AssemblyReference(
                assemblyName.Name,
                assemblyName.Version, 
                false,
                assemblyName.GetPublicKeyToken());
         
            var resolver = new NetCoreAssemblyResolver();
            var assemblyDef = resolver.Resolve(assemblyRef);

            Assert.NotNull(assemblyDef);
            Assert.Equal(assemblyName.Name, assemblyDef.Name);
        }

        [Fact]
        public void ResolveLocalLibrary()
        {
            var resolver = new NetCoreAssemblyResolver();
            resolver.SearchDirectories.Add(Path.GetDirectoryName(typeof(AssemblyResolverTest).Assembly.Location));
         
            var assemblyDef = AssemblyDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location);
            var assemblyRef = new AssemblyReference(assemblyDef);

            Assert.Equal(assemblyDef, resolver.Resolve(assemblyRef), _comparer);

            resolver.ClearCache();
            Assert.False(resolver.HasCached(assemblyRef));
            
            resolver.AddToCache(assemblyRef, assemblyDef);
            Assert.True(resolver.HasCached(assemblyRef));
            Assert.Equal(assemblyDef, resolver.Resolve(assemblyRef));

            resolver.RemoveFromCache(assemblyRef);
            Assert.NotEqual(assemblyDef, resolver.Resolve(assemblyRef));

        }
    }
}