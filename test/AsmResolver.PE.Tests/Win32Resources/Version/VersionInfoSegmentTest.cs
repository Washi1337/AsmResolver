using System.Linq;
using AsmResolver.PE.Win32Resources;
using AsmResolver.PE.Win32Resources.Version;
using Xunit;

namespace AsmResolver.PE.Tests.Win32Resources.Version
{
    public class VersionInfoSegmentTest
    {
        private static VersionInfoSegment FindVersionResource(IPEImage image)
        {
            var directory = image.Resources.Entries
                .OfType<IResourceDirectory>()
                .First(d => d.Type == ResourceType.Version);

            var data = (IResourceData) ((IResourceDirectory) directory.Entries[0]).Entries[0];
            var versionInfo = VersionInfoSegment.FromReader(data.Contents.CreateReader());
            return versionInfo;
        }

        [Fact]
        public void ReadFixedVersion()
        {
            string path = typeof(PEImage).Assembly.Location;
            var versionInfo = FindVersionResource(PEImage.FromFile(path));

            var fixedVersionInfo = versionInfo.FixedVersionInfo;
            Assert.Equal(new System.Version(1,0,0,0), fixedVersionInfo.FileVersion);
            Assert.Equal(new System.Version(1,0,0,0), fixedVersionInfo.ProductVersion);
        }
    }
}