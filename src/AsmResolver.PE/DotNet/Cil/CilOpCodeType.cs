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
    /// Provides members for all operation code categories.
    /// </summary>
    public enum CilOpCodeType : byte
    {
        /// <summary>
        /// Deprecated, should not be used.
        /// </summary>
        Annotation,
        
        /// <summary>
        /// Indicates the operation code is a macro instruction that expands to another instruction, but taking less space.
        /// </summary>
        Macro,
        
        /// <summary>
        /// Indicates the operation code is a reserved instruction.
        /// </summary>
        Nternal,
        
        /// <summary>
        /// Indicates the operation code applies to objects.
        /// </summary>
        Objmodel,
        
        /// <summary>
        /// Indicates the operation code is a prefix to another instruction.
        /// </summary>
        Prefix,
        
        /// <summary>
        /// Indicates the operation code is a built-in primitive instruction.
        /// </summary>
        Primitive,
    }
}