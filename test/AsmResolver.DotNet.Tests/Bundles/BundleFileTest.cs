using System.Linq;
using System.Text;
using AsmResolver.DotNet.Bundles;
using Xunit;

namespace AsmResolver.DotNet.Tests.Bundles
{
    public class BundleFileTest
    {
        [Fact]
        public void ReadUncompressedStringContents()
        {
            var manifest = BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V6);
            var file = manifest.Files.First(f => f.Type == BundleFileType.RuntimeConfigJson);
            string contents = Encoding.UTF8.GetString(file.GetData()).Replace("\r", "");

            Assert.Equal(@"{
  ""runtimeOptions"": {
    ""tfm"": ""net6.0"",
    ""framework"": {
      ""name"": ""Microsoft.NETCore.App"",
      ""version"": ""6.0.0""
    },
    ""configProperties"": {
      ""System.Reflection.Metadata.MetadataUpdater.IsSupported"": false
    }
  }
}".Replace("\r", ""), contents);
        }

        [Fact]
        public void ReadUncompressedAssemblyContents()
        {
            var manifest = BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V6);
            var bundleFile = manifest.Files.First(f => f.RelativePath == "HelloWorld.dll");

            var embeddedImage = ModuleDefinition.FromBytes(bundleFile.GetData());
            Assert.Equal("HelloWorld.dll", embeddedImage.Name);
        }
    }
}
