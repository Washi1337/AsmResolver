using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.TestCases.Fields;
using AsmResolver.PE.Builder;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.File;
using AsmResolver.Tests;
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

        [SkippableTheory]
        [InlineData(MachineType.I386)]
        [InlineData(MachineType.Amd64)]
        [InlineData(MachineType.Arm64)]
        public void HelloWorldRebuildNoChange(MachineType machineType)
        {
            XunitHelpers.SkipIfNotMachine(machineType);

            // Read image
            var image = PEImage.FromBytes(
                machineType switch
                {
                    MachineType.I386 => Properties.Resources.HelloWorld,
                    MachineType.Amd64 => Properties.Resources.HelloWorld_X64,
                    MachineType.Arm64 => Properties.Resources.HelloWorld_Arm64,
                    _ => throw new ArgumentOutOfRangeException(nameof(machineType))
                },
                TestReaderParameters
            );

            // Rebuild
            var builder = new ManagedPEFileBuilder();
            var peFile = builder.CreateFile(image);

            // Verify
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(peFile, "HelloWorld", "Hello World!\n");
        }

        [SkippableFact]
        public void HelloWorldX86ToX64()
        {
            XunitHelpers.SkipIfNotX64();

            // Read image
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);

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

        [SkippableFact]
        public void HelloWorldX64ToX86()
        {
            XunitHelpers.SkipIfNotX86OrX64();

            // Read image
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_X64, TestReaderParameters);

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
            var image = PEImage.FromBytes(Properties.Resources.FieldRvaTest, TestReaderParameters);

            using var stream = new MemoryStream();
            var file = new ManagedPEFileBuilder(EmptyErrorListener.Instance).CreateFile(image);
            file.Write(stream);

            var newImage = PEImage.FromBytes(stream.ToArray(), TestReaderParameters);
            var table = newImage.DotNetDirectory!.Metadata!
                .GetStream<TablesStream>()
                .GetTable<FieldRvaRow>();

            byte[] data = new byte[16];
            table[0].Data.CreateReader().ReadBytes(data, 0, data.Length);
            Assert.Equal(new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15}, data);
            Assert.Equal(0x12345678u, table[1].Data.Rva);
        }

        [Fact]
        public void RvaFieldAlignmentIsPreserved()
        {
            var image = PEImage.FromFile(typeof(RVAField).Assembly.Location, TestReaderParameters);

            var tablesStream = image.DotNetDirectory!.Metadata!.GetStream<TablesStream>();

            var fieldRvaTable = tablesStream.GetTable<FieldRvaRow>();
            var typeDefTable = tablesStream.GetTable<TypeDefinitionRow>();
            var stringHeap = image.DotNetDirectory.Metadata.GetStream<StringsStream>();

            var privateImplDetailsTypeRid = typeDefTable
                .Select((t, i) => (TypeDef: t, Rid: i))
                .Single(tuple => stringHeap.GetStringByIndex(tuple.TypeDef.Name)!.Equals("<PrivateImplementationDetails>"))
                .Rid;

            var fieldRange = tablesStream.GetFieldRange((uint)privateImplDetailsTypeRid + 1);

            var targetField = fieldRange.Single(f =>
                fieldRvaTable.TryGetRowByKey(1, f.Rid, out var row)
                && row.Data.CreateReader().ReadBytes(8 * 4).AsSpan().SequenceEqual(MemoryMarshal.AsBytes([1L, 2, 3, 4]))
            );

            _ = fieldRvaTable.TryGetRidByKey(1, targetField.Rid, out var targetRvaField);

            // ensure we start aligned
            Assert.Equal(0u, fieldRvaTable.GetByRid(targetRvaField).Data.Rva % 8);

            var stream = new MemoryStream();
            var file = new ManagedPEFileBuilder(EmptyErrorListener.Instance).CreateFile(image);
            file.Write(stream);

            var newImage = PEImage.FromBytes(stream.ToArray(), TestReaderParameters);

            var newFieldRvaRow = newImage.DotNetDirectory!.Metadata!.GetStream<TablesStream>().GetTable<FieldRvaRow>().GetByRid(targetRvaField);

            Assert.Equal(0u, newFieldRvaRow.Data.Rva % 8);
        }
    }
}
