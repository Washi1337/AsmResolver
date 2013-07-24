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
        BinaryReader reader;
        Win32Assembly assembly;

        static List<x86OpCode> opcodeList;

        static x86Disassembler()
        {
            LoadOpCodes();
        }

        static void LoadOpCodes()
        {
            opcodeList = new List<x86OpCode>();
            foreach (FieldInfo info in typeof(x86OpCodes).GetFields(BindingFlags.Public| BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (info.FieldType == typeof(x86OpCode))
                {
                    opcodeList.Add((x86OpCode)info.GetValue(null));
                }
            }
        }


        /// <summary>
        /// Creates a new instance of a disassembler, by using an assembly as input.
        /// </summary>
        /// <param name="assembly">The assembly to be disassembled.</param>
        public x86Disassembler(Win32Assembly assembly)
        {
            this.reader = assembly._peImage.Reader;
            this.assembly = assembly;
            this.IsDynamic = false;
        }

        /// <summary>
        /// Creates a new instance of a disassembler, by using a byte array as input.
        /// </summary>
        /// <param name="bytes">The bytes to be disassembled.</param>
        public x86Disassembler(byte[] bytes)
            :this(new MemoryStream(bytes))
        {
        }

        /// <summary>
        /// Creates a new instance of a disassembler, by using a stream as input.
        /// </summary>
        /// <param name="stream">The stream to be disassembled.</param>
        public x86Disassembler(Stream stream)
        {
            this.reader = new BinaryReader(stream);
            this.IsDynamic = true;
        }

        /// <summary>
        /// Gets or sets a value indicating the current byte offset of the disassembler.
        /// </summary>
        public uint CurrentOffset
        {
            get { return (uint)reader.BaseStream.Position; }
            set
            {
                reader.BaseStream.Position = value;
            }
        }
        
        /// <summary>
        /// Indicates the x86 disassembler is being created by a custom stream or byte array and not from an assembly image.
        /// </summary>
        public bool IsDynamic
        {
            get;
            private set;
        }

        /// <summary>
        /// Disassembles bytes to a collection of assembly instructions.
        /// </summary>
        /// <param name="rawStartOffset">The starting offset</param>
        /// <param name="length">The length. This value is overwritten when the last instruction's bounds are outside of the bounds.</param>
        /// <returns></returns>
        public InstructionCollection Disassemble(long rawStartOffset, long length)
        {
            reader.BaseStream.Position = rawStartOffset;
            InstructionCollection instructions = new InstructionCollection();
     
            long offset = rawStartOffset;
            long endOffset = rawStartOffset + length;
            while (reader.BaseStream.Position < endOffset)
            {
                x86Instruction instruction = DisassembleNextInstruction();
                instructions.Add(instruction);
                offset += instruction.Size;
                reader.BaseStream.Position = offset;
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
            x86Instruction newInstruction = new x86Instruction();
            newInstruction.Offset = Offset.FromFileOffset((uint)reader.BaseStream.Position, assembly) ;

            newInstruction.OpCode = RetrieveNextOpCode();
            ProcessVariableByteIndex(ref newInstruction.code);

            ProcessRegisterOperands(ref newInstruction);
            byte[] rawOperand = ReadRawOperand(newInstruction.OpCode);
            newInstruction.operandbytes = ASMGlobals.MergeBytes(newInstruction.operandbytes, rawOperand);
            ProcessOperandBytes(ref newInstruction);

            ProcessOverrideOperand(ref newInstruction);

            return newInstruction;
        }


        private x86OpCode RetrieveNextOpCode()
        {
            x86OpCode returnOpCode = x86OpCode.Create(x86OpCodes.Unknown);
            byte opcodeByte = reader.ReadByte();
            returnOpCode._opcodeBytes = new byte[] { opcodeByte };

            x86OpCode[] matchingOpcodes = MatchWithOpCodes(opcodeByte);

            // if there is one match, set the returning opcode to that match. If not, then it's an instruction
            // from an opcode group, and select it by checking the next byte.
            if (matchingOpcodes.Length == 1)
                returnOpCode = x86OpCode.Create(matchingOpcodes[0]);
            else if (matchingOpcodes.Length > 1)
            {
                x86OpCode selected = SelectOpCodeFromToken(matchingOpcodes, reader.ReadByte());
                if (selected != null)
                    returnOpCode = selected;
                if (selected == null || selected._variableByteIndex > -1)
                    reader.BaseStream.Seek(-1, SeekOrigin.Current);
            }
            return returnOpCode;
        }

        private x86OpCode[] MatchWithOpCodes(byte opcodeByte)
        {
            List<x86OpCode> matches = new List<x86OpCode>();
            foreach (x86OpCode opcode in opcodeList)
            {
                if (opcode._opcodeBytes != null && opcode._opcodeBytes[0] == opcodeByte)
                    matches.Add(opcode);
            }
            return matches.ToArray();
        }

        private x86OpCode SelectOpCodeFromToken(x86OpCode[] matches, byte token)
        {
            byte groupIndex = matches[0]._opcodeBytes[0];
            
            byte hByte = (byte)(token >> 4 << 4);
            byte lByte = (byte)(token - hByte);

            int index = (int)Math.Floor((double)(token % 0x40) / (double)8.0f);

            // 0xff group has got more than 8 instructions with some special tokens.
            if (groupIndex == 0xFF)
            {
                // instructions with lowerbyte of 5 or D are an exception.
                if (lByte == 0x5 || lByte == 0xD)
                    return matches.FirstOrDefault(m => m._opcodeBytes[1] == token);
                else
                    index *= 2;
            }

            if (index < 0 || index >= matches.Length)
                return null;

            return matches[index];
        }

        private void ProcessVariableByteIndex(ref x86OpCode opcode)
        {
            if (opcode._variableByteIndex >= 0)
            {
                opcode._opcodeBytes[opcode._variableByteIndex] = reader.ReadByte();
            }
        }

        private void ProcessRegisterOperands(ref x86Instruction instruction)
        {
            switch (instruction.OpCode.GetRegisterOperandType())
            {
                case x86OperandType.Multiple32Register:
                case x86OperandType.Multiple32Or8Register:
                case x86OperandType.Multiple16Register:
                case x86OperandType.RegisterLeaRegister:
                    DecodeDoubleRegisters(ref instruction, instruction.code._opcodeBytes[instruction.code._opcodeBytes.Length - 1]);
                    break;
                case x86OperandType.Register8:
                case x86OperandType.Register32:
                case x86OperandType.Register32Or8:
                    DecodeSingleRegister(ref instruction, instruction.code._opcodeBytes[instruction.code._opcodeBytes.Length - 1]);
                    break;
            }
        }

        private byte[] ReadRawOperand(x86OpCode opcode)
        {
            switch (opcode.GetNormalOperandType())
            {
                case x86OperandType.Byte:
                case x86OperandType.ShortInstructionAddress:
                    return new byte[] { reader.ReadByte() };

                case x86OperandType.Dword:
                case x86OperandType.InstructionAddress:
                    return reader.ReadBytes(sizeof(int));

                case x86OperandType.Fword:
                    return reader.ReadBytes(6);

                case x86OperandType.Word:
                    return reader.ReadBytes(sizeof(ushort));
                case x86OperandType.WordAndByte:
                    return reader.ReadBytes(sizeof(ushort) + sizeof(byte));

                case x86OperandType.Qword:
                    return reader.ReadBytes(sizeof(ulong));

                case x86OperandType.Multiple32Register:
                case x86OperandType.Multiple16Register:
                case x86OperandType.Multiple32Or8Register:
                case x86OperandType.Register32:
                case x86OperandType.RegisterLeaRegister:
                    break;
            }
            return null;
        }

        private void ProcessOperandBytes(ref x86Instruction instruction)
        {
            uint nextOffset = (uint)(instruction.Offset.FileOffset + instruction.Size);
            
            Operand operandValue = null;
            switch (instruction.OpCode.GetNormalOperandType())
            {
                case x86OperandType.Byte:
                    operandValue = new Operand(instruction.operandbytes[0]);
                    break;
                case x86OperandType.Word:
                    operandValue = new Operand(BitConverter.ToInt16(instruction.operandbytes, 0));
                    break;
                case x86OperandType.WordAndByte:
                    break; // TODO
                case x86OperandType.Dword:
                    operandValue = new Operand(BitConverter.ToUInt32(instruction.operandbytes, 0));
                    break;
                case x86OperandType.Fword:
                    break; // TODO
                case x86OperandType.Qword:
                    operandValue = new Operand(BitConverter.ToUInt64(instruction.operandbytes, 0));
                    break;
                case x86OperandType.InstructionAddress:
                    operandValue = CreateTargetOffset((uint)(nextOffset + BitConverter.ToInt32(instruction.operandbytes, 0)));
                    break;
                case x86OperandType.ShortInstructionAddress:
                    operandValue = CreateTargetOffset((uint)(nextOffset + ASMGlobals.ByteToSByte(instruction.operandbytes[0])));
                    break;
                case x86OperandType.Register32:
                    DecodeSingleRegister(ref instruction, instruction.code._opcodeBytes[instruction.code._opcodeBytes.Length - 1]);
                    break;
                case x86OperandType.Instruction:
                    // opcode is prefix.
                    x86Instruction nextInstruction = DisassembleNextInstruction();
                    operandValue = new Operand(nextInstruction);
                    instruction.operandbytes = ASMGlobals.MergeBytes(nextInstruction.code.OpCodeBytes, nextInstruction.operandbytes);
                    instruction.code._operandLength = nextInstruction.Size;
                    break;
                case x86OperandType.None:
                    if (instruction.code.IsBasedOn(x86OpCodes.Unknown))
                        operandValue = new Operand(instruction.code._opcodeBytes[0]);
                    break;
            }

            if (operandValue != null)
            {
                if (instruction.operand1 != null)
                    instruction.operand2 = operandValue;
                else
                    instruction.operand1 = operandValue;
            }
        }

        private void ProcessOverrideOperand(ref x86Instruction instruction)
        {
            x86OperandType operandType = instruction.OpCode.GetOverrideOperandType();
            if (operandType.HasFlag(x86OperandType.ForceDwordPointer))
            {

                if (instruction.operand2 != null)
                    if (instruction.operand2.Value is uint)
                        instruction.operand2 = CreatePtr((uint)instruction.operand2.Value, OperandType.DwordPointer);
                    else
                        instruction.operand2.ValueType = OperandType.DwordPointer;
                else
                    if (instruction.operand1.Value is uint)
                        instruction.operand1 = CreatePtr((uint)instruction.operand1.Value, OperandType.DwordPointer);
                    else
                        instruction.operand1.ValueType = OperandType.DwordPointer;
            }
            if (operandType.HasFlag(x86OperandType.OverrideOperandOrder))
            {
                Operand temp = instruction.operand1;
                instruction.operand1 = instruction.operand2;
                instruction.operand2 = temp;
            }
        }



        private Operand CreateTargetOffset(uint offset, OperandType offsetType = OperandType.Normal)
        {
            return new Operand(Offset.FromFileOffset(offset, assembly));
        }

        private Operand CreatePtr(uint offset, OperandType offsetType = OperandType.DwordPointer)
        {
            return new Operand(Offset.FromVa(offset, assembly), OperandType.DwordPointer);
        }

        private void DecodeSingleRegister(ref x86Instruction instruction, byte registersToken)
        {
            x86Instruction result = instruction;
            x86OpCode resultopcode = instruction.OpCode;
            resultopcode._opcodeBytes[instruction.OpCode._variableByteIndex] = registersToken;
            result.OpCode = resultopcode;

            bool isGroupOpCode = MatchWithOpCodes(instruction.OpCode._opcodeBytes[0]).Length > 1;

            int actualregister = registersToken % 8;

            OperandType registerValueType = OperandType.Normal;
            int addition = 0;
            if (registersToken < 0x40)
            {
                //normal dword pointer
                if (!isGroupOpCode && registersToken >= 0x8)
                {
                    ProcessInvalidInstruction(ref instruction);
                    return;
                }
                registerValueType = OperandType.DwordPointer;

            }
            else if (registersToken >= 0x40 && registersToken < 0x80)
            {
                //dword pointer + sbyte addition
                if (!isGroupOpCode && registersToken >= 0x48)
                {
                    ProcessInvalidInstruction(ref instruction);
                    return;
                }
                registerValueType = OperandType.DwordPointer;
                instruction.operandbytes = new byte[] { reader.ReadByte() };
                instruction.OpCode._operandLength++;
                addition = ASMGlobals.ByteToSByte(instruction.operandbytes[0]);
            }
            else if (registersToken >= 0x80 && registersToken < 0xC0)
            {
                //dword pointer + int addition
                if (!isGroupOpCode && registersToken >= 0x88)
                {
                    ProcessInvalidInstruction(ref instruction);
                    return;
                }
                registerValueType = OperandType.DwordPointer;
                instruction.operandbytes = reader.ReadBytes(4);
                instruction.OpCode._operandLength += 4;
                addition = BitConverter.ToInt32(instruction.operandbytes, 0);
            }
            else if (registersToken >= 0xC0 && registersToken <= 0xFF)
            {
                // normal register -> do nothing.
                if (!isGroupOpCode && registersToken >= 0xC8)
                {
                    ProcessInvalidInstruction(ref instruction);
                    return;
                }
            }
            else
            {
                // TODO: Invalid single register token.
                
            }
            if (instruction.OpCode._operandType.HasFlag(x86OperandType.Register8))
            {
                actualregister |= (byte)x86Register.Bit8Mask;
            }
            actualregister |= GetSingleRegisterMask(registersToken);
            instruction.operand1 = new Operand((x86Register)actualregister, registerValueType, addition);
        }

        private void DecodeDoubleRegisters(ref x86Instruction instruction, byte registersToken)
        {
            x86Instruction result = instruction;
            x86OpCode resultopcode = instruction.OpCode;
            if (resultopcode._variableByteIndex == -1)
                return;
            resultopcode._opcodeBytes[instruction.OpCode._variableByteIndex] = registersToken;
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
                instruction.operandbytes = reader.ReadBytes(1);
                instruction.code._operandLength++; 
                instruction.operand1 = new Operand(register1, isLEA ? OperandType.LeaRegister : OperandType.DwordPointer, ASMGlobals.ByteToSByte(instruction.operandbytes[0]));
                instruction.operand2 = new Operand(register2, OperandType.Normal);
            }
            // one register is a dword pointer with an int32 addition
            else if (registersToken >= 0x80 && registersToken <= 0xBF)
            {
                instruction.operandbytes = reader.ReadBytes(4);
                instruction.code._operandLength += 4;
                int addition = BitConverter.ToInt32(instruction.operandbytes, 0);
                instruction.operand1 = new Operand(register1, isLEA ? OperandType.LeaRegister : OperandType.DwordPointer, addition);
                instruction.operand2 = new Operand(register2, OperandType.Normal);
            }
            // normal multiple registers.
            else if (registersToken >= 0xC0 && registersToken <= 0xFF)
            {
                instruction.operand1 = new Operand(register1, isLEA ? OperandType.LeaRegister : OperandType.Normal);
                instruction.operand2 = new Operand(register2, OperandType.Normal);
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
                    GetDoubleRegisterMask(registerToken, out mask1, out mask2);
                    registerToken1 |= mask1;
                    registerToken2 |= mask2;
                    break;
                case x86OperandType.Multiple32Register:
                    // do nothing, normal registers are used.
                    break;

            }
            // notice inverted registers.
            register2 = (x86Register)registerToken1;
            register1 = (x86Register)registerToken2;
        }

        private byte GetSingleRegisterMask(byte registerToken)
        {
            if (registerToken >= 0xC0)
                return (byte)x86Register.Bit8Mask;
            return 0;
        }

        private void GetDoubleRegisterMask(byte registerToken, out byte registerMask1, out byte registerMask2)
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

        private void ProcessInvalidInstruction(ref x86Instruction instruction)
        {
            
            byte firstByte = instruction.OpCode.OpCodeBytes[0];
            instruction.OpCode = x86OpCode.Create(x86OpCodes.Unknown);
            instruction.OpCode._opcodeBytes = new byte[] { firstByte };
            CurrentOffset = instruction.Offset.FileOffset + 1;
        }

      
    }
}
