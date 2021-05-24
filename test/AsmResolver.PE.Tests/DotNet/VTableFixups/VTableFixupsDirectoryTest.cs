using System.IO;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Builder;
using AsmResolver.PE.DotNet.VTableFixups;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.VTableFixups
{
    public class VTableFixupsDirectoryTest
    {
        private static IPEImage RebuildAndReloadManagedPE(IPEImage image)
        {
            // Build.
            using var tempStream = new MemoryStream();
            var builder = new ManagedPEFileBuilder();
            var newPeFile = builder.CreateFile(image);
            newPeFile.Write(new BinaryStreamWriter(tempStream));

            // Reload.
            var newImage = PEImage.FromBytes(tempStream.ToArray());
            return newImage;
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void ReadVTableTokens(bool is32Bit, bool rebuild)
        {
            var peImage = PEImage.FromBytes(is32Bit
                ? Properties.Resources.UnmanagedExports_x32
                : Properties.Resources.UnmanagedExports_x64);
            if (rebuild)
                peImage = RebuildAndReloadManagedPE(peImage);

            var fixups = peImage.DotNetDirectory.VTableFixups;
            int token = 0x06000001;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++, token++)
                {
                    Assert.Equal(token, fixups[i].Tokens[j]);
                }
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void ReadVTableFixupsDirectory(bool is32Bit, bool rebuild)
        {
            var peImage = PEImage.FromBytes(is32Bit
                ? Properties.Resources.UnmanagedExports_x32
                : Properties.Resources.UnmanagedExports_x64);
            if (rebuild)
                peImage = RebuildAndReloadManagedPE(peImage);

            var fixups = peImage.DotNetDirectory.VTableFixups;

            Assert.Equal(2, fixups.Count);
            Assert.Equal(3, fixups[0].Tokens.Count);
            Assert.Equal(3, fixups[1].Tokens.Count);
            var entrySize = is32Bit ? VTableType.VTable32Bit : VTableType.VTable64Bit;
            Assert.Equal(entrySize | VTableType.VTableFromUnmanaged, fixups[0].Tokens.Type);
            Assert.Equal(entrySize | VTableType.VTableFromUnmanaged, fixups[1].Tokens.Type);
        }
    }
}
