using System.Linq;
using System.Text;
using AsmResolver.PE.DotNet.Bundles;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Bundles
{
    public class BundleFileTest
    {
        [Fact]
        public void ReadUncompressedStringContents()
        {
            var manifest = BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V6);
            var file = manifest.Files.First(f => f.Type == BundleFileType.RuntimeConfigJson);
            string contents = Encoding.UTF8.GetString(file.GetData());

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
}", contents);
        }

        [Fact]
        public void ReadUncompressedAssemblyContents()
        {
            var manifest = BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V6);
            var bundleFile = manifest.Files.First(f => f.RelativePath == "HelloWorld.dll");

            var embeddedImage = PEImage.FromBytes(bundleFile.GetData());
            var metadata = embeddedImage.DotNetDirectory!.Metadata!;

            uint nameIndex = metadata
                .GetStream<TablesStream>()
                .GetTable<ModuleDefinitionRow>()
                .GetByRid(1)
                .Name;

            string? name = metadata
                .GetStream<StringsStream>()
                .GetStringByIndex(nameIndex);

            Assert.Equal("HelloWorld.dll", name);
        }
    }
}
