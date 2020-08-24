using System.Linq;
using AsmResolver.PE.Debug;
using Xunit;

namespace AsmResolver.PE.Tests.Debug
{
    public class DebugDataEntryTest
    {
        [Fact]
        public void ReadEntries()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll);
            Assert.Equal(new[]
            {
                DebugDataType.CodeView,
                DebugDataType.VcFeature
            }, image.DebugData.Select(d => d.Contents.Type));
        }
    }
}