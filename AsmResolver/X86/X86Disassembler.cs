using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.X86
{
    /// <summary>
    /// Provides a mechanism for disassembling bytes to x86 instructions.
    /// </summary>
    public class X86Disassembler
    {
        private readonly IBinaryStreamReader _reader;

        public X86Disassembler(IBinaryStreamReader reader)
            : this(reader, 0)
        {
        }

        public X86Disassembler(IBinaryStreamReader reader, long baseAddress)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            _reader = reader;
            BaseAddress = baseAddress;
        }

        /// <summary>
        /// Gets the base address.
        /// </summary>
        public long BaseAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// Disassembles the next instruction of the input stream.
        /// </summary>
        /// <returns>The disassembled instruction.</returns>
        public X86Instruction ReadNextInstruction()
        {
            long offset = BaseAddress + _reader.Position;
            var instruction = new X86Instruction(offset);

            List<byte> prefixBytes;
            byte nextCode = ReadNextOpCode(out prefixBytes);

            instruction.OpCode = InterpretOpCodeByte(prefixBytes, nextCode);
            if (instruction.OpCode.Mnemonics == null)
            {
                if (prefixBytes.Count > 0)
                {
                    nextCode = prefixBytes[0];
                    _reader.Position = offset + 1;
                }
                instruction.Operand1 = new X86Operand(nextCode);
                return instruction;
            }

            var registerToken = instruction.OpCode.HasRegisterToken ? _reader.ReadByte() : (byte)0;
            var mnemonicIndex = instruction.OpCode.HasOpCodeModifier ? (registerToken >> 3) & 7 : 0;
            instruction.Mnemonic = instruction.OpCode.Mnemonics[mnemonicIndex];

            foreach (var prefixByte in prefixBytes)
            {
                var prefix = X86Prefixes.PrefixesByByte[prefixByte]
                    .FirstOrDefault(x => x.CanPrecedeOpCode(instruction.OpCode));

                if (Equals(prefix, default(X86Prefix)))
                {
                    instruction.Prefixes.Clear();
                    instruction.OpCode = default(X86OpCode);
                    instruction.Operand1 = new X86Operand(prefixBytes[0]);
                    _reader.Position = offset + 1;
                    return instruction;
                }

                instruction.Prefixes.Add(prefix);
            }
            
            instruction.Operand1 = ReadOperand(instruction.Prefixes, instruction.OpCode.OperandTypes1[mnemonicIndex],
                instruction.OpCode.OperandSizes1[mnemonicIndex], instruction.OpCode.Op1, registerToken);

            instruction.Operand2 = ReadOperand(instruction.Prefixes, instruction.OpCode.OperandTypes2[mnemonicIndex],
                instruction.OpCode.OperandSizes2[mnemonicIndex], instruction.OpCode.Op1, registerToken);

            instruction.Operand3 = ReadOperand(instruction.Prefixes, instruction.OpCode.OperandType3,
                instruction.OpCode.OperandSize3, instruction.OpCode.Op1, registerToken);

            return instruction;
        }

        private byte ReadNextOpCode(out List<byte> prefixBytes)
        {
            prefixBytes = new List<byte>(4);

            byte nextCode = 0;
            for (int i = 0; i < 4; i++)
            {
                nextCode = _reader.ReadByte();

                IList<X86Prefix> prefixes;
                if (!X86Prefixes.PrefixesByByte.TryGetValue(nextCode, out prefixes))
                    break;
                prefixBytes.Add(nextCode);
            }
            return nextCode;
        }

        private X86OpCode InterpretOpCodeByte(IList<byte> prefixBytes, byte code1)
        {
            // TODO: use prefix.
            
            switch (code1)
            {
                case 0x0F:
                    return X86OpCodes.MultiByteOpCodes[_reader.ReadByte()];

                default:
                    return X86OpCodes.SingleByteOpCodes[code1];
            }
        }

        private X86Operand ReadOperand(ICollection<X86Prefix> prefixes, X86OperandType method, X86OperandSize size, byte opcode, byte registerToken)
        {
            switch (method)
            {
                case X86OperandType.OpCodeRegister:
                    return new X86Operand(GetRegisterFromToken((byte)(opcode & 7), GetRegisterSize(prefixes, size)));

                case X86OperandType.Register:
                    return new X86Operand(GetRegisterFromToken((byte)((registerToken >> 3) & 7),
                        GetRegisterSize(prefixes, size)));

                case X86OperandType.RegisterOrMemoryAddress:
                    return GetRegOrMemOperand32(prefixes, registerToken, size);

                case X86OperandType.ImmediateData:
                    return new X86Operand(ReadImmediateData(prefixes, size));

                case X86OperandType.MemoryAddress:
                    return new X86Operand(GetOperandType(prefixes, size), _reader.ReadUInt32());

                case X86OperandType.RegisterAl:
                    return new X86Operand(X86Register.Al);

                case X86OperandType.RegisterCl:
                    return new X86Operand(X86Register.Cl);

                case X86OperandType.RegisterDx:
                    return new X86Operand(X86Register.Dx);

                case X86OperandType.RegisterEax:
                    return new X86Operand(X86Register.Eax);
                    
                case X86OperandType.ImmediateOne:
                    return new X86Operand(1);

                case X86OperandType.RelativeOffset:
                    return new X86Operand((ulong)(Convert.ToInt64(ReadSignedImmediateData(size)) + BaseAddress + _reader.Position));

                case X86OperandType.None:
                    return null;

            }
            throw new NotSupportedException("Unrecognized or unsupported addressing method.");
        }

        private object ReadSignedImmediateData(X86OperandSize size)
        {
            switch (size)
            {
                case X86OperandSize.Byte:
                    return _reader.ReadSByte();
                case X86OperandSize.Word:
                    return _reader.ReadInt16();
                case X86OperandSize.Dword:
                    return _reader.ReadInt32();
                case X86OperandSize.WordOrDword:
                    return _reader.ReadInt32(); // TODO: use operand-size override opcode
                // TODO: fword
            }
            throw new NotSupportedException();
        }

        private object ReadImmediateData(ICollection<X86Prefix> prefixes, X86OperandSize size)
        {
            switch (size)
            {
                case X86OperandSize.Byte:
                    return _reader.ReadByte();
                case X86OperandSize.Word:
                    return _reader.ReadUInt16();
                case X86OperandSize.Dword:
                    return _reader.ReadUInt32();
                case X86OperandSize.WordOrDword:
                    return prefixes.Contains(X86Prefixes.OperandSizeOverride) 
                        ? _reader.ReadUInt16()
                        : _reader.ReadUInt32();
                    // TODO: fword
            }
            throw new NotSupportedException();
        }

        private static X86RegisterSize GetRegisterSize(ICollection<X86Prefix> prefixes, X86OperandSize size)
        {
            switch (size)
            {
                case X86OperandSize.Byte:
                    return X86RegisterSize.Byte;
                case X86OperandSize.Word:
                    return X86RegisterSize.Word;
                case X86OperandSize.Dword:
                    return X86RegisterSize.Dword;
                case X86OperandSize.WordOrDword:
                    return prefixes.Contains(X86Prefixes.OperandSizeOverride)
                        ? X86RegisterSize.Word
                        : X86RegisterSize.Dword;
            }
            throw new ArgumentException();
        }

        private static X86OperandUsage GetOperandType(ICollection<X86Prefix> prefixes, X86OperandSize size)
        {
            switch (size)
            {
                case X86OperandSize.Byte:
                    return X86OperandUsage.BytePointer;
                case X86OperandSize.Dword:
                    return X86OperandUsage.DwordPointer;
                case X86OperandSize.WordOrDword:
                    return prefixes.Contains(X86Prefixes.OperandSizeOverride)
                        ? X86OperandUsage.WordPointer
                        : X86OperandUsage.DwordPointer;
                case X86OperandSize.Fword:
                    return X86OperandUsage.FwordPointer;
            }
            throw new ArgumentException();
        }

        private X86Operand GetRegOrMemOperand32(ICollection<X86Prefix> prefixes, byte registerToken, X86OperandSize size)
        {
            // Mechanism:
            // http://ref.x86asm.net/coder32.html#modrm_byte_32
            
            // ModR/M byte:
            //  mod | reg/mem | (reg2)
            // -----+---------+-------
            //  7 6 |  5 4 3  | (2 1 0)
            
            var modifier = (X86RegOrMemModifier)(registerToken >> 6);
            var operand = new X86Operand();

            // Register-only operands:
            if (modifier == X86RegOrMemModifier.RegisterOnly)
            {
                operand.Value = GetRegisterFromToken((byte)(registerToken & 0x7), GetRegisterSize(prefixes, size));
                operand.OperandUsage = X86OperandUsage.Normal;
                return operand;
            }

            // Register-pointer operands are always 32-bit registers.
            var register = GetRegisterFromToken((byte)(registerToken & 0x7), X86RegisterSize.Dword);
            operand.OperandUsage = GetOperandType(prefixes, size);
            operand.Value = register;

            // EBP register is replaced by a direct address.
            if (modifier == X86RegOrMemModifier.RegisterPointer && register == X86Register.Ebp)
            {
                operand.Value = _reader.ReadUInt32();
                return operand;
            }

            // ESP register are replaced by a scaled index operand.
            if (register == X86Register.Esp)
                MakeScaledIndexOperandFromToken(operand, _reader.ReadByte());

            // Read correction based on modifier.
            switch (modifier)
            {
                case X86RegOrMemModifier.RegisterDispShortPointer:
                    operand.Offset = _reader.ReadSByte();
                    operand.OffsetType = X86OffsetType.Short;
                    break;
                case X86RegOrMemModifier.RegisterDispLongPointer:
                    operand.Offset = _reader.ReadInt32();
                    operand.OffsetType = X86OffsetType.Long;
                    break;
            }

            return operand;
        }

        private static void MakeScaledIndexOperandFromToken(X86Operand operand, byte token)
        {
            // Mechanism:
            // http://ref.x86asm.net/coder32.html#sib_byte_32

            // SIB-byte:
            //  mul | scaled_reg | reg
            // -----+------------+-------
            //  7 6 |   5 4 3    | 2 1 0

            var scaledIndex = new X86ScaledIndex
            {
                Register = GetRegisterFromToken((byte)((token >> 3) & 7), X86RegisterSize.Dword),
                Multiplier = 1 << ((token >> 6) & 3),
            };

            // ESP scales are ignored.
            if (scaledIndex.Register != X86Register.Esp)
                operand.ScaledIndex = scaledIndex;
            
            operand.Value = GetRegisterFromToken((byte)(token & 0x7), X86RegisterSize.Dword);
           
        }

        private static X86Register GetRegisterFromToken(byte token, X86RegisterSize size)
        {
            var register = (X86Register)token;
            switch (size)
            {
                case X86RegisterSize.Byte:
                    return register;
                case X86RegisterSize.Word:
                    return register | X86Register.Ax;
                case X86RegisterSize.Dword:
                    return register | X86Register.Eax;
            }
            throw new NotSupportedException();
        }
    }
}
