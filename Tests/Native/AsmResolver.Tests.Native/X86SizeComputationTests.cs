using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.X86;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsmResolver.Tests.Native
{
    [TestClass]
    public class X86SizeComputationTests
    {
        [TestMethod]
        public void Misc()
        {
            TestSizeComputation(Properties.Resources.Misc);
        }

        [TestMethod]
        public void RelativeOffsets()
        {
            TestSizeComputation(Properties.Resources.RelativeOffsets);
        }

        [TestMethod]
        public void Reg8_RegOrMem8()
        {
            TestSizeComputation(Properties.Resources.RegOrMem8_Reg8);
        }

        [TestMethod]
        public void RegOrMem8_Reg8()
        {
            TestSizeComputation(Properties.Resources.Reg8_RegOrMem8);
        }

        [TestMethod]
        public void Reg8_RegOrMem8_SIB()
        {
            TestSizeComputation(Properties.Resources.RegOrMem8_Reg8_sib);
        }

        [TestMethod]
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

                var expectedSize = reader.Position - instruction.Offset;
                var computeSize = instruction.ComputeSize();

                if (expectedSize != computeSize)
                    Assert.Fail("The instruction {0} has size {1}, but {2} was computed.", instruction, expectedSize,
                        computeSize);
            }
        }

    }
}
