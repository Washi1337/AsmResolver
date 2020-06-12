using System.Diagnostics;
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
            
            foreach (var entry in actualInfo.Tables[0].Entries)
            {
                string expected = entry.Key switch
                {
                    StringTableEntry.CommentsKey => expectedInfo.Comments,
                    StringTableEntry.CompanyNameKey => expectedInfo.CompanyName,
                    StringTableEntry.FileDescriptionKey => expectedInfo.FileDescription,
                    StringTableEntry.FileVersionKey => expectedInfo.FileVersion,
                    StringTableEntry.InternalNameKey => expectedInfo.InternalName,
                    StringTableEntry.LegalCopyrightKey => expectedInfo.LegalCopyright,
                    StringTableEntry.LegalTrademarksKey => expectedInfo.LegalTrademarks,
                    StringTableEntry.OriginalFilenameKey => expectedInfo.OriginalFilename,
                    StringTableEntry.PrivateBuildKey => expectedInfo.PrivateBuild,
                    StringTableEntry.ProductNameKey => expectedInfo.ProductName,
                    StringTableEntry.ProductVersionKey => expectedInfo.ProductVersion,
                    StringTableEntry.SpecialBuildKey => expectedInfo.SpecialBuild,
                    _ => null,
                };

                if (expected is null)
                    continue;
                
                Assert.Equal(expected, entry.Value);
            }
        }

    }
}