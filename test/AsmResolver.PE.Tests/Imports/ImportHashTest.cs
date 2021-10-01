using AsmResolver.PE.Imports;
using Xunit;

namespace AsmResolver.PE.Tests.Imports
{
    public class ImportHashTest
    {
        [Fact]
        public void SimpleDllImportHash()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll);
            byte[] hash = image.GetImportHash();

            Assert.Equal(new byte[]
            {
                0xf4, 0x69, 0x90, 0x30, 0xd0, 0xf3, 0xc7, 0x7a, 0xdc, 0x12, 0x05, 0xa3, 0xd1, 0xee, 0xdb, 0x3b,
            }, hash);
        }
    }
}
