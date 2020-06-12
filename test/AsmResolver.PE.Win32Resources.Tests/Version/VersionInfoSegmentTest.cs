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

    }
}