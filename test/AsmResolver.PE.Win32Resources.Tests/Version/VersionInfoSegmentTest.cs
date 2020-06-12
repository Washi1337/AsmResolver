using System.Diagnostics;
using System.IO;
using System.Linq;
using AsmResolver.PE.Win32Resources.Version;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.Version
{
    public class VersionInfoSegmentTest
    {
        private static VersionInfoSegment FindVersionInfo(IPEImage image)
        {
            var directory = image.Resources.Entries
                .OfType<IResourceDirectory>()
                .First(d => d.Type == ResourceType.Version);

            var data = (IResourceData) ((IResourceDirectory) directory.Entries[0]).Entries[0];
            return VersionInfoSegment.FromReader(data.Contents.CreateReader());
        }

        [Fact]
        public void ReadFixedVersion()
        {
            var versionInfo = FindVersionInfo(PEImage.FromBytes(Properties.Resources.HelloWorld));
            var fixedVersionInfo = versionInfo.FixedVersionInfo;
            
            Assert.Equal(new System.Version(1,0,0,0), fixedVersionInfo.FileVersion);
            Assert.Equal(new System.Version(1,0,0,0), fixedVersionInfo.ProductVersion);
        }

        [Fact]
        public void ReadStringFileInfo()
        {
            string path = typeof(PEImage).Assembly.Location;
            var versionInfo = FindVersionInfo(PEImage.FromFile(path)); 
                
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
        public void PersistentStringFileInfo()
        {
            // Prepare mock data.
            var versionInfo = new VersionInfoSegment();
            
            var stringFileInfo = new StringFileInfo();
            var table = new StringTable(0, 0x4b0);
            table[StringTable.ProductNameKey] = "Sample product";
            table[StringTable.FileVersionKey] = "1.2.3.4";
            table[StringTable.ProductVersionKey] = "1.0.0.0";
            table[StringTable.FileDescriptionKey] = "This is a sample description";
            stringFileInfo.Tables.Add(table);

            versionInfo[StringFileInfo.StringFileInfoKey] = stringFileInfo;

            // Serialize.
            var tempStream = new MemoryStream();
            versionInfo.Write(new BinaryStreamWriter(tempStream));
            
            // Reload.
            var newVersionInfo = VersionInfoSegment.FromReader(new ByteArrayReader(tempStream.ToArray()));
            
            // Verify.
            var newStringFileInfo = newVersionInfo.GetChild<StringFileInfo>(StringFileInfo.StringFileInfoKey);
            Assert.NotNull(newStringFileInfo);
            Assert.Single(newStringFileInfo.Tables);
            
            var newTable = newStringFileInfo.Tables[0];
            foreach (var entry in table)
                Assert.Equal(entry.Value, newTable[entry.Key]);
        }

    }
}