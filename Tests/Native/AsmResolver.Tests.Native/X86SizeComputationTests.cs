using AsmResolver.X86;
using Xunit;

namespace AsmResolver.Tests.Native
{
    public class X86SizeComputationTests
    {
        [Fact]
        public void Misc()
        {
            TestSizeComputation(Properties.Resources.Misc);
        }

        [Fact]
        public void RelativeOffsets()
        {
            TestSizeComputation(Properties.Resources.RelativeOffsets);
        }

        [Fact]
        public void Reg8_RegOrMem8()
        {
            TestSizeComputation(Properties.Resources.RegOrMem8_Reg8);
        }

        [Fact]
        public void RegOrMem8_Reg8()
        {
            TestSizeComputation(Properties.Resources.Reg8_RegOrMem8);
        }

        [Fact]
        public void Reg8_RegOrMem8_SIB()
        {
            TestSizeComputation(Properties.Resources.RegOrMem8_Reg8_sib);
        }

        [Fact]
        public void OpCodeRegisterToken()
        {
            TestSizeComputation(Properties.Resources.OpCodeRegisterToken);
        }

        private static void TestSizeComputation(byte[] opcodes)
        {
            var reader = new MemoryStreamReader(opcodes);
            var disassembler = new X86Disassembler(reader);
            
            while (reader.Position < reader.Length)
            {
                var instruction = disassembler.ReadNextInstruction();

                long expectedSize = reader.Position - instruction.Offset;
                long computeSize = instruction.ComputeSize();

                Assert.Equal(expectedSize, computeSize);
            }
        }

    }
}
