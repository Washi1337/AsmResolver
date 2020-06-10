using System.Linq;
using Xunit;

namespace AsmResolver.PE.Tests.Win32Resources.Version
{
    public class VersionInfoSegmentTest
    {
        [Fact]
        public void Test()
        {
            string path = typeof(PEImage).Assembly.Location;
            var image = PEImage.FromFile(path);
            image.Resources.Entries.First()
        }
    }
}