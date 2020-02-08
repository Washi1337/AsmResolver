// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

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
    }
}