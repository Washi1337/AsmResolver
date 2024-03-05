using AsmResolver.PE.DotNet.Cil;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Cil
{
    public class CilInstructionTest
    {
        [Theory]
        [InlineData(CilCode.Ldc_I4, 1234, 1234)]
        [InlineData(CilCode.Ldc_I4_M1, null, -1)]
        [InlineData(CilCode.Ldc_I4_0, null, 0)]
        [InlineData(CilCode.Ldc_I4_1, null, 1)]
        [InlineData(CilCode.Ldc_I4_2, null, 2)]
        [InlineData(CilCode.Ldc_I4_3, null, 3)]
        [InlineData(CilCode.Ldc_I4_4, null, 4)]
        [InlineData(CilCode.Ldc_I4_5, null, 5)]
        [InlineData(CilCode.Ldc_I4_6, null, 6)]
        [InlineData(CilCode.Ldc_I4_7, null, 7)]
        [InlineData(CilCode.Ldc_I4_8, null, 8)]
        [InlineData(CilCode.Ldc_I4_S, (sbyte) -10, -10)]
        public void GetLdcI4ConstantTest(CilCode code, object? operand, int expectedValue)
        {
            var instruction = new CilInstruction(code.ToOpCode(), operand);
            Assert.Equal(expectedValue, instruction.GetLdcI4Constant());
        }

        [Fact]
        public void ReplaceWithOperand()
        {
            var instruction = new CilInstruction(CilOpCodes.Ldstr, "Hello, world!");

            Assert.NotNull(instruction.Operand);

            instruction.ReplaceWith(CilOpCodes.Ldc_I4, 1337);

            Assert.Equal(CilOpCodes.Ldc_I4, instruction.OpCode);
            Assert.Equal(1337, instruction.Operand);
        }

        [Fact]
        public void ReplaceWithoutOperandShouldRemoveOperand()
        {
            var instruction = new CilInstruction(CilOpCodes.Ldstr, "Hello, world!");

            Assert.NotNull(instruction.Operand);

            instruction.ReplaceWith(CilOpCodes.Ldnull);

            Assert.Equal(CilOpCodes.Ldnull, instruction.OpCode);
            Assert.Null(instruction.Operand);
        }

        [Fact]
        public void ReplaceWithNop()
        {
            var instruction = new CilInstruction(CilOpCodes.Ldstr, "Hello, world!");

            instruction.ReplaceWithNop();

            Assert.Equal(CilOpCodes.Nop, instruction.OpCode);
            Assert.Null(instruction.Operand);
        }
    }
}
