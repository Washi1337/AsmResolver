using System.Linq;
using AsmResolver.PE.Exports;
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

        [Fact]
        public void ReadOrdinals()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll_Exports);
            Assert.Equal(new[]
            {
                1u,
                2u,
            }, image.Exports.Entries.Select(e => e.Ordinal));
        }

        [Fact]
        public void ChangeBaseOrdinalShouldUpdateAllOrdinals()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll_Exports);
            image.Exports.BaseOrdinal = 10;
            Assert.Equal(new[]
            {
                10u,
                11u,
            }, image.Exports.Entries.Select(e => e.Ordinal));
        }

        [Fact]
        public void RemoveExportShouldUpdateOrdinals()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll_Exports);
            var export = image.Exports.Entries[0];
            image.Exports.Entries.RemoveAt(0);
            Assert.Equal(0u, export.Ordinal);
            Assert.Equal(1u, image.Exports.Entries[0].Ordinal);
        }

        [Fact]
        public void InsertExportShouldUpdateOrdinals()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll_Exports);
            image.Exports.Entries.Insert(0, new ExportedSymbol(new VirtualAddress(0x1234), "NewExport"));
            Assert.Equal(new[]
            {
                1u,
                2u,
                3u,
            }, image.Exports.Entries.Select(e => e.Ordinal));
            
        }
    }
}