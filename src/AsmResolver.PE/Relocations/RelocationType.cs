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

namespace AsmResolver.PE.Relocations
{
    /// <summary>
    /// Provides members for all possible types of relocations that can be applied while loading a PE into memory. 
    /// </summary>
    public enum RelocationType
    {
        Absolute = 0,
        High = 1,
        Low = 2,
        HighLow = 3,
        HighAdj = 4,
        MipsJmpAddr = 5,
        ArmMov32 = 5,
        RiscVHigh20 = 5,
        ThumbMov32 = 7,
        RiscVLow12I = 7,
        RiscVLow12S = 8,
        MipsJmpAddr16 = 9,
        Dir64 = 10
    }
}