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

namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Provides valid attributes for describing a portable executable file.
    /// </summary>
    [Flags]
    public enum Characteristics : ushort
    {
        RelocsStripped = 0x0001,
        Image = 0x0002,
        LineNumsStripped = 0x0004,
        LocalSymsStripped = 0x0008,
        AggressiveWsTrim = 0x0010,
        LargeAddressAware = 0x0020,
        Machine32Bit = 0x0100,
        DebugStripped = 0x0200,
        RemovableRunFromSwap = 0x0400,
        NetRunFromSwap = 0x0800,
        System = 0x1000,
        Dll = 0x2000,
        UpSystemOnly = 0x4000,
    }
}