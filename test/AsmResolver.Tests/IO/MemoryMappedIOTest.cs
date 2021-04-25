using System.IO;
using AsmResolver.IO;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.Tests.IO
{
    public class MemoryMappedIOTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly TemporaryDirectoryFixture _fixture;

        public MemoryMappedIOTest(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void ReadShouldReturnFileContents()
        {
            byte[] contents = {1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
            string tempPath = Path.Combine(_fixture.BasePath, "MemoryMappedIOTest_ReadShouldReturnFileContents.bin");
            File.WriteAllBytes(tempPath, contents);

            using var factory = new MemoryMappedReaderFactory(tempPath);
            var reader = factory.CreateReader();
            Assert.Equal(contents, reader.ReadToEnd());
        }

        [Fact]
        public void CreateReaderPastStreamShouldThrow()
        {
            byte[] contents = {1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
            string tempPath = Path.Combine(_fixture.BasePath, "MemoryMappedIOTest_ReadPastStreamShouldThrow.bin");
            File.WriteAllBytes(tempPath, contents);

            using var factory = new MemoryMappedReaderFactory(tempPath);
            Assert.Throws<EndOfStreamException>(() => factory.CreateReader(0, 0, (uint) (contents.Length + 1)));
        }
    }
}
