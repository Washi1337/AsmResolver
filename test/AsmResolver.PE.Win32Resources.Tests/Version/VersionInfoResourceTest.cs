using System.Diagnostics;
using System.IO;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Builder;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Win32Resources.Builder;
using AsmResolver.PE.Win32Resources.Version;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.Version
{
    public class VersionInfoResourceTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly TemporaryDirectoryFixture _fixture;

        public VersionInfoResourceTest(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void ReadFixedVersion()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);

            var versionInfo = VersionInfoResource.FromDirectory(image.Resources!);
            Assert.NotNull(versionInfo);
            var fixedVersionInfo = versionInfo!.FixedVersionInfo;

            Assert.Equal(new System.Version(1,0,0,0), fixedVersionInfo.FileVersion);
            Assert.Equal(new System.Version(1,0,0,0), fixedVersionInfo.ProductVersion);
        }

        [Fact]
        public void PersistentFixedVersionInfo()
        {
            // Prepare mock data.
            var versionInfo = new VersionInfoResource();
            var fixedVersionInfo = new FixedVersionInfo
            {
                FileVersion = new System.Version(1, 2, 3, 4),
                ProductVersion = new System.Version(1, 2, 3, 4),
                FileDate = 0x12345678_9ABCDEF,
                FileFlags = FileFlags.SpecialBuild,
                FileFlagsMask = FileFlags.ValidBitMask,
                FileType = FileType.App,
                FileOS = FileOS.NT,
                FileSubType = FileSubType.DriverInstallable,
            };
            versionInfo.FixedVersionInfo = fixedVersionInfo;

            // Serialize.
            var tempStream = new MemoryStream();
            versionInfo.Write(new BinaryStreamWriter(tempStream));

            // Reload.
            var infoReader = new BinaryStreamReader(tempStream.ToArray());
            var newVersionInfo = VersionInfoResource.FromReader(ref infoReader);
            var newFixedVersionInfo = newVersionInfo.FixedVersionInfo;

            // Verify.
            Assert.Equal(fixedVersionInfo.FileVersion, newFixedVersionInfo.FileVersion);
            Assert.Equal(fixedVersionInfo.ProductVersion, newFixedVersionInfo.ProductVersion);
            Assert.Equal(fixedVersionInfo.FileDate, newFixedVersionInfo.FileDate);
            Assert.Equal(fixedVersionInfo.FileFlags, newFixedVersionInfo.FileFlags);
            Assert.Equal(fixedVersionInfo.FileFlagsMask, newFixedVersionInfo.FileFlagsMask);
            Assert.Equal(fixedVersionInfo.FileType, newFixedVersionInfo.FileType);
            Assert.Equal(fixedVersionInfo.FileOS, newFixedVersionInfo.FileOS);
            Assert.Equal(fixedVersionInfo.FileSubType, newFixedVersionInfo.FileSubType);
        }

        [Fact]
        public void ReadStringFileInfo()
        {
            string path = typeof(PEImage).Assembly.Location;

            var image = PEImage.FromFile(path);
            var versionInfo = VersionInfoResource.FromDirectory(image.Resources!)!;
            Assert.NotNull(versionInfo);

            var expectedInfo = FileVersionInfo.GetVersionInfo(path);
            var actualInfo = versionInfo.GetChild<StringFileInfo>(StringFileInfo.StringFileInfoKey);

            foreach ((string key, string value) in actualInfo.Tables[0])
            {
                string expected = key switch
                {
                    StringTable.CommentsKey => expectedInfo.Comments,
                    StringTable.CompanyNameKey => expectedInfo.CompanyName,
                    StringTable.FileDescriptionKey => expectedInfo.FileDescription,
                    StringTable.FileVersionKey => expectedInfo.FileVersion,
                    StringTable.InternalNameKey => expectedInfo.InternalName,
                    StringTable.LegalCopyrightKey => expectedInfo.LegalCopyright,
                    StringTable.LegalTrademarksKey => expectedInfo.LegalTrademarks,
                    StringTable.OriginalFilenameKey => expectedInfo.OriginalFilename,
                    StringTable.PrivateBuildKey => expectedInfo.PrivateBuild,
                    StringTable.ProductNameKey => expectedInfo.ProductName,
                    StringTable.ProductVersionKey => expectedInfo.ProductVersion,
                    StringTable.SpecialBuildKey => expectedInfo.SpecialBuild,
                    _ => null,
                };

                if (expected is null)
                    continue;

                Assert.Equal(expected, value);
            }
        }

        [Fact]
        public void PersistentVarFileInfo()
        {
            // Prepare mock data.
            var versionInfo = new VersionInfoResource();

            var varFileInfo = new VarFileInfo();
            var table = new VarTable();
            for (ushort i = 0; i < 10; i++)
                table.Values.Add(i);
            varFileInfo.Tables.Add(table);

            versionInfo.AddEntry(varFileInfo);

            // Serialize.
            var tempStream = new MemoryStream();
            versionInfo.Write(new BinaryStreamWriter(tempStream));

            // Reload.
            var infoReader = new BinaryStreamReader(tempStream.ToArray());
            var newVersionInfo = VersionInfoResource.FromReader(ref infoReader);

            // Verify.
            var newVarFileInfo = newVersionInfo.GetChild<VarFileInfo>(VarFileInfo.VarFileInfoKey);
            Assert.NotNull(newVarFileInfo);
            Assert.Single(newVarFileInfo.Tables);

            var newTable = newVarFileInfo.Tables[0];
            Assert.Equal(table.Values, newTable.Values);
        }

        [Fact]
        public void PersistentStringFileInfo()
        {
            // Prepare mock data.
            var versionInfo = new VersionInfoResource();

            var stringFileInfo = new StringFileInfo();
            var table = new StringTable(0, 0x4b0)
            {
                [StringTable.ProductNameKey] = "Sample product",
                [StringTable.FileVersionKey] = "1.2.3.4",
                [StringTable.ProductVersionKey] = "1.0.0.0",
                [StringTable.FileDescriptionKey] = "This is a sample description"
            };
            stringFileInfo.Tables.Add(table);

            versionInfo.AddEntry(stringFileInfo);

            // Serialize.
            var tempStream = new MemoryStream();
            versionInfo.Write(new BinaryStreamWriter(tempStream));

            // Reload.
            var infoReader = new BinaryStreamReader(tempStream.ToArray());
            var newVersionInfo = VersionInfoResource.FromReader(ref infoReader);

            // Verify.
            var newStringFileInfo = newVersionInfo.GetChild<StringFileInfo>(StringFileInfo.StringFileInfoKey);
            Assert.NotNull(newStringFileInfo);
            Assert.Single(newStringFileInfo.Tables);

            var newTable = newStringFileInfo.Tables[0];
            foreach ((string key, string value) in table)
                Assert.Equal(value, newTable[key]);
        }

        [Fact]
        public void PersistentVersionResource()
        {
            // Load dummy
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var resources = image.Resources!;

            // Update version info.
            var versionInfo = VersionInfoResource.FromDirectory(resources)!;
            Assert.NotNull(versionInfo);

            versionInfo.FixedVersionInfo.ProductVersion = new System.Version(1, 2, 3, 4);
            versionInfo.InsertIntoDirectory(resources);

            // Rebuild
            using var stream = new MemoryStream();
            new ManagedPEFileBuilder().CreateFile(image).Write(new BinaryStreamWriter(stream));

            // Reload version info.
            var newImage = PEImage.FromBytes(stream.ToArray());
            var newVersionInfo = VersionInfoResource.FromDirectory(newImage.Resources!)!;
            Assert.NotNull(newVersionInfo);

            // Verify.
            Assert.Equal(versionInfo.FixedVersionInfo.ProductVersion, newVersionInfo.FixedVersionInfo.ProductVersion);
        }

        [Fact]
        public void VersionInfoAlignment()
        {
            // https://github.com/Washi1337/AsmResolver/issues/202

            // Open dummy
            var file = PEFile.FromBytes(Properties.Resources.HelloWorld_PaddedVersionInfo);
            var image = PEImage.FromFile(file);

            // Update version info.
            var versionInfo = VersionInfoResource.FromDirectory(image.Resources!)!;
            var info = versionInfo.GetChild<StringFileInfo>(StringFileInfo.StringFileInfoKey);
            info.Tables[0][StringTable.FileDescriptionKey] = "This is a test application";
            versionInfo.InsertIntoDirectory(image.Resources!);

            // Replace section.
            var resourceBuffer = new ResourceDirectoryBuffer();
            resourceBuffer.AddDirectory(image.Resources!);

            var section = file.GetSectionContainingRva(file.OptionalHeader.GetDataDirectory(DataDirectoryIndex.ResourceDirectory).VirtualAddress);
            section.Contents = resourceBuffer;

            file.UpdateHeaders();
            file.OptionalHeader.SetDataDirectory(DataDirectoryIndex.ResourceDirectory,
                new DataDirectory(resourceBuffer.Rva, resourceBuffer.GetPhysicalSize()));

            // Rebuild
            using var stream = new MemoryStream();
            file.Write(stream);

            // Reopen and verify.
            var newImage = PEImage.FromBytes(stream.ToArray());
            var newVersionInfo = VersionInfoResource.FromDirectory(newImage.Resources!)!;
            var newInfo = newVersionInfo.GetChild<StringFileInfo>(StringFileInfo.StringFileInfoKey);
            Assert.Equal("This is a test application", newInfo.Tables[0][StringTable.FileDescriptionKey]);
            Assert.Equal(versionInfo.Lcid, newVersionInfo.Lcid);
        }
    }
}
