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
    /// Provides members defining all possible stack behaviours that a single CIL operation can have.
    /// </summary>
    public enum CilStackBehaviour : byte
    {
        /// <summary>
        /// Indicates the operation pops no values from the stack.
        ///</summary>
        Pop0,

        /// <summary>
        /// Indicates the operation pops one value from the stack.
        ///</summary>
        Pop1,

        /// <summary>
        /// Indicates the operation pops one value off the stack for the first operand, and one value of the stack
        /// for the second operand.
        ///</summary>
        Pop1_Pop1,

        /// <summary>
        /// Indicates the operation pops a 32-bit integer off the stack.
        ///</summary>
        PopI,

        /// <summary>
        /// Indicates the operation pops a 32-bit integer off the stack for the first operand, and a value off the stack
        /// for the second operand.
        ///</summary>
        PopI_Pop1,

        /// <summary>
        /// Indicates the operation pops a 32-bit integer off the stack for the first operand, and a 32-bit integer off
        /// the stack for the second operand.
        ///</summary>
        PopI_PopI,

        /// <summary>
        /// Indicates the operation pops a 32-bit integer off the stack for the first operand, and a 64-bit integer off
        /// the stack for the second operand.
        ///</summary>
        PopI_PopI8,

        /// <summary>
        /// Indicates the operation pops a 32-bit integer off the stack for the first operand, a 32-bit integer off the
        /// stack for the second operand, and a 32-bit integer off the stack for the third operand.
        ///</summary>
        PopI_PopI_PopI,

        /// <summary>
        /// Indicates the operation pops a 32-bit integer off the stack for the first operand, and a 32-bit floating
        /// point number off the stack for the second operand.
        ///</summary>
        PopI_PopR4,

        /// <summary>
        /// Indicates the operation pops a 32-bit integer off the stack for the first operand, and a 64-bit floating
        /// point number off the stack for the second operand.
        ///</summary>
        PopI_PopR8,

        /// <summary>
        /// Indicates the operation pops a reference off the stack.
        ///</summary>
        PopRef,

        /// <summary>
        /// Indicates the operation pops a reference off the stack for the first operand, and a value off the stack for
        /// the second operand.
        ///</summary>
        PopRef_Pop1,

        /// <summary>
        /// Indicates the operation pops a reference off the stack for the first operand, and a 32-bit integer off the
        /// stack for the second operand.
        ///</summary>
        PopRef_PopI,

        /// <summary>
        /// Indicates the operation pops a reference off the stack for the first operand, a value off the stack for the
        /// second operand, and a value off the stack for the third operand.
        ///</summary>
        PopRef_PopI_PopI,

        /// <summary>
        /// Indicates the operation pops a reference off the stack for the first operand, a value off the stack for the
        /// second operand, and a 64-bit integer off the stack for the third operand.
        ///</summary>
        PopRef_PopI_PopI8,

        /// <summary>
        /// Indicates the operation pops a reference off the stack for the first operand, a value off the stack for the
        /// second operand, and a 32-bit integer off the stack for the third operand.
        ///</summary>
        PopRef_PopI_PopR4,

        /// <summary>
        /// Indicates the operation pops a reference off the stack for the first operand, a value off the stack for the
        /// second operand, and a 64-bit floating point number off the stack for the third operand.
        ///</summary>
        PopRef_PopI_PopR8,

        /// <summary>
        /// Indicates the operation pops a reference off the stack for the first operand, a value off the stack for the
        /// second operand, and a reference off the stack for the third operand.
        ///</summary>
        PopRef_PopI_PopRef,

        /// <summary>
        /// Indicates the operation pops a reference off the stack for the first operand, a value off the stack for the
        /// second operand, and a 32-bit integer off the stack for the third operand.
        ///</summary>
        PopRef_PopI_Pop1,
        
        /// <summary>
        /// Indicates the operation clears the evaluation stack.
        /// </summary>
        PopAll,
        
        /// <summary>
        /// Indicates the operation pushes no values onto the stack.
        ///</summary>
        Push0,

        /// <summary>
        /// Indicates the operation pushes one value onto the stack.
        ///</summary>
        Push1,

        /// <summary>
        /// Indicates the operation pushes 1 value onto the stack for the first operand, and 1 value onto the stack for
        /// the second operand.
        ///</summary>
        Push1_Push1,

        /// <summary>
        /// Indicates the operation pushes a 32-bit integer onto the stack.
        ///</summary>
        PushI,

        /// <summary>
        /// Indicates the operation pushes a 64-bit integer onto the stack.
        ///</summary>
        PushI8,

        /// <summary>
        /// Indicates the operation pushes a 32-bit floating point number onto the stack.
        ///</summary>
        PushR4,

        /// <summary>
        /// Indicates the operation pushes a 64-bit floating point number onto the stack.
        ///</summary>
        PushR8,

        /// <summary>
        /// Indicates the operation pushes a reference onto the stack.
        ///</summary>
        PushRef,

        /// <summary>
        /// Indicates the operation pops a variable amount of values off the stack.
        ///</summary>
        VarPop,

        /// <summary>
        /// Indicates the operation pushes a variable amount of values onto the stack.
        ///</summary>
        VarPush,
    }
}