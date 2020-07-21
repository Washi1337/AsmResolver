using System.IO;
using System.Linq;
using AsmResolver.PE.DotNet.Builder;
using AsmResolver.PE.Win32Resources.Icon;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.Icon
{
    public class IconResourceTest
    {
        [Fact]
        public void ReadIconGroupResourceDirectory()
        {
            // Load dummy.
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);

            // Read icon resources.
            var iconResource = IconResource.FromDirectory(image.Resources);

            // Verify.
            Assert.Single(iconResource.GetIconGroups());
            Assert.Equal(4, iconResource.GetIconGroups().ToList()[0].GetIconEntries().Count());
        }

        [Fact]
        public void PersistentIconResources()
        {
            // Load dummy.
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);

            // Update icon resources.
            var iconResource = IconResource.FromDirectory(image.Resources);
            foreach (var iconGroup in iconResource.GetIconGroups())
            {
                iconGroup.RemoveEntry(4);
                iconGroup.Count--;
            }
            iconResource.WriteToDirectory(image.Resources);

            // Rebuild.
            using var stream = new MemoryStream();
            new ManagedPEFileBuilder().CreateFile(image).Write(new BinaryStreamWriter(stream));

            // Reload version info.
            var newImage = PEImage.FromBytes(stream.ToArray());
            var newIconResource = IconResource.FromDirectory(newImage.Resources);

            // Verify.
            Assert.Equal(iconResource.GetIconGroups().ToList()[0].Count, newIconResource.GetIconGroups().ToList()[0].Count);
        }
    }
}