using System.Linq;
using AsmResolver.PE.DotNet.Bundles;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Bundles
{
    public class BundleManifestTest
    {
        [Fact]
        public void ReadBundleManifestHeaderV1()
        {
            var manifest = BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V1);
            Assert.Equal(1u, manifest.MajorVersion);
            Assert.Equal("j7LK4is5ipe1CCtiafaTb8uhSOR7JhI=", manifest.BundleID);
            Assert.Equal(new[]
            {
                "HelloWorld.dll", "HelloWorld.deps.json", "HelloWorld.runtimeconfig.json"
            }, manifest.Files.Select(f => f.RelativePath));
        }

        [Fact]
        public void ReadBundleManifestHeaderV2()
        {
            var manifest = BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V2);
            Assert.Equal(2u, manifest.MajorVersion);
            Assert.Equal("poUQ+RBCefcEL4xrSAXdE2I5M+5D_Pk=", manifest.BundleID);
            Assert.Equal(new[]
            {
                "HelloWorld.dll", "HelloWorld.deps.json", "HelloWorld.runtimeconfig.json"
            }, manifest.Files.Select(f => f.RelativePath));
        }

        [Fact]
        public void ReadBundleManifestHeaderV6()
        {
            var manifest = BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V6);
            Assert.Equal(6u, manifest.MajorVersion);
            Assert.Equal("lc43r48XAQNxN7Cx8QQvO9JgZI5lqPA=", manifest.BundleID);
            Assert.Equal(new[]
            {
                "HelloWorld.dll", "HelloWorld.deps.json", "HelloWorld.runtimeconfig.json"
            }, manifest.Files.Select(f => f.RelativePath));
        }
    }
}
