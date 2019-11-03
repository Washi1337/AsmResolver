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
        public CilInstruction(CilOpCode opCode)
            : this(0, opCode, null)
        {
        }

        public CilInstruction(CilOpCode opCode, object operand)
            : this(0, opCode, operand)
        {
        }

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
        
    }
}