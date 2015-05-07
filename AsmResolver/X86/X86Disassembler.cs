using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.X86
{
    public class X86Disassembler
    {
        private readonly IBinaryStreamReader _reader;

        public X86Disassembler(IBinaryStreamReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            _reader = reader;
        }

        public X86Instruction ReadNextInstruction()
        {
            var instruction = new X86Instruction(_reader.Position)
            {
                OpCode = ReadOpcode()
            };

            if (instruction.OpCode.Mnemonics == null)
                return instruction;

            var registerToken = instruction.OpCode.HasRegisterToken ? _reader.ReadByte() : (byte)0;
            var mnemonicIndex = instruction.OpCode.HasOpCodeModifier ? (registerToken >> 3) & 7 : 0;
            instruction.Mnemonic = instruction.OpCode.Mnemonics[mnemonicIndex];

            instruction.Operand1 = ReadOperand(instruction.OpCode.AddressingMethods1[mnemonicIndex],
                instruction.OpCode.OperandSizes1[mnemonicIndex], instruction.OpCode.Op1, registerToken);

            instruction.Operand2 = ReadOperand(instruction.OpCode.AddressingMethods2[mnemonicIndex],
                instruction.OpCode.OperandSizes2[mnemonicIndex], instruction.OpCode.Op1, registerToken);

            return instruction;
        }

        private X86OpCode ReadOpcode()
        {
            var code1 = _reader.ReadByte();

            switch (code1)
            {
                case 0x0F:
                    return X86OpCodes.MultiByteOpCodes[_reader.ReadByte()];

                default:
                    return X86OpCodes.SingleByteOpCodes[code1];
            }
        }

        private X86Operand ReadOperand(X86AddressingMethod method, X86OperandSize size, byte opcode, byte registerToken)
        {
            var offset = _reader.Position;
            switch (method)
            {
                case X86AddressingMethod.OpCodeRegister:
                    return new X86Operand(GetRegisterFromToken((byte)(opcode & 7), GetRegisterSize(size)));

                case X86AddressingMethod.Register:
                    return new X86Operand(GetRegisterFromToken((byte)((registerToken >> 3) & 7),
                        GetRegisterSize(size)));

                case X86AddressingMethod.RegisterOrMemoryAddress:
                    return GetRegOrMemOperand32(registerToken, size);

                case X86AddressingMethod.ImmediateData:
                    return new X86Operand(ReadImmediateData(size));

                case X86AddressingMethod.MemoryAddress:
                    return new X86Operand(_reader.ReadUInt32(), GetOperandType(size));

                case X86AddressingMethod.RegisterAl:
                    return new X86Operand(X86Register.Al);

                case X86AddressingMethod.RegisterCl:
                    return new X86Operand(X86Register.Cl);

                case X86AddressingMethod.RegisterDx:
                    return new X86Operand(X86Register.Dx);

                case X86AddressingMethod.RegisterEax:
                    return new X86Operand(X86Register.Eax);
                    
                case X86AddressingMethod.ImmediateOne:
                    return new X86Operand(1);

                case X86AddressingMethod.RelativeOffset:
                    return new X86Operand((ulong)(Convert.ToInt64(ReadSignedImmediateData(size)) + _reader.Position));

                case X86AddressingMethod.None:
                    return null;

            }
            throw new NotSupportedException();
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

        private object ReadImmediateData(X86OperandSize size)
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
                    return _reader.ReadUInt32(); // TODO: use operand-size override opcode
                    // TODO: fword
            }
            throw new NotSupportedException();
        }

        private static X86RegisterSize GetRegisterSize(X86OperandSize size)
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
                    return X86RegisterSize.Dword ; // TODO: use operand-size override opcode
            }
            throw new ArgumentException();
        }

        private static X86OperandType GetOperandType(X86OperandSize size)
        {
            switch (size)
            {
                case X86OperandSize.Byte:
                    return X86OperandType.BytePointer;
                case X86OperandSize.Dword:
                    return X86OperandType.DwordPointer;
                case X86OperandSize.WordOrDword:
                    return X86OperandType.DwordPointer; // TODO: use operand-size override opcode
                case X86OperandSize.Fword:
                    return X86OperandType.FwordPointer;
            }
            throw new ArgumentException();
        }

        private X86Operand GetRegOrMemOperand32(byte registerToken, X86OperandSize size)
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
                operand.Value = GetRegisterFromToken((byte)(registerToken & 0x7), GetRegisterSize(size));
                operand.OperandType = X86OperandType.Normal;
                return operand;
            }

            // Register-pointer operands are always 32-bit registers.
            var register = GetRegisterFromToken((byte)(registerToken & 0x7), X86RegisterSize.Dword);
            operand.OperandType = GetOperandType(size);
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
                    operand.Correction = _reader.ReadSByte();
                    break;
                case X86RegOrMemModifier.RegisterDispLongPointer:
                    operand.Correction = _reader.ReadInt32();
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
