using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsmResolver.PE.Win32Resources;
using AsmResolver.PE.Win32Resources.Version;
using Xunit;

namespace AsmResolver.PE.Tests.Win32Resources.Version
{
    public class VersionInfoSegmentTest
    {
        private static readonly VersionInfoSegment _versionInfo;
        
        static VersionInfoSegmentTest()
        {
            string path = typeof(PEImage).Assembly.Location;
            var directory = PEImage.FromFile(path)
                .Resources.Entries
                .OfType<IResourceDirectory>()
                .First(d => d.Type == ResourceType.Version);

            var data = (IResourceData) ((IResourceDirectory) directory.Entries[0]).Entries[0];
            _versionInfo = VersionInfoSegment.FromReader(data.Contents.CreateReader());
        }
        
        [Fact]
        public void ReadFixedVersion()
        {
            var fixedVersionInfo = _versionInfo.FixedVersionInfo;
            Assert.Equal(new System.Version(1,0,0,0), fixedVersionInfo.FileVersion);
            Assert.Equal(new System.Version(1,0,0,0), fixedVersionInfo.ProductVersion);
        }

        [Fact]
        public void ReadStringFileInfo()
        {
            var expectedInfo = FileVersionInfo.GetVersionInfo(typeof(PEImage).Assembly.Location);
            var readInfo = _versionInfo.GetChild<StringFileInfo>(StringFileInfo.StringFileInfoKey);
            int count = 0;
            
            foreach (var entry in readInfo.Tables[0].Entries)
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
                
                count++;
                Assert.Equal(expected, entry.Value);
            }
            
            // Check if all attributes were found.
            Assert.Equal(12, count);
        }

    }
}