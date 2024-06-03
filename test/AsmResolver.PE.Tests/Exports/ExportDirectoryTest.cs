using System;
using System.IO;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.Builder;
using AsmResolver.PE.Exports;
using AsmResolver.PE.Exports.Builder;
using AsmResolver.PE.File;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.PE.Tests.Exports
{
    public class ExportDirectoryTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly TemporaryDirectoryFixture _fixture;

        public ExportDirectoryTest(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

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
            }, image.Exports?.Entries.Select(e => e.Name) ?? Array.Empty<string>());
        }

        [Fact]
        public void ReadExportAddresses()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll_Exports);
            Assert.NotNull(image.Exports);
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
            Assert.NotNull(image.Exports);
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
            Assert.Single(newImage.Exports!.Entries);
            var newExportedSymbol = newImage.Exports.Entries[0];
            Assert.Equal(exportedSymbol.Name, newExportedSymbol.Name);
            Assert.Equal(exportedSymbol.Ordinal, newExportedSymbol.Ordinal);
            Assert.Equal(exportedSymbol.Address.Rva, newExportedSymbol.Address.Rva);
        }

        [Fact]
        public void PersistentExportedSymbolByOrdinal()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);

            // Prepare mock.
            var exportDirectory = new ExportDirectory("HelloWorld.dll");
            var exportedSymbol = new ExportedSymbol(new VirtualAddress(0x12345678));
            exportDirectory.Entries.Add(exportedSymbol);
            image.Exports = exportDirectory;

            // Rebuild.
            var newImage = RebuildAndReloadManagedPE(image);

            // Verify.
            Assert.Single(newImage.Exports!.Entries);
            var newExportedSymbol = newImage.Exports.Entries[0];
            Assert.True(exportedSymbol.IsByOrdinal);
            Assert.Equal(exportedSymbol.Ordinal, newExportedSymbol.Ordinal);
            Assert.Equal(exportedSymbol.Address.Rva, newExportedSymbol.Address.Rva);
        }

        [Fact]
        public void PersistentExportedSymbolMany()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);

            // Prepare mock.
            var exportDirectory = new ExportDirectory("HelloWorld.dll");
            var namedSymbol1 = new ExportedSymbol(new VirtualAddress(0x12345678), "TestExport1");
            var unnamedSymbol1 = new ExportedSymbol(new VirtualAddress(0x11112222));
            var namedSymbol2 = new ExportedSymbol(new VirtualAddress(0xabcdef00), "TestExport2");
            var unnamedSymbol2 = new ExportedSymbol(new VirtualAddress(0x33334444));
            var namedSymbol3 = new ExportedSymbol(new VirtualAddress(0x1337c0de), "TestExport3");
            var unnamedSymbol3 = new ExportedSymbol(new VirtualAddress(0x55556666));
            exportDirectory.Entries.Add(namedSymbol1);
            exportDirectory.Entries.Add(unnamedSymbol1);
            exportDirectory.Entries.Add(namedSymbol2);
            exportDirectory.Entries.Add(unnamedSymbol2);
            exportDirectory.Entries.Add(namedSymbol3);
            exportDirectory.Entries.Add(unnamedSymbol3);
            image.Exports = exportDirectory;

            // Rebuild.
            var newImage = RebuildAndReloadManagedPE(image);

            // Verify.
            Assert.Equal(6, newImage.Exports!.Entries.Count);
            var newSymbol = newImage.Exports.Entries[0];
            Assert.Equal(namedSymbol1.Name, newSymbol.Name);
            Assert.Equal(namedSymbol1.Ordinal, newSymbol.Ordinal);
            Assert.Equal(namedSymbol1.Address.Rva, newSymbol.Address.Rva);
            newSymbol = newImage.Exports.Entries[1];
            Assert.True(newSymbol.IsByOrdinal);
            Assert.Equal(unnamedSymbol1.Ordinal, newSymbol.Ordinal);
            Assert.Equal(unnamedSymbol1.Address.Rva, newSymbol.Address.Rva);
            newSymbol = newImage.Exports.Entries[2];
            Assert.Equal(namedSymbol2.Name, newSymbol.Name);
            Assert.Equal(namedSymbol2.Ordinal, newSymbol.Ordinal);
            Assert.Equal(namedSymbol2.Address.Rva, newSymbol.Address.Rva);
            newSymbol = newImage.Exports.Entries[3];
            Assert.True(newSymbol.IsByOrdinal);
            Assert.Equal(unnamedSymbol2.Ordinal, newSymbol.Ordinal);
            Assert.Equal(unnamedSymbol2.Address.Rva, newSymbol.Address.Rva);
            newSymbol = newImage.Exports.Entries[4];
            Assert.Equal(namedSymbol3.Name, newSymbol.Name);
            Assert.Equal(namedSymbol3.Ordinal, newSymbol.Ordinal);
            Assert.Equal(namedSymbol3.Address.Rva, newSymbol.Address.Rva);
            newSymbol = newImage.Exports.Entries[5];
            Assert.True(newSymbol.IsByOrdinal);
            Assert.Equal(unnamedSymbol3.Ordinal, newSymbol.Ordinal);
            Assert.Equal(unnamedSymbol3.Address.Rva, newSymbol.Address.Rva);
        }

        [Fact]
        public void ReadForwarderSymbol()
        {
            var exports = PEImage.FromBytes(Properties.Resources.ForwarderDlls_ProxyDll).Exports!;

            var bar = exports.Entries.First(x => x.Name == "Bar");
            var baz = exports.Entries.First(x => x.Name == "Baz");

            Assert.True(bar.IsForwarder);
            Assert.Equal("ActualDll.Foo", bar.ForwarderName);
            Assert.False(baz.IsForwarder);
        }

        [Fact]
        public void RebuildForwarderSymbol()
        {
            var file = PEFile.FromBytes(Properties.Resources.ForwarderDlls_ProxyDll);

            // Update a forwarder name.
            var exports = PEImage.FromFile(file).Exports!;
            var bar = exports.Entries.First(x => x.Name == "Bar");
            bar.ForwarderName = "ActualDll.Bar";

            // Rebuild export directory.
            var buffer = new ExportDirectoryBuffer();
            buffer.AddDirectory(exports);

            var section = new PESection(
                ".export",
                SectionFlags.MemoryRead | SectionFlags.ContentInitializedData,
                buffer);

            // Rebuild.
            file.Sections.Add(section);
            file.UpdateHeaders();
            file.OptionalHeader.SetDataDirectory(
                DataDirectoryIndex.ExportDirectory,
                new DataDirectory(section.Rva, section.GetPhysicalSize()));

            using var stream = new MemoryStream();
            file.Write(stream);

            // Verify new forwarder symbol is present.
            var newExports = PEImage.FromBytes(stream.ToArray()).Exports!;
            var newBar = newExports.Entries.First(x => x.Name == "Bar");
            Assert.Equal("ActualDll.Bar", newBar.ForwarderName);

            // Try running it.
            var runner = _fixture.GetRunner<NativePERunner>();
            string basePath = runner.GetTestDirectory(nameof(ExportDirectoryTest), nameof(RebuildForwarderSymbol));
            string exePath = Path.Combine(basePath, "ForwarderTest.exe");

            System.IO.File.WriteAllBytes(
                Path.Combine(basePath, "ActualDll.dll"),
                Properties.Resources.ForwarderDlls_ActualDll);
            System.IO.File.WriteAllBytes(
                exePath,
                Properties.Resources.ForwarderDlls_ForwarderTest);
            System.IO.File.WriteAllBytes(
                Path.Combine(basePath, "ProxyDll.dll"),
                stream.ToArray());

            string output = runner.RunAndCaptureOutput(exePath);
            Assert.Equal("ActualDLL::Bar\nProxyDll::Baz\nHello World!\n", output);
        }
    }
}
