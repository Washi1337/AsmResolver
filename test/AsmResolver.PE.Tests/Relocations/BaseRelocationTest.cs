using AsmResolver.PE.Relocations;
using Xunit;

namespace AsmResolver.PE.Tests.Relocations
{
    public class BaseRelocationTest
    {
        [Fact]
        public void DotNetHelloWorld()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            
            Assert.Equal(new[]
            {
                new BaseRelocation(RelocationType.HighLow, new VirtualAddress(0x2690)),
                new BaseRelocation(RelocationType.Absolute, new VirtualAddress(0x2000)),
            }, peImage.Relocations);
        }
    }
}