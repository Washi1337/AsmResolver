using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using TUP.AsmResolver.PE;
namespace TUP.AsmResolver.ASM
{
    /// <summary>
    /// Disassembler for Win32 Assemblies. Pre-Pre-Pre Alpha version :D. UNDER CONSTRUCTION
    /// </summary>
    public class x86Disassembler
    {
        PeImage image;

        static List<x86OpCode> opcodeList;

        static x86Disassembler()
        {
            LoadOpCodes();
        }

        static void LoadOpCodes()
        {
            opcodeList = new List<x86OpCode>();
            foreach (FieldInfo info in typeof(x86OpCodes).GetFields())
            {
                if (info.FieldType == typeof(x86OpCode))
                {
                    opcodeList.Add((x86OpCode)info.GetValue(null));
                }
            }
        }

        /// <summary>
        /// Creates a new instance of a disassembler.
        /// </summary>
        /// <param name="assembly">The assembly to be disassembled.</param>
        public x86Disassembler(Win32Assembly assembly)
        {
            this.image = assembly.peImage;
            
        }



        /// <summary>
        /// Gets or sets a value indicating the current byte offset of the disassembler.
        /// </summary>
        public uint CurrentOffset
        {
            get { return (uint)image.Position; }
            set
            { image.SetOffset(value); 
            }
        }
        /// <summary>
        /// Disassembles bytes to a collection of assembly instructions.
        /// </summary>
        /// <param name="rawStartOffset">The starting offset</param>
        /// <param name="length">The length. This value is overwritten when the last instruction's bounds are outside of the bounds.</param>
        /// <returns></returns>
        public InstructionCollection Disassemble(long rawStartOffset, long length)
        {
            image.SetOffset(rawStartOffset);
            InstructionCollection instructions = new InstructionCollection();

            image.reader.BaseStream.Position = rawStartOffset;
            long offset = rawStartOffset;
            while (image.stream.Position < rawStartOffset + length)
            {
                x86Instruction instruction = DisassembleNextInstruction();
                instructions.Add(instruction);
                offset += rawStartOffset + instruction.Size;
                image.SetOffset(offset);
                

            }

          //  reader.Dispose();
            return instructions;
        }
        /// <summary>
        /// Disassembles the next instruction at the current offset.
        /// </summary>
        /// <returns></returns>
        public x86Instruction DisassembleNextInstruction()
        {
            x86Instruction newInstruction = new x86Instruction(image.assembly);
            newInstruction.Offset = Offset.FromFileOffset((uint)image.stream.Position, image.assembly);

           // if (newInstruction.offset == 0x6d1f)
           //     System.Diagnostics.Debugger.Break();

            newInstruction.OpCode = RetrieveNextOpCode();
            ProcessVariableByteIndex(ref newInstruction.code);

            newInstruction.operandbytes = ReadRawOperand(newInstruction.OpCode);
            ProcessInstructionBytes(ref newInstruction);

            return newInstruction;
        }



        private x86OpCode RetrieveNextOpCode()
        {
            x86OpCode returnOpCode = x86OpCode.Create(x86OpCodes.Unknown); 
            byte opcodeByte = image.ReadByte();
            returnOpCode.opcodebytes = new byte[] { opcodeByte };

            x86OpCode[] matchingOpcodes = MatchWithOpCodes(opcodeByte);

            // if there is one match, set the returning opcode to that match. If not, then it's an instruction
            // from an opcode group, and select it by checking the next byte.
            if (matchingOpcodes.Length == 1)
                returnOpCode = x86OpCode.Create(matchingOpcodes[0]);
            else if (matchingOpcodes.Length > 1)
            {
                x86OpCode selected = SelectOpCodeFromToken(matchingOpcodes, image.ReadByte());
                if (selected != null)
                    returnOpCode = selected;
                if (selected == null || selected.variableByteIndex > -1)
                    image.stream.Seek(-1, SeekOrigin.Current);
            }
            return returnOpCode;
        }

        private x86OpCode[] MatchWithOpCodes(byte opcodeByte)
        {
            List<x86OpCode> matches = new List<x86OpCode>();
            foreach (x86OpCode opcode in opcodeList)
            {
                if (opcode.opcodebytes != null && opcode.opcodebytes[0] == opcodeByte)
                    matches.Add(opcode);
            }
            return matches.ToArray();
        }

        private x86OpCode SelectOpCodeFromToken(x86OpCode[] matches, byte token)
        {
            byte groupIndex = matches[0].opcodebytes[0];

            int index = token / 8;

            if (index >= 0x8)
                index = token - ((token / 8) * 8);

            // 0xff group has got more than 8 instructions with some special tokens.
            if (groupIndex == 0xFF)
            {
                // instructions with lowerbyte of 5 or D are an exception.
                byte lByte = (byte)(token - ((token >> 4) << 4));
                if (lByte == 0x5 || lByte == 0xD)
                    return matches.FirstOrDefault(m => m.opcodebytes[1] == token);
                else
                    index *= 2;
            }

            if (index >= matches.Length)
                return null;

            return matches[index];
        }

        private void ProcessVariableByteIndex(ref x86OpCode opcode)
        {
            if (opcode.variableByteIndex >= 0)
            {
                opcode.opcodebytes[opcode.variableByteIndex] = image.ReadByte();
            }
        }




        private byte[] ReadRawOperand(x86OpCode opcode)
        {
            switch (opcode.operandtype)
            {
                case x86OperandType.Byte:
                case x86OperandType.ShortInstructionAddress:
                    return new byte[] { image.ReadByte() };

                case x86OperandType.Dword:
                case x86OperandType.DwordPtr:
                case x86OperandType.InstructionAddress:
                case x86OperandType.RegisterAndDword:
                    return image.ReadBytes(sizeof(int)) ;

                case x86OperandType.Fword:
                case x86OperandType.FwordPtr:
                    return image.ReadBytes(6);

                case x86OperandType.Word:
                    return image.ReadBytes(sizeof(ushort));
                case x86OperandType.WordAndByte:
                    return image.ReadBytes(sizeof(ushort) + sizeof(byte));

                case x86OperandType.Qword:
                    return image.ReadBytes(sizeof(ulong));

                case x86OperandType.Multiple32Register:
                case x86OperandType.Multiple16Register:
                case x86OperandType.Multiple32Or8Register:
                case x86OperandType.Register:
                case x86OperandType.RegisterAndByte:
                case x86OperandType.RegisterLeaRegister:
                case x86OperandType.RegisterOffset:
                case x86OperandType.RegisterPointer:
                    break;
            }
            return null;
        }

        private void ProcessInstructionBytes(ref x86Instruction instruction)
        {
            uint nextOffset = (uint)(instruction.Offset.FileOffset + instruction.Size);
            switch (instruction.code.operandtype)
            {
                case x86OperandType.Byte:
                    instruction.operand1 = new Operand(instruction.operandbytes[0]);
                    break;
                case x86OperandType.DwordPtr:
                    instruction.operand1 = CreatePtr(BitConverter.ToUInt32(instruction.operandbytes, 0), OperandType.DwordPointer);
                    break;
                case x86OperandType.Dword:
                    instruction.operand1 = new Operand(BitConverter.ToUInt32(instruction.operandbytes, 0));
                    break;
                case x86OperandType.InstructionAddress:
                    instruction.operand1 = CreateTargetOffset((uint)(nextOffset + BitConverter.ToInt32(instruction.operandbytes, 0)));
                    break;
                case x86OperandType.ShortInstructionAddress:
                    instruction.operand1 = CreateTargetOffset(nextOffset + instruction.operandbytes[0]);
                    break;
                case x86OperandType.Multiple32Register:
                case x86OperandType.Multiple32Or8Register:
                case x86OperandType.Multiple16Register:
                case x86OperandType.RegisterLeaRegister:
                    DecodeDoubleRegisters(ref instruction, instruction.code.opcodebytes[instruction.code.opcodebytes.Length - 1]);
                    break;
                case x86OperandType.Qword:
                    instruction.operand1 = new Operand(BitConverter.ToUInt64(instruction.operandbytes, 0));
                    break;
                case x86OperandType.Register:
                    DecodeSingleRegister(ref instruction, instruction.code.opcodebytes[instruction.code.opcodebytes.Length - 1]);
                    break;
                case x86OperandType.RegisterAndDword:
                    DecodeSingleRegister(ref instruction, instruction.code.opcodebytes[instruction.code.opcodebytes.Length - 1]);
                    instruction.operand2 = new Operand(BitConverter.ToInt32(instruction.operandbytes, 0));
                    break;
                case x86OperandType.RegisterAndByte:
                case x86OperandType.RegisterOffset:
                case x86OperandType.RegisterPointer:
                case x86OperandType.Word:
                case x86OperandType.WordAndByte:

                case x86OperandType.Fword:
                case x86OperandType.FwordPtr:
                    break;
                case x86OperandType.Instruction:
                    // opcode is prefix.
                    x86Instruction nextInstruction = DisassembleNextInstruction();
                    instruction.operand1 = new Operand(nextInstruction);
                    instruction.operandbytes = ASMGlobals.MergeBytes(nextInstruction.code.OpCodeBytes, nextInstruction.operandbytes);
                    instruction.code.operandlength = nextInstruction.Size;
                    break;
                case x86OperandType.None:
                    if (instruction.code.IsBasedOn(x86OpCodes.Unknown))
                        instruction.operand1 = new Operand(instruction.code.opcodebytes[0]);
                    break;
            }
        }

        private Operand CreateTargetOffset(uint offset, OperandType offsetType = OperandType.Normal)
        {
            return new Operand(Offset.FromFileOffset(offset, image.assembly));
        }

        private Operand CreatePtr(uint offset, OperandType offsetType = OperandType.DwordPointer)
        {
            return new Operand(Offset.FromVa(offset, image.assembly), OperandType.DwordPointer);
        }

        private void DecodeSingleRegister(ref x86Instruction instruction, byte registerstoken)
        {
            x86Instruction result = instruction;
            x86OpCode resultopcode = instruction.OpCode;
            resultopcode.opcodebytes[instruction.OpCode.variableByteIndex] = registerstoken;
            result.OpCode = resultopcode;


            int multiplier = (int)Math.Floor(Convert.ToDouble(registerstoken / 0x8));
            int start = multiplier * 0x8;
            int actualregister = registerstoken - start;

            OperandType registerValueType = OperandType.Normal;
            int addition = 0;
            if (registerstoken < 0x8)
            {
                //normal dword pointer
                registerValueType = OperandType.DwordPointer;
            
            }
            else if (registerstoken >= 0x40 && registerstoken < 0x48)
            {
                //dword pointer + sbyte addition
                registerValueType = OperandType.DwordPointer;
                instruction.operandbytes = new byte[] { image.ReadByte() };
                instruction.OpCode.operandlength = 1;
                addition = ASMGlobals.ByteToSByte(instruction.operandbytes[0]);
            }
            else if (registerstoken >= 0x80 && registerstoken < 0x88)
            {
                //dword pointer + int addition
                registerValueType = OperandType.DwordPointer;
                instruction.operandbytes = image.ReadBytes(4);
                instruction.OpCode.operandlength = 4;
                addition = BitConverter.ToInt32(instruction.operandbytes, 0);
            }
            else if (registerstoken >= 0xC0 && registerstoken < 0xC8)
            {
                // normal register -> do nothing.
            }
            else
            {
                // TODO: Invalid single register token.
                
            }

            instruction.operand1 = new Operand((x86Register)actualregister, registerValueType, addition);
        }

