using System.IO;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.Builder;
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
            var iconResource = IconResource.FromDirectory(image.Resources!)!;
            Assert.NotNull(iconResource);

            // Verify.
            Assert.Single(iconResource.GetIconGroups());
            Assert.Equal(4, iconResource.GetIconGroups().ToList()[0].GetIconEntries().Count());
        }

        [Fact]
        public void PersistentIconResources()
        {
            // Load dummy.
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var resources = image.Resources!;

            // Update icon resources.
            var iconResource = IconResource.FromDirectory(resources)!;
            Assert.NotNull(iconResource);

            foreach (var iconGroup in iconResource.GetIconGroups())
            {
                iconGroup.RemoveEntry(4);
                iconGroup.Count--;
            }
            iconResource.InsertIntoDirectory(resources);

            // Rebuild.
            using var stream = new MemoryStream();
            new ManagedPEFileBuilder().CreateFile(image).Write(new BinaryStreamWriter(stream));

            // Reload version info.
            var newImage = PEImage.FromBytes(stream.ToArray());
            var newIconResource = IconResource.FromDirectory(newImage.Resources!)!;
            Assert.NotNull(newIconResource);

            // Verify.
            Assert.Equal(iconResource.GetIconGroups().ToList()[0].Count, newIconResource.GetIconGroups().ToList()[0].Count);
        }
    }
}
