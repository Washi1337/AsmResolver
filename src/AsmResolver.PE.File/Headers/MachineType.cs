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

namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Provides members for each processor architecture that a portable executable (PE) file can encode for. 
    /// </summary>
    public enum MachineType : ushort
    {
        Unknown = 0x0000,
        Am33 = 0x01D3,
        Amd64 = 0x8664,
        Arm = 0x01C0,
        ArmNt = 0x01C4,
        Arm64 = 0xAA64,
        Ebc = 0x0EBC,
        I386 = 0x014C,
        Ia64 = 0x0200,
        M32R = 0x9041,
        Mips16 = 0x0266,
        MipsFpu = 0x0366,
        MipsFpu16 = 0x0466,
        PowerPc = 0x01F0,
        PowerPcFp = 0x01F1,
        R4000 = 0x0166,
        Sh3 = 0x01A2,
        Sh3Dsp = 0x01A3,
        Sh4 = 0x01A6,
        Sh5 = 0x01A8,
        Thumb = 0x01C2,
        WceMipsV2 = 0x0169,
    }
}