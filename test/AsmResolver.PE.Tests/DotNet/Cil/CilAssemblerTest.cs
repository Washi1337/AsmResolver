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

        private sealed class MockOperandBuilder : ICilOperandBuilder
        {
            /// <inheritdoc />
            public int GetVariableIndex(object operand)
            {
                throw new System.NotImplementedException();
            }

            /// <inheritdoc />
            public int GetArgumentIndex(object operand)
            {
                throw new System.NotImplementedException();
            }

            /// <inheritdoc />
            public uint GetStringToken(object operand)
            {
                throw new System.NotImplementedException();
            }

            /// <inheritdoc />
            public MetadataToken GetMemberToken(object operand)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}