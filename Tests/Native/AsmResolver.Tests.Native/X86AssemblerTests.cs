using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.X86;
using Xunit;

namespace AsmResolver.Tests.Native
{
    public class X86AssemblerTests
    {

        [Fact]
        public void RegOrMem8_Reg8()
        {
            var body = CreateRegOrMemTestInstructions(X86OpCodes.Add_RegOrMem8_Reg8, X86Mnemonic.Add, false, false).ToArray();

            TestAssembler(body);
        }

        [Fact]
        public void Reg8_RegOrMem8()
        {
            var body = CreateRegOrMemTestInstructions(X86OpCodes.Add_Reg8_RegOrMem8, X86Mnemonic.Add, true, false).ToArray();

            TestAssembler(body);
        }

        [Fact]
        public void RegOrMem8_Reg8_SIB()
        { 
            var body = CreateRegOrMemSibTestInstructions(X86OpCodes.Add_RegOrMem8_Reg8, X86Mnemonic.Add).ToArray();

            TestAssembler(body);
        }

        [Fact]
        public void OpCodeRegisterToken()
        {
            var body = CreateOpCodeRegisterTokenTestInstructions().ToArray();

            TestAssembler(body);
        }

        [Fact]
        public void AssembleThreeOperands()
        {
            var body = Create3OperandsInstructions().ToArray();

            TestAssembler(body);
        }

        private static IEnumerable<X86Instruction> CreateRegOrMemTestInstructions(X86OpCode opcode, X86Mnemonic mnemonic, bool flippedOperands, bool threeOperands)
        {  
            for (int operandType = 0; operandType < 3; operandType++)
            {
                for (int register2Index = 0; register2Index < 8; register2Index++)
                {
                    for (int register1Index = 0; register1Index < 8; register1Index++)
                    {
                        var operand1 = new X86Operand(X86OperandUsage.BytePointer,
                            (X86Register)register1Index | X86Register.Eax);
                        var operand2 = new X86Operand(X86OperandUsage.Normal, (X86Register)register2Index);

                        var instruction = new X86Instruction()
                        {
                            OpCode = opcode,
                            Mnemonic = mnemonic,
                        };

                        if (flippedOperands)
                        {
                            instruction.Operand2 = operand1;
                            instruction.Operand1 = operand2;
                        }
                        else
                        {
                            instruction.Operand1 = operand1;
                            instruction.Operand2 = operand2;
                        }

                        switch (register1Index)
                        {
                            case 4: // esp
                                continue;
                            case 5: // ebp
                                if (operandType != 0)
                                    continue;
                                operand1.Value = 0x1337u;
                                break;
                        }

                        switch (operandType)
                        {
                            case 1:
                                operand1.Offset = 1;
                                operand1.OffsetType = X86OffsetType.Short;
                                break;
                            case 2:
                                operand1.Offset = 0x1337;
                                operand1.OffsetType = X86OffsetType.Long;
                                break;
                        }

                        if (threeOperands)
                        {
                            switch (opcode.OperandSize3)
                            {
                                case X86OperandSize.Byte:
                                    instruction.Operand3 = new X86Operand((byte) 0x12);
                                    break;
                                case X86OperandSize.WordOrDword:
                                    instruction.Operand3 = new X86Operand(0x1337u);
                                    break;
                            }
                        }
                        
                        yield return instruction;
                    }
                }
            }
        }

        private static IEnumerable<X86Instruction> CreateRegOrMemSibTestInstructions(X86OpCode opcode, X86Mnemonic mnemonic)
        {
            for (int operandType = 0; operandType < 3; operandType++)
            {
                for (int multiplier = 1; multiplier < 16; multiplier*=2)
                {
                    for (int scaledRegIndex = 0; scaledRegIndex < 8; scaledRegIndex++)
                    {
                        if (scaledRegIndex == 4)
                            continue;

                        var operand1 = new X86Operand(X86OperandUsage.BytePointer, X86Register.Eax,
                            new X86ScaledIndex((X86Register)scaledRegIndex | X86Register.Eax, multiplier));

                        var operand2 = new X86Operand(X86OperandUsage.Normal, X86Register.Al);

                        var instruction = new X86Instruction()
                        {
                            OpCode = opcode,
                            Mnemonic = mnemonic,
                            Operand1 = operand1,
                            Operand2 = operand2,
                        };

                        switch (operandType)
                        {
                            case 1:
                                operand1.Offset = 1;
                                operand1.OffsetType = X86OffsetType.Short;
                                break;
                            case 2:
                                operand1.Offset = 0x1337;
                                operand1.OffsetType = X86OffsetType.Long;
                                break;
                        }
                        
                        yield return instruction;
                    }
                }
            }
        }

        private static IEnumerable<X86Instruction> CreateOpCodeRegisterTokenTestInstructions()
        {
            var opcode = X86OpCodes.Arithmetic_RegOrMem8_Imm8;
            for (int index = 0; index < opcode.Mnemonics.Length; index++)
            {
                var mnemonic = opcode.Mnemonics[index];

                var instruction = new X86Instruction()
                {
                    OpCode = opcode,
                    Mnemonic = mnemonic,
                    Operand1 = new X86Operand(X86OperandUsage.BytePointer, 0x1337u),
                    Operand2 = new X86Operand((byte)index),
                };

                yield return instruction;
            }
        }

        private static IEnumerable<X86Instruction> Create3OperandsInstructions()
        {
            for (int reg1 = (int) X86Register.Eax; reg1 <= (int) X86Register.Edi; reg1++)
            {
                for (int reg2 = (int) X86Register.Eax; reg2 <= (int) X86Register.Edi; reg2++)
                {
                    var operand1 = new X86Operand((X86Register) reg1);
                    var operand2 = new X86Operand(X86OperandUsage.DwordPointer, (X86Register) reg2);
                    if ((X86Register) operand2.Value == X86Register.Ebp)
                        operand2.Value = 0x1337u;
                    
                    yield return new X86Instruction
                    {
                        OpCode = X86OpCodes.IMul_Reg1632_RegOrMem1632_Imm1632,
                        Mnemonic = X86Mnemonic.Imul,
                        Operand1 = operand1,
                        Operand2 = operand2,
                        Operand3 = new X86Operand(1337u)
                    };
                }
            }
        }

        private static void TestAssembler(IReadOnlyList<X86Instruction> instructions)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryStreamWriter(stream);
                var assembler = new X86Assembler(writer);

                foreach (var instruction in instructions)
                    assembler.Write(instruction);
                
                ValidateCode(instructions, stream.ToArray());
            }
        }
        
        private static void ValidateCode(IReadOnlyList<X86Instruction> originalBody, byte[] assemblerOutput)
        {
            var formatter = new FasmX86Formatter();
            var reader = new MemoryStreamReader(assemblerOutput);
            var disassembler = new X86Disassembler(reader);

            for (int i = 0; i < originalBody.Count; i++)
            {
                var newInstruction = disassembler.ReadNextInstruction();
                Assert.Equal(formatter.FormatInstruction(originalBody[i]),
                    formatter.FormatInstruction(newInstruction));
            }
        }

        
    }
}
