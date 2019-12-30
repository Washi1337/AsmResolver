using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class AssemblyResolverTest
    {
        [Fact]
        public void ResolveCorLib()
        {
            var assemblyName = typeof(object).Assembly.GetName();
            var assemblyRef = new AssemblyReference(
                assemblyName.Name,
                assemblyName.Version, 
                false,
                assemblyName.GetPublicKeyToken());
         
            var resolver = new DefaultAssemblyResolver();
            var assemblyDef = resolver.Resolve(assemblyRef);

            Assert.NotNull(assemblyDef);
            Assert.Equal(assemblyName.Name, assemblyDef.Name);
        }
    }
}