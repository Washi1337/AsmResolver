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
            return Offset == other.Offset && OpCode.Equals(other.OpCode) && Equals(Operand, other.Operand);
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
        
    }
}