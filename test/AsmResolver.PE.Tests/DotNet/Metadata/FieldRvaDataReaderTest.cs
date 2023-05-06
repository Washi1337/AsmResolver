using AsmResolver.DotNet.TestCases.Fields;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.Platforms;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata
{
    public class FieldRvaDataReaderTest
    {
        private static FieldRvaRow FindFieldRvaRow(TablesStream tablesStream, MetadataToken cctorToken, MetadataToken fieldToken)
        {
            var reader = tablesStream
                .GetTable<MethodDefinitionRow>(TableIndex.Method)
                .GetByRid(cctorToken.Rid)
                .Body.CreateReader();

            var body = CilRawMethodBody.FromReader(ThrowErrorListener.Instance, ref reader)!;
            var disassembler = new CilDisassembler(body.Code.CreateReader());

            var initialValueFieldToken = MetadataToken.Zero;

            var instructions = disassembler.ReadInstructions();
            for (int i = 0; i < instructions.Count; i++)
            {
                if (instructions[i].OpCode.Code == CilCode.Ldtoken
                    && instructions[i + 2].OpCode.Code == CilCode.Stsfld
                    && (MetadataToken) instructions[i + 2].Operand! == fieldToken)
                {
                    initialValueFieldToken = (MetadataToken) instructions[i].Operand!;
                    break;
                }
            }

            Assert.NotEqual(MetadataToken.Zero, initialValueFieldToken);
            Assert.True(tablesStream
                .GetTable<FieldRvaRow>(TableIndex.FieldRva)
                .TryGetRowByKey(1, initialValueFieldToken.Rid, out var fieldRvaRow));
            return fieldRvaRow;
        }

        [Fact]
        public void ReadByteArray()
        {
            // Open image.
            var image = PEImage.FromFile(typeof(InitialValues).Assembly.Location);
            var directory = image.DotNetDirectory!;

            // Get token of field.
            var cctorToken = (MetadataToken) typeof(InitialValues)
                .TypeInitializer
                !.MetadataToken;
            var fieldToken = (MetadataToken) typeof(InitialValues)
                .GetField(nameof(InitialValues.ByteArray))
                !.MetadataToken;

            // Find associated field rva row.
            var fieldRvaRow = FindFieldRvaRow(directory.Metadata!.GetStream<TablesStream>(), cctorToken, fieldToken);

            // Read the data.
            var dataReader = new FieldRvaDataReader();
            var segment = dataReader.ResolveFieldData(
                ThrowErrorListener.Instance,
                Platform.Get(image.MachineType),
                directory,
                fieldRvaRow) as IReadableSegment;

            Assert.NotNull(segment);
            Assert.Equal(InitialValues.ByteArray, segment.ToArray());
        }
    }
}
