using System;

namespace AsmResolver.X86
{
    /// <summary>
    /// Provides a mechanism for assembling x86 instructions into bytes.
    /// </summary>
    public class X86Assembler
    {
        private readonly IBinaryStreamWriter _writer;

        public X86Assembler(IBinaryStreamWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            _writer = writer;
        }

        /// <summary>
        /// Writes an instruction to the output stream.
        /// </summary>
        /// <param name="instruction">The instruction to write.</param>
        public void Write(X86Instruction instruction)
        {
            var opcode = instruction.OpCode;
            WriteOpCode(opcode);

            var mnemonicIndex = Array.IndexOf(opcode.Mnemonics, instruction.Mnemonic);
            if (mnemonicIndex == -1)
                throw new ArgumentException("Instruction's mnemonic is not supported by its opcode.");

            if (opcode.HasRegisterToken)
            {
                var token = (byte)(ComputeRegisterTokenPart(opcode.OperandTypes1[mnemonicIndex],
                                        opcode.OperandSizes1[mnemonicIndex], instruction.Operand1) |
                                   ComputeRegisterTokenPart(opcode.OperandTypes2[mnemonicIndex],
                                       opcode.OperandSizes2[mnemonicIndex], instruction.Operand2));

                if (opcode.HasOpCodeModifier)
                {
                    token |= (byte)(mnemonicIndex << 3);
                }

                _writer.WriteByte(token);
            }

            if (instruction.Operand1 != null)
            {
                WriteOperand(opcode.OperandTypes1[mnemonicIndex],
                    opcode.OperandSizes1[mnemonicIndex], instruction.Operand1);
                if (instruction.Operand2 != null)
                {
                    WriteOperand(opcode.OperandTypes2[mnemonicIndex],
                        opcode.OperandSizes2[mnemonicIndex], instruction.Operand2);
                    
                    if (instruction.Operand3 != null)
                        WriteOperand(opcode.OperandType3, opcode.OperandSize3, instruction.Operand3);
                }
            }
        }

        private void WriteOperand(X86OperandType method, X86OperandSize size, X86Operand operand)
        {
            WriteOperandValue(method, size, operand);
            WriteOperandOffset(operand.OffsetType, operand.Offset);
        }

        private void WriteOperandValue(X86OperandType method, X86OperandSize size, X86Operand operand)
        {
            switch (method)
            {
                case X86OperandType.MemoryAddress:
                case X86OperandType.DirectAddress:
                case X86OperandType.ImmediateData:
                {
                    WriteNumber(operand.Value, size);
                    break;
                }
                case X86OperandType.RegisterOrMemoryAddress:
                {
                    if ((operand.ScaledIndex != null) ||
                        (operand.OperandUsage != X86OperandUsage.Normal && operand.Value is X86Register &&
                         (X86Register)operand.Value == X86Register.Esp))
                        _writer.WriteByte(ComputeRegOrMemSibToken(operand));
                    else if (!(operand.Value is X86Register))
                        WriteNumber(operand.Value, X86OperandSize.Dword);
                    break;
                }
                case X86OperandType.RelativeOffset:
                {
                    WriteNumber(Convert.ToUInt32(operand.Value) - (_writer.Position + (int) size), size);
                    break;
                }
            }
        }

        private void WriteOperandOffset(X86OffsetType type, int offset)
        {
            switch (type)
            {
                case X86OffsetType.None:
                    break;
                case X86OffsetType.Short:
                    _writer.WriteSByte((sbyte)offset);
                    break;
                case X86OffsetType.Long:
                    _writer.WriteInt32(offset);
                    break;
                default:
                    throw new NotSupportedException("Unrecognized or unsupported offset type.");
            }
        }

        private void WriteNumber(object value, X86OperandSize size)
        {
            switch (size)
            {
                case X86OperandSize.Byte:
                    if (value is sbyte || value is short || value is int || value is long)
                        _writer.WriteSByte(Convert.ToSByte(value));
                    else
                        _writer.WriteByte(Convert.ToByte(value));
                    break;
                case X86OperandSize.Word:
                    if (value is sbyte || value is short || value is int || value is long)
                        _writer.WriteInt16(Convert.ToInt16(value));
                    else
                        _writer.WriteUInt16(Convert.ToUInt16(value));
                    break;
                case X86OperandSize.WordOrDword:
                case X86OperandSize.Dword:
                    if (value is sbyte || value is short || value is int || value is long)
                        _writer.WriteInt32(Convert.ToInt32(value));
                    else
                        _writer.WriteUInt32(Convert.ToUInt32(value));
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void WriteOpCode(X86OpCode opCode)
        {
            _writer.WriteByte(opCode.Op1);
            // todo: multibyte opcodes.
        }
        
        private static byte ComputeRegisterTokenPart(X86OperandType method, X86OperandSize size, X86Operand operand)
        {
            switch (method)
            {
                case X86OperandType.Register:
                {
                    return (byte)(ComputeRegisterToken((X86Register)operand.Value) << 3);
                }
                case X86OperandType.RegisterOrMemoryAddress:
                {
                    return ComputeRegOrMemToken(operand);
                }
            }
            return 0;
        }

        private static byte ComputeRegisterToken(X86Register register)
        {
            return (byte)((byte)(register) & 7);
        }

        private static byte ComputeRegOrMemToken(X86Operand operand)
        {            
            // Mechanism:
           // http://ref.x86asm.net/coder32.html#modrm_byte_32

            // ModR/M byte:
            //  mod | reg/mem | (reg2)
            // -----+---------+-------
            //  7 6 |  5 4 3  | (2 1 0)

            var modifier = DetermineRegOrMemModifier(operand);
            var token = (byte)((byte)modifier << 6);

            if (operand.ScaledIndex != null)
                token |= ComputeRegisterToken(X86Register.Esp);
            else if (operand.Value is X86Register)
                token |= ComputeRegisterToken((X86Register)operand.Value);
            else
                return ComputeRegisterToken(X86Register.Ebp);

            return token;
        }

        private static X86RegOrMemModifier DetermineRegOrMemModifier(X86Operand operand)
        {
            if (operand.Value is X86Register)
            {
                switch (operand.OffsetType)
                {
                    case X86OffsetType.None:
                        return operand.OperandUsage == X86OperandUsage.Normal
                            ? X86RegOrMemModifier.RegisterOnly
                            : X86RegOrMemModifier.RegisterPointer;
                    case X86OffsetType.Short:
                        return X86RegOrMemModifier.RegisterDispShortPointer;
                    case X86OffsetType.Long:
                        return X86RegOrMemModifier.RegisterDispLongPointer;
                }
                throw new NotSupportedException("Unsupported or unrecognized operand offset type.");
            }

            if (operand.Value is uint)
                return X86RegOrMemModifier.RegisterPointer;

            throw new ArgumentException("Operand is not a valid RegOrMem operand.", "operand");
        }

        private static byte ComputeRegOrMemSibToken(X86Operand operand)
        {
            // Mechanism:
            // http://ref.x86asm.net/coder32.html#sib_byte_32

            // SIB-byte:
            //  mul | scaled_reg | reg
            // -----+------------+-------
            //  7 6 |   5 4 3    | 2 1 0

            var token = ComputeRegisterToken((X86Register)operand.Value);

            if (operand.ScaledIndex == null)
                token |= 0x20;
            else
            {
                token |= (byte)(ComputeRegisterToken(operand.ScaledIndex.Register) << 3);
                switch (operand.ScaledIndex.Multiplier)
                {
                    case 1:
                        break;
                    case 2:
                        token |= 0x40;
                        break;
                    case 4:
                        token |= 0x80;
                        break;
                    case 8:
                        token |= 0xC0;
                        break;
                    default:
                        throw new ArgumentException("Operand has an invalid scaled index multiplier.", "operand");
                }
            }

            return token;
        }

    }
}
