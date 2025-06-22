using AsmResolver.PE.Exceptions;
using Xunit;

namespace AsmResolver.PE.Tests.Exceptions
{
    public class X64ExceptionDirectoryTest
    {
        [Fact]
        public void ReadSEHTable()
        {
            var image = PEImage.FromBytes(Properties.Resources.SEHSamples, TestReaderParameters);
            var exceptions = Assert.IsAssignableFrom<ExceptionDirectory<X64RuntimeFunction>>(image.Exceptions);

            Assert.Equal(0x1010u, exceptions.Functions[0].Begin.Rva);
            Assert.Equal(0x1065u, exceptions.Functions[0].End.Rva);
            Assert.Equal(0x1E0Cu, exceptions.Functions[^1].Begin.Rva);
            Assert.Equal(0x1E24u, exceptions.Functions[^1].End.Rva);
        }
    }
}
