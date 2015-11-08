using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Msil
{
    public sealed class MsilInstruction
    {
        public static MsilInstruction Create(MsilOpCode code)
        {
            if (code.OperandType != MsilOperandType.InlineNone)
                throw new ArgumentException("Opcode requires an operand.", "code");
            return new MsilInstruction(0, code, null);
        }

        public static MsilInstruction Create(MsilOpCode code, MsilInstruction instruction)
        {
            if (code.OperandType != MsilOperandType.ShortInlineBrTarget && code.OperandType != MsilOperandType.InlineBrTarget)
                throw new ArgumentException("Opcode does not accept an instruction operand.", "code");
            return new MsilInstruction(0, code, instruction);
        }

        public static MsilInstruction Create(MsilOpCode code, IMemberReference operand)
        {
            switch (code.OperandType)
            {
                case MsilOperandType.InlineField:
                case MsilOperandType.InlineMethod:
                case MsilOperandType.InlineTok:
                case MsilOperandType.InlineType:
                    return new MsilInstruction(0, code, operand);
            }
            throw new ArgumentException("Opcode does not accept a member operand operand.", "code");
        }

        public static MsilInstruction Create(MsilOpCode code, sbyte operand)
        {
            if (code.OperandType != MsilOperandType.ShortInlineR)
                throw new ArgumentException("Opcode does not accept an int8 operand.", "code");
            return new MsilInstruction(0, code, operand);
        }

        public static MsilInstruction Create(MsilOpCode code, int operand)
        {
            if (code.OperandType != MsilOperandType.InlineI)
                throw new ArgumentException("Opcode does not accept an int32 operand.", "code");
            return new MsilInstruction(0, code, operand);
        }

        public static MsilInstruction Create(MsilOpCode code, long operand)
        {
            if (code.OperandType != MsilOperandType.InlineI8)
                throw new ArgumentException("Opcode does not accept an int64 operand.", "code");
            return new MsilInstruction(0, code, operand);
        }

        public static MsilInstruction Create(MsilOpCode code, float operand)
        {
            if (code.OperandType != MsilOperandType.ShortInlineR)
                throw new ArgumentException("Opcode does not accept a float32 operand.", "code");
            return new MsilInstruction(0, code, operand);
        }

        public static MsilInstruction Create(MsilOpCode code, double operand)
        {
            if (code.OperandType != MsilOperandType.InlineR)
                throw new ArgumentException("Opcode does not accept a float64 operand.", "code");
            return new MsilInstruction(0, code, operand);
        }

        public static MsilInstruction Create(MsilOpCode code, string operand)
        {
            if (code.OperandType != MsilOperandType.InlineString)
                throw new ArgumentException("Opcode does not accept a string operand.", "code");
            return new MsilInstruction(0, code, operand);
        }

        public static MsilInstruction Create(MsilOpCode code, IList<MsilInstruction> operand)
        {
            if (code.OperandType != MsilOperandType.InlineSwitch)
                throw new ArgumentException("Opcode does not accept an instruction array operand.", "code");
            return new MsilInstruction(0, code, operand);
        }

        public static MsilInstruction Create(MsilOpCode code, VariableSignature operand)
        {
            if (code.OperandType != MsilOperandType.InlineVar && code.OperandType != MsilOperandType.ShortInlineVar)
                throw new ArgumentException("Opcode does not accept a local variable operand.", "code");
            return new MsilInstruction(0, code, operand);
        }

        public static MsilInstruction Create(MsilOpCode code, ParameterSignature operand)
        {
            if (code.OperandType != MsilOperandType.InlineArgument && code.OperandType != MsilOperandType.ShortInlineArgument)
                throw new ArgumentException("Opcode does not accept a parameter operand.", "code");
            return new MsilInstruction(0, code, operand);
        }

        public static MsilInstruction Create(MsilOpCode code, StandAloneSignature operand)
        {
            if (code.OperandType != MsilOperandType.InlineSig)
                throw new ArgumentException("Opcode does not accept a signature operand.", "code");
            return new MsilInstruction(0, code, operand);
        }

        public MsilInstruction(int offset, MsilOpCode opCode, object operand)
        {
            Offset = offset;
            OpCode = opCode;
            Operand = operand;
        }

        public int Offset
        {
            get;
            set;
        }

        public MsilOpCode OpCode
        {
            get;
            set;
        }

        public object Operand
        {
            get;
            set;
        }

        public int Size
        {
            get { return OpCode.Size + GetOperandSize(); }
        }

        public int GetOperandSize()
        {
            switch (OpCode.OperandType)
            {
                case MsilOperandType.InlineNone:
                    return 0;
                case MsilOperandType.ShortInlineArgument:
                case MsilOperandType.ShortInlineVar:
                case MsilOperandType.ShortInlineI:
                case MsilOperandType.ShortInlineBrTarget:
                    return 1;
                case MsilOperandType.InlineVar:
                case MsilOperandType.InlineArgument:
                    return 2;
                case MsilOperandType.ShortInlineR:
                case MsilOperandType.InlineI:
                case MsilOperandType.InlineField:
                case MsilOperandType.InlineMethod:
                case MsilOperandType.InlineSig:
                case MsilOperandType.InlineTok:
                case MsilOperandType.InlineType:
                case MsilOperandType.InlineString:
                case MsilOperandType.InlineBrTarget:
                    return 4;

                case MsilOperandType.InlineR:
                case MsilOperandType.InlineI8:
                    return 8;

                case MsilOperandType.InlineSwitch:
                    var array = Operand as Array;
                    if (array == null)
                        return 4;
                    return 4 * (array.GetLength(0) + 1);
            }
            throw new NotSupportedException();
        }

        public string OperandToString()
        {
            switch (OpCode.OperandType)
            {
                case MsilOperandType.InlineNone:
                    return string.Empty;

                case MsilOperandType.InlineArgument:
                case MsilOperandType.ShortInlineArgument:
                case MsilOperandType.InlineVar:
                case MsilOperandType.ShortInlineVar:
                    // TODO: return index

                case MsilOperandType.InlineField:
                case MsilOperandType.InlineR:
                case MsilOperandType.ShortInlineR:
                case MsilOperandType.InlineI:
                case MsilOperandType.InlineI8:
                case MsilOperandType.ShortInlineI:
                case MsilOperandType.InlineMethod:
                case MsilOperandType.InlineSig:
                case MsilOperandType.InlineTok:
                case MsilOperandType.InlineType:
                    return Convert.ToString(Operand, CultureInfo.InvariantCulture);

                case MsilOperandType.InlineString:
                    if (Operand is string)
                        return string.Format("\"{0}\"", Operand);
                    return ((MetadataToken)Operand).ToUInt32().ToString("X8");
                    
                case MsilOperandType.InlineSwitch:
                    return string.Join(", ", ((MsilInstruction[])Operand).Select(x => "IL_" + x.Offset.ToString("X4")));
                case MsilOperandType.InlineBrTarget:
                case MsilOperandType.ShortInlineBrTarget:
                    return "IL_" + ((MsilInstruction)Operand).Offset.ToString("X4");
            }
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return string.Format("IL_{0:X4}: {1}{2}", Offset, OpCode.Name,
                Operand == null ? string.Empty : " " + OperandToString());
        }
    }
}