        private void DecodeDoubleRegisters(ref x86Instruction instruction, byte registersToken)
        {
            x86Instruction result = instruction;
            x86OpCode resultopcode = instruction.OpCode;
            if (resultopcode.variableByteIndex == -1)
                return;
            resultopcode.opcodebytes[instruction.OpCode.variableByteIndex] = registersToken;
            result.OpCode = resultopcode;

            // lea instructions has got different notations.
            bool isLEA = instruction.OpCode.IsBasedOn(x86OpCodes.Lea);

            x86Register register1;
            x86Register register2;

            DecodeRegisterPair(instruction, registersToken, out register1, out register2);

            // one register is a dword pointer
            if (registersToken <= 0x3F)
            {
                instruction.operand1 = new Operand(register1, OperandType.DwordPointer);
                instruction.operand2 = new Operand(register2, OperandType.Normal);
            }
            // one register is a dword pointer with an sbyte addition
            else if (registersToken > 0x3F && registersToken < 0x7F)
            {
                instruction.operandbytes = image.ReadBytes(1);
                instruction.code.operandlength++;
                instruction.operand1 = new Operand(register1, OperandType.Normal);
                instruction.operand2 = new Operand(register2, isLEA ? OperandType.LeaRegister : OperandType.DwordPointer, ASMGlobals.ByteToSByte(instruction.operandbytes[0]));
            }
            // one register is a dword pointer with an int32 addition
            else if (registersToken >= 0x80 && registersToken <= 0xBF)
            {
                instruction.operandbytes = image.ReadBytes(4);
                instruction.code.operandlength += 4;
                int addition = BitConverter.ToInt32(instruction.operandbytes, 0);
                instruction.operand1 = new Operand(register1, OperandType.Normal);
                instruction.operand2 = new Operand(register2, isLEA ? OperandType.LeaRegister : OperandType.DwordPointer, addition);
            }
            // normal multiple registers.
            else if (registersToken >= 0xC0 && registersToken <= 0xFF)
            {
                instruction.operand1 = new Operand(register1, OperandType.Normal);
                instruction.operand2 = new Operand(register2, isLEA ? OperandType.LeaRegister : OperandType.Normal);
            }

            


        }

        private void DecodeRegisterPair(x86Instruction instruction, byte registerToken, out x86Register register1, out x86Register register2)
        {
            byte registerToken1 = (byte)(registerToken >> 4 << 4); // high bits.
            byte registerToken2 = (byte)(registerToken - registerToken1); // lower bits.

            while (registerToken1 >= 0x40)
                registerToken1 -= 0x40;
            registerToken1 /= 8;
            if (registerToken2 > 0x7)
            {
                registerToken2 -= 8;
                registerToken1++;
            }

            switch (instruction.OpCode.OperandType)
            {
                case x86OperandType.Multiple16Register:
                    // add bit16 mask
                    registerToken1 |= (byte)x86Register.Bit16Mask;
                    registerToken2 |= (byte)x86Register.Bit16Mask;
                    break;
                case x86OperandType.Multiple32Or8Register:
                    // Not sure if right or not 
                    byte mask1;
                    byte mask2;
                    GetRegisterMask(registerToken, out mask1, out mask2);
                    registerToken1 |= mask1;
                    registerToken2 |= mask2;
                    break;
                case x86OperandType.Multiple32Register:
                    // do nothing, normal registers are used.
                    break;

            }
            register1 = (x86Register)registerToken1;
            register2 = (x86Register)registerToken2;
        }

        private void GetRegisterMask(byte registerToken, out byte registerMask1, out byte registerMask2)
        {
            // Not sure if this is right.
            if (registerToken < 0xC0)
            {
                registerMask1 = 0;
                registerMask2 = (byte)x86Register.Bit8Mask;
            }
            else
            {
                registerMask1 = (byte)x86Register.Bit8Mask;
                registerMask2 = (byte)x86Register.Bit8Mask;
            }
        }

    }
}
