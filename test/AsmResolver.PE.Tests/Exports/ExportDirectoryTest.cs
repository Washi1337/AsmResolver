using System.Linq;
using Xunit;

namespace AsmResolver.PE.Tests.Exports
{
    public class ExportDirectoryTest
    {
        [Fact]
        public void ReadName()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll_Exports);
            Assert.Equal("SimpleDll.dll", image.Exports.Name);
        }
        
        [Fact]
        public void ReadExportNames()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll_Exports);
            Assert.Equal(new[]
            {
                "NamedExport1",
                "NamedExport2",
            }, image.Exports.Entries.Select(e => e.Name));
        }
        
        [Fact]
        public void ReadExportAddresses()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll_Exports);
            Assert.Equal(new[]
            {
                0x000111DBu,
                0x00011320u,
            }, image.Exports.Entries.Select(e => e.Address.Rva));
        }
    }
}