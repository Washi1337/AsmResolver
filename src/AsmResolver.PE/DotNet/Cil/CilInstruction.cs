using System;
using System.Collections;
using System.Linq;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Represents a single instruction in a managed CIL method body.
    /// </summary>
    public class CilInstruction
    {
        /// <summary>
        /// Create a new instruction pushing the provided integer value, using the smallest possible operation code and
        /// operand size. 
        /// </summary>
        /// <param name="value">The constant to push.</param>
        /// <returns>The instruction.</returns>
        public static CilInstruction CreateLdcI4(int value)
        {
            var (code, operand) = GetLdcI4OpCodeOperand(value);
            return new CilInstruction(code, operand);
        }

        /// <summary>
        /// Determines the smallest possible operation code and operand required to push the provided integer constant.
        /// </summary>
        /// <param name="value">The constant to push.</param>
        /// <returns>The operation code and operand.</returns>
        public static (CilOpCode code, object operand) GetLdcI4OpCodeOperand(int value)
        {
            CilOpCode code;
            object operand = null;
            switch (value)
            {
                case -1:
                    code = CilOpCodes.Ldc_I4_M1;
                    break;
                case 0:
                    code = CilOpCodes.Ldc_I4_0;
                    break;
                case 1:
                    code = CilOpCodes.Ldc_I4_1;
                    break;
                case 2:
                    code = CilOpCodes.Ldc_I4_2;
                    break;
                case 3:
                    code = CilOpCodes.Ldc_I4_3;
                    break;
                case 4:
                    code = CilOpCodes.Ldc_I4_4;
                    break;
                case 5:
                    code = CilOpCodes.Ldc_I4_5;
                    break;
                case 6:
                    code = CilOpCodes.Ldc_I4_6;
                    break;
                case 7:
                    code = CilOpCodes.Ldc_I4_7;
                    break;
                case 8:
                    code = CilOpCodes.Ldc_I4_8;
                    break;
                case { } x when x >= sbyte.MinValue && x <= sbyte.MaxValue:
                    code = CilOpCodes.Ldc_I4_S;
                    operand = (sbyte) x;
                    break;
                default:
                    code = CilOpCodes.Ldc_I4;
                    operand = value;
                    break;
            }

            return (code, operand);
        }

        /// <summary>
        /// Creates a new CIL instruction with no operand.
        /// </summary>
        /// <param name="opCode">The operation to perform.</param>
        /// <remarks>
        /// This constructor does not do any verification on the correctness of the instruction.
        /// </remarks>
        public CilInstruction(CilOpCode opCode)
            : this(0, opCode, null)
        {
        }

        /// <summary>
        /// Creates a new CIL instruction with no operand.
        /// </summary>
        /// <param name="offset">The offset of the instruction, relative to the start of the method body's code.</param>
        /// <param name="opCode">The operation to perform.</param>
        /// <remarks>
        /// This constructor does not do any verification on the correctness of the instruction.
        /// </remarks>
        public CilInstruction(int offset, CilOpCode opCode)
            : this(offset, opCode, null)
        {
        }

        /// <summary>
        /// Creates a new CIL instruction with an operand..
        /// </summary>
        /// <param name="opCode">The operation to perform.</param>
        /// <param name="operand">The operand.</param>
        /// <remarks>
        /// This constructor does not do any verification on the correctness of the instruction.
        /// </remarks>
        public CilInstruction(CilOpCode opCode, object operand)
            : this(0, opCode, operand)
        {
        }

        /// <summary>
        /// Creates a new CIL instruction with an operand..
        /// </summary>
        /// <param name="offset">The offset of the instruction, relative to the start of the method body's code.</param>
        /// <param name="opCode">The operation to perform.</param>
        /// <param name="operand">The operand.</param>
        /// <remarks>
        /// This constructor does not do any verification on the correctness of the instruction.
        /// </remarks>
        public CilInstruction(int offset, CilOpCode opCode, object operand)
        {
            Offset = offset;
            OpCode = opCode;
            Operand = operand;
        }

        /// <summary>
        /// Gets or sets the offset to the start of the instruction, relative to the start of the code. 
        /// </summary>
        public int Offset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the operation to perform.
        /// </summary>
        public CilOpCode OpCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the operand of the instruction, if available.
        /// </summary>
        public object Operand
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the size in bytes of the CIL instruction.
        /// </summary>
        public int Size => OpCode.Size + GetOperandSize();

        private int GetOperandSize() =>
            OpCode.OperandType switch
            {
                CilOperandType.InlineNone => 0,
                CilOperandType.ShortInlineI => sizeof(sbyte),
                CilOperandType.ShortInlineArgument => sizeof(sbyte),
                CilOperandType.ShortInlineBrTarget => sizeof(sbyte),
                CilOperandType.ShortInlineVar => sizeof(sbyte),
                CilOperandType.InlineVar => sizeof(ushort),
                CilOperandType.InlineArgument => sizeof(ushort),
                CilOperandType.InlineBrTarget => sizeof(uint),
                CilOperandType.InlineI => sizeof(uint),
                CilOperandType.InlineField => sizeof(uint),
                CilOperandType.InlineMethod => sizeof(uint),
                CilOperandType.InlineSig => sizeof(uint),
                CilOperandType.InlineString => sizeof(uint),
                CilOperandType.InlineTok => sizeof(uint),
                CilOperandType.InlineType => sizeof(uint),
                CilOperandType.InlineI8 => sizeof(ulong),
                CilOperandType.ShortInlineR => sizeof(float),
                CilOperandType.InlineR => sizeof(double),
                CilOperandType.InlineSwitch => ((((ICollection) Operand).Count + 1) * sizeof(int)),
                CilOperandType.InlinePhi => throw new NotSupportedException(),
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <inheritdoc />
        public override string ToString()
        {
            return Operand is null
                ? $"IL_{Offset:X4}: {OpCode.Mnemonic}"
                : $"IL_{Offset:X4}: {OpCode.Mnemonic} {Operand}";
        }

        /// <summary>
        /// Determines whether the provided instruction is considered equal to the current instruction.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns><c>true</c> if the instructions are equal, <c>false</c> otherwise.</returns>
        protected bool Equals(CilInstruction other)
        {
            if (Offset != other.Offset || !OpCode.Equals(other.OpCode)) 
                return false;

            if (OpCode.Code == CilCode.Switch 
                && Operand is IEnumerable list1
                && other.Operand is IEnumerable list2)
            {
                return list1.Cast<object>().SequenceEqual(list2.Cast<object>());
            }
            
            return Equals(Operand, other.Operand);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj)) 
                return true;
            if (obj.GetType() != GetType()) 
                return false;
            return Equals((CilInstruction) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Offset;
                hashCode = (hashCode * 397) ^ OpCode.GetHashCode();
                hashCode = (hashCode * 397) ^ (Operand != null ? Operand.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Creates a new label to the current instruction.
        /// </summary>
        /// <returns>The label.</returns>
        public ICilLabel CreateLabel() => new CilInstructionLabel(this);

        /// <summary>
        /// Determines whether the instruction is using a variant of the ldloc opcodes.
        /// </summary>
        public bool IsLdloc()
        {
            switch (OpCode.Code)
            {
                case CilCode.Ldloc:
                case CilCode.Ldloc_0:
                case CilCode.Ldloc_1:
                case CilCode.Ldloc_2:
                case CilCode.Ldloc_3:
                case CilCode.Ldloc_S:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the instruction is using a variant of the stloc opcodes.
        /// </summary>
        public bool IsStloc()
        {
            switch (OpCode.Code)
            {
                case CilCode.Stloc:
                case CilCode.Stloc_0:
                case CilCode.Stloc_1:
                case CilCode.Stloc_2:
                case CilCode.Stloc_3:
                case CilCode.Stloc_S:
                    return true;
                default:
                    return false;
            }
        }


        /// <summary>
        /// Determines whether the instruction is using a variant of the ldarg opcodes.
        /// </summary>
        public bool IsLdarg()
        {
            switch (OpCode.Code)
            {
                case CilCode.Ldarg:
                case CilCode.Ldarg_0:
                case CilCode.Ldarg_1:
                case CilCode.Ldarg_2:
                case CilCode.Ldarg_3:
                case CilCode.Ldarg_S:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the instruction is using a variant of the starg opcodes.
        /// </summary>
        public bool IsStarg()
        {
            switch (OpCode.Code)
            {
                case CilCode.Starg:
                case CilCode.Starg_S:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the instruction is a branching instruction (either conditional or unconditional).
        /// </summary>
        public bool IsBranch() =>
            OpCode.FlowControl == CilFlowControl.Branch
            || OpCode.FlowControl == CilFlowControl.ConditionalBranch;

        /// <summary>
        /// Determines whether the instruction is an unconditional branch instruction.
        /// </summary>
        public bool IsUnconditionalBranch() => 
            OpCode.FlowControl == CilFlowControl.Branch;

        /// <summary>
        /// Determines whether the instruction is a conditional branch instruction.
        /// </summary>
        public bool IsConditionalBranch() => 
            OpCode.FlowControl == CilFlowControl.ConditionalBranch;

        /// <summary>
        /// Determines whether the instruction is an instruction pushing an int32 constant onto the stack.
        /// </summary>
        public bool IsLdcI4()
        {
            switch (OpCode.Code)
            {
                case CilCode.Ldc_I4:
                case CilCode.Ldc_I4_S:
                case CilCode.Ldc_I4_0:
                case CilCode.Ldc_I4_1:
                case CilCode.Ldc_I4_2:
                case CilCode.Ldc_I4_3:
                case CilCode.Ldc_I4_4:
                case CilCode.Ldc_I4_5:
                case CilCode.Ldc_I4_6:
                case CilCode.Ldc_I4_7:
                case CilCode.Ldc_I4_8:
                case CilCode.Ldc_I4_M1:
                    return true;
                
                default:
                    return false;
            }
        }

        /// <summary>
        /// When this instruction is an ldc.i4 variant, gets the in32 constant that is being pushed onto the stack.
        /// </summary>
        public int GetLdcI4Constant()
        {
            return OpCode.Code switch
            {
                CilCode.Ldc_I4 => (int) Operand,
                CilCode.Ldc_I4_S => (sbyte) Operand,
                CilCode.Ldc_I4_0 => 0,
                CilCode.Ldc_I4_1 => 1,
                CilCode.Ldc_I4_2 => 2,
                CilCode.Ldc_I4_3 => 3,
                CilCode.Ldc_I4_4 => 4,
                CilCode.Ldc_I4_5 => 5,
                CilCode.Ldc_I4_6 => 6,
                CilCode.Ldc_I4_7 => 7,
                CilCode.Ldc_I4_8 => 8,
                CilCode.Ldc_I4_M1 => -1,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}