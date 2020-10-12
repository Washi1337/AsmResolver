using System;
using System.IO;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Cil
{
    public class CilAssemblerTest
    {
        [Theory]
        [InlineData(CilCode.Ldc_I4, 1234L)]
        [InlineData(CilCode.Ldc_I4_S, 1234)]
        [InlineData(CilCode.Ldc_I8, 1234)]
        public void InvalidPrimitiveOperandShouldThrow(CilCode code, object operand)
        {
            var stream = new MemoryStream();
            var writer = new BinaryStreamWriter(stream);
            var assembler = new CilAssembler(writer, new MockOperandBuilder());

            Assert.ThrowsAny<ArgumentException>(() =>
                assembler.WriteInstruction(new CilInstruction(code.ToOpCode(), operand)));
        }

        [Fact]
        public void BranchTooFarAwayShouldThrow()
        {
            var stream = new MemoryStream();
            var writer = new BinaryStreamWriter(stream);
            var assembler = new CilAssembler(writer, new MockOperandBuilder());

            Assert.ThrowsAny<OverflowException>(() =>
                assembler.WriteInstruction(new CilInstruction(CilOpCodes.Br_S, new CilOffsetLabel(0x12345))));
            assembler.WriteInstruction(new CilInstruction(CilOpCodes.Br, new CilOffsetLabel(0x12345)));
        }

        [Fact]
        public void TooLargeLocalShouldThrow()
        {
            var stream = new MemoryStream();
            var writer = new BinaryStreamWriter(stream);
            var assembler = new CilAssembler(writer, new MockOperandBuilder());

            Assert.ThrowsAny<OverflowException>(() =>
                assembler.WriteInstruction(new CilInstruction(CilOpCodes.Ldloc_S, 0x12345)));
            assembler.WriteInstruction(new CilInstruction(CilOpCodes.Ldloc, 0x12345));
        }

        private sealed class MockOperandBuilder : ICilOperandBuilder
        {
            /// <inheritdoc />
            public int GetVariableIndex(object operand) => Convert.ToInt32(operand);

            /// <inheritdoc />
            public int GetArgumentIndex(object operand) => Convert.ToInt32(operand);

            /// <inheritdoc />
            public uint GetStringToken(object operand) => Convert.ToUInt32(operand);

            /// <inheritdoc />
            public MetadataToken GetMemberToken(object operand) => (MetadataToken) operand;
        }
    }
}