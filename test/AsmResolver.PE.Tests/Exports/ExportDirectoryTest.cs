using System.IO;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Builder;
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
            Assert.Equal("SimpleDll.dll", image.Exports?.Name);
        }

        [Fact]
        public void ReadExportNames()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll_Exports);
            Assert.Equal(new[]
            {
                "NamedExport1",
                "NamedExport2",
            }, image.Exports?.Entries.Select(e => e.Name));
        }

        [Fact]
        public void ReadExportAddresses()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll_Exports);
            Assert.Equal(new[]
            {
                0x000111DBu,
                0x00011320u,
            }, image.Exports?.Entries.Select(e => e.Address.Rva));
        }

        [Fact]
        public void ReadOrdinals()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll_Exports);
            Assert.Equal(new[]
            {
                1u,
                2u,
            }, image.Exports?.Entries.Select(e => e.Ordinal));
        }

        [Fact]
        public void ChangeBaseOrdinalShouldUpdateAllOrdinals()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll_Exports);
            image.Exports!.BaseOrdinal = 10;
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
            var export = image.Exports!.Entries[0];
            image.Exports.Entries.RemoveAt(0);
            Assert.Equal(0u, export.Ordinal);
            Assert.Equal(1u, image.Exports.Entries[0].Ordinal);
        }

        [Fact]
        public void InsertExportShouldUpdateOrdinals()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll_Exports);
            image.Exports!.Entries.Insert(0, new ExportedSymbol(new VirtualAddress(0x1234), "NewExport"));
            Assert.Equal(new[]
            {
                1u,
                2u,
                3u,
            }, image.Exports.Entries.Select(e => e.Ordinal));
        }

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

        [Fact]
        public void PersistentExportLibraryName()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);
            image.Exports = new ExportDirectory("HelloWorld.dll")
            {
                Entries = {new ExportedSymbol(new VirtualAddress(0x12345678), "TestExport")}
            };
            var newImage = RebuildAndReloadManagedPE(image);
            Assert.Equal(image.Exports.Name, newImage.Exports?.Name);
        }

        [Fact]
        public void PersistentExportedSymbol()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);

            // Prepare mock.
            var exportDirectory = new ExportDirectory("HelloWorld.dll");
            var exportedSymbol = new ExportedSymbol(new VirtualAddress(0x12345678), "TestExport");
            exportDirectory.Entries.Add(exportedSymbol);
            image.Exports = exportDirectory;

            // Rebuild.
            var newImage = RebuildAndReloadManagedPE(image);

            // Verify.
            Assert.Equal(1, newImage.Exports!.Entries.Count);
            var newExportedSymbol = newImage.Exports.Entries[0];
            Assert.Equal(exportedSymbol.Name, newExportedSymbol.Name);
            Assert.Equal(exportedSymbol.Ordinal, newExportedSymbol.Ordinal);
            Assert.Equal(exportedSymbol.Address.Rva, newExportedSymbol.Address.Rva);
        }

        [Fact]
        public void PersistentExportedSymbolMany()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);

            // Prepare mock.
            var exportDirectory = new ExportDirectory("HelloWorld.dll");
            var exportedSymbol1 = new ExportedSymbol(new VirtualAddress(0x12345678), "TestExport1");
            var exportedSymbol2 = new ExportedSymbol(new VirtualAddress(0xabcdef00), "TestExport2");
            var exportedSymbol3 = new ExportedSymbol(new VirtualAddress(0x1337c0de), "TestExport3");
            exportDirectory.Entries.Add(exportedSymbol1);
            exportDirectory.Entries.Add(exportedSymbol2);
            exportDirectory.Entries.Add(exportedSymbol3);
            image.Exports = exportDirectory;

            // Rebuild.
            var newImage = RebuildAndReloadManagedPE(image);

            // Verify.
            Assert.Equal(3, newImage.Exports!.Entries.Count);
            var newExportedSymbol = newImage.Exports.Entries[0];
            Assert.Equal(exportedSymbol1.Name, newExportedSymbol.Name);
            Assert.Equal(exportedSymbol1.Ordinal, newExportedSymbol.Ordinal);
            Assert.Equal(exportedSymbol1.Address.Rva, newExportedSymbol.Address.Rva);
            newExportedSymbol = newImage.Exports.Entries[1];
            Assert.Equal(exportedSymbol2.Name, newExportedSymbol.Name);
            Assert.Equal(exportedSymbol2.Ordinal, newExportedSymbol.Ordinal);
            Assert.Equal(exportedSymbol2.Address.Rva, newExportedSymbol.Address.Rva);
            newExportedSymbol = newImage.Exports.Entries[2];
            Assert.Equal(exportedSymbol3.Name, newExportedSymbol.Name);
            Assert.Equal(exportedSymbol3.Ordinal, newExportedSymbol.Ordinal);
            Assert.Equal(exportedSymbol3.Address.Rva, newExportedSymbol.Address.Rva);
        }
    }
}
