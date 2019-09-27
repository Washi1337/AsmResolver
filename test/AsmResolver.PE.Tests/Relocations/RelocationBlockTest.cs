using AsmResolver.PE.Relocations;
using Xunit;

namespace AsmResolver.PE.Tests.Relocations
{
    public class RelocationBlockTest
    {
        [Fact]
        public void DotNetHelloWorld()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            
            Assert.Equal(1, peImage.Relocations.Count);
           
            var blockBase = peImage.Relocations[0];
            Assert.Equal(0x2000u, blockBase.PageRva);
            Assert.Equal(2, blockBase.Entries.Count);
            Assert.Equal(RelocationType.HighLow, blockBase.Entries[0].RelocationType);
            Assert.Equal(RelocationType.Absolute, blockBase.Entries[1].RelocationType);
        }
    }
}