using System.Text;
using AsmResolver.X86;
using Xunit;

namespace AsmResolver.Tests.Native
{
    public class X86DisassemblerTests
    {
        private static readonly FasmX86Formatter _formatter = new FasmX86Formatter();

        [Fact]
        public void Reg8_RegOrMem8()
        {
            TestDisassembler(Properties.Resources.Reg8_RegOrMem8, Properties.Resources.Reg8_RegOrMem8_source);
        }

        [Fact]
        public void RegOrMem8_Reg8()
        {
            TestDisassembler(Properties.Resources.RegOrMem8_Reg8, Properties.Resources.RegOrMem8_Reg8_source);
        }

        [Fact]
        public void RegOrMem8_Reg8_SIB()
        {
            TestDisassembler(Properties.Resources.RegOrMem8_Reg8_sib, Properties.Resources.RegOrMem8_Reg8_sib_source);
        }

        [Fact]
        public void Misc()
        {
            TestDisassembler(Properties.Resources.Misc, Properties.Resources.Misc_source);
        }

        [Fact]
        public void RelativeOffsets()
        {
            TestDisassembler(Properties.Resources.RelativeOffsets, Properties.Resources.RelativeOffsets_source);
        }

        [Fact]
        public void OpCodeRegisterToken()
        {
            TestDisassembler(Properties.Resources.OpCodeRegisterToken, Properties.Resources.OpCodeRegisterToken_source);
        }

        [Fact]
        public void ThreeOperands()
        {
            TestDisassembler(Properties.Resources.ThreeOperands, Properties.Resources.ThreeOperands_source);
        }

        private static void TestDisassembler(byte[] code, string source)
        {
            var reader = new MemoryStreamReader(code);
            var disassembler = new X86Disassembler(reader);

            var sourceLines = NormalizeSource(source).Split('\n');
            var currentLine = 0;

            while (reader.Position < reader.Length)
            {
                var instruction = disassembler.ReadNextInstruction();

                var formattedInstruction = _formatter.FormatInstruction(instruction);
                Assert.Equal(sourceLines[currentLine], formattedInstruction);
                currentLine++;
            }
        }

        private static string NormalizeSource(string source)
        {
            var builder = new StringBuilder(source);
            builder.Replace("\r\n", "\n").Replace("use32\n\n", "").Replace("\n\n", "\n");
            return builder.ToString();
        }

    }
}
