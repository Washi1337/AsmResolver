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

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides all possible flags that can be set in the first (two) byte(s) of a CIL method body.
    /// </summary>
    [Flags]
    public enum CilMethodBodyAttributes : ushort
    {
        /// <summary>
        /// Indicates the method body is using the tiny format.
        /// </summary>
        Tiny = 0x2,
        
        /// <summary>
        /// Indicates the method body is using the fat format.
        /// </summary>
        Fat = 0x3,
        
        /// <summary>
        /// Indicates more sections follow after the raw code of the method body.
        /// </summary>
        MoreSections = 0x8,
        
        /// <summary>
        /// Indicates all locals defined in the method body should be initialized to zero by the runtime.
        /// </summary>
        InitLocals = 0x10,
    }
}