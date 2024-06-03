using System.IO;
using AsmResolver.PE.Builder;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.File;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.PE.Tests.Builder
{
    public class ManagedPEFileBuilderTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly TemporaryDirectoryFixture _fixture;

        public ManagedPEFileBuilderTest(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void HelloWorldRebuild32BitNoChange()
        {
            // Read image
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);

            // Rebuild
            var builder = new ManagedPEFileBuilder();
            var peFile = builder.CreateFile(image);

            // Verify
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(peFile, "HelloWorld", "Hello World!\n");
        }

        [Fact]
        public void HelloWorldRebuild64BitNoChange()
        {
            // Read image
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_X64);

            // Rebuild
            var builder = new ManagedPEFileBuilder();
            var peFile = builder.CreateFile(image);

            // Verify
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(peFile, "HelloWorld", "Hello World!\n");
        }

        [Fact]
        public void HelloWorld32BitTo64Bit()
        {
            // Read image
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);

            // Change machine type and pe kind to 64-bit
            image.MachineType = MachineType.Amd64;
            image.PEKind = OptionalHeaderMagic.PE32Plus;

            // Rebuild
            var builder = new ManagedPEFileBuilder();
            var peFile = builder.CreateFile(image);

            // Verify
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(peFile, "HelloWorld", "Hello World!\n");
        }

        [Fact]
        public void HelloWorld64BitTo32Bit()
        {
            // Read image
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_X64);

            // Change machine type and pe kind to 32-bit
            image.MachineType = MachineType.I386;
            image.PEKind = OptionalHeaderMagic.PE32;

            // Rebuild
            var builder = new ManagedPEFileBuilder();
            var peFile = builder.CreateFile(image);

            // Verify
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(peFile, "HelloWorld", "Hello World!\n");
        }

        [Fact]
        public void UpdateFieldRvaRowsUnchanged()
        {
            var image = PEImage.FromBytes(Properties.Resources.FieldRvaTest);

            using var stream = new MemoryStream();
            var file = new ManagedPEFileBuilder(EmptyErrorListener.Instance).CreateFile(image);
            file.Write(stream);

            var newImage = PEImage.FromBytes(stream.ToArray());
            var table = newImage.DotNetDirectory!.Metadata!
                .GetStream<TablesStream>()
                .GetTable<FieldRvaRow>();

            byte[] data = new byte[16];
            table[0].Data.CreateReader().ReadBytes(data, 0, data.Length);
            Assert.Equal(new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15}, data);
            Assert.Equal(0x12345678u, table[1].Data.Rva);
        }

    }
}
