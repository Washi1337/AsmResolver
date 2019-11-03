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
    /// Defines all possible flags that an extra section in a method body can set.
    /// </summary>
    [Flags]
    public enum CilExtraSectionAttributes : byte
    {
        /// <summary>
        /// Indicates the extra section contains an exception handler table.
        /// </summary>
        EHTable = 0x01,
        
        /// <summary>
        /// Indicates the extra section contains an OptIL table (not supported anymore by the CLR).
        /// </summary>
        OptILTable = 0x02,
        
        /// <summary>
        /// Indicates the extra section uses the fat format to store its data.
        /// </summary>
        FatFormat = 0x40,
        
        /// <summary>
        /// Indicates at least one more section follows this extra section.
        /// </summary>
        MoreSections = 0x80,
    }
}