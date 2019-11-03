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
    /// Provides members defining all flow control categories of a CIL operation code.  
    /// </summary>
    public enum CilFlowControl
    {
        /// <summary>
        /// Indicates the operation is an unconditional branching operation.
        /// </summary>
        Branch,
        
        /// <summary>
        /// Indicates the operation is a debugger break operation.
        /// </summary>
        Break,
        
        /// <summary>
        /// Indicates the operation calls a method, and returns afterwards to the next instruction.
        /// </summary>
        Call,
        
        /// <summary>
        /// Indicates the operation is a conditional branching operation.
        /// </summary>
        CondBranch,
        
        /// <summary>
        /// Indicates the operation provides information about a subsequent instruction.
        /// </summary>
        Meta,
        
        /// <summary>
        /// Indicates the operation has no special flow control properties and will execute the next instruction
        /// in the instruction stream.
        /// </summary>
        Next,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        Phi,
        
        /// <summary>
        /// Indicates the operation exits the current method, and potentially returns a value.
        /// </summary>
        Return,
        
        /// <summary>
        /// Indicates the operation throws an exception.
        /// </summary>
        Throw,
    }
}