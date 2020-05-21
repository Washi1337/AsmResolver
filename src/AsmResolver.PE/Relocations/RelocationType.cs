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
        /// <summary>
        /// Indicates the relocation is ignored.
        /// </summary>
        Absolute = 0,
        
        /// <summary>
        /// Indicates the base relocation adds the high 16 bits of the difference to the 16-bit field at offset.
        /// The 16-bit field represents the high value of a 32-bit word. 
        /// </summary>
        High = 1,
        
        /// <summary>
        /// Indicates the base relocation adds the low 16 bits of the difference to the 16-bit field at offset.
        /// The 16-bit field represents the low half of a 32-bit word. 
        /// </summary>
        Low = 2,
        
        /// <summary>
        /// Indicates the base relocation applies all 32 bits of the difference to the 32-bit field at offset. 
        /// </summary>
        HighLow = 3,
        
        /// <summary>
        /// Indicates the base relocation adds the high 16 bits of the difference to the 16-bit field at offset.
        /// The 16-bit field represents the high value of a 32-bit word. The low 16 bits of the 32-bit value are stored
        /// in the 16-bit word that follows this base relocation. This means that this base relocation occupies two slots. 
        /// </summary>
        HighAdj = 4,
        
        /// <summary>
        /// Indicates the relocation interpretation is dependent on the machine type.
        /// </summary>
        /// <remarks>
        /// This relocation is meaningful only when the machine type is MIPS.
        /// </remarks>
        MipsJmpAddr = 5,
        
        /// <summary>
        /// Indicates the base relocation applies the 32-bit address of a symbol across a consecutive MOVW/MOVT
        /// instruction pair. 
        /// </summary>
        /// <remarks>
        /// This relocation is meaningful only when the machine type is ARM or Thumb.
        /// </remarks>
        ArmMov32 = 5,
        
        /// <summary>
        /// Indicates the base relocation applies to the high 20 bits of a 32-bit absolute address. 
        /// </summary>
        /// <remarks>
        /// This relocation is meaningful only when the machine type is RISC-V.
        /// </remarks>
        RiscVHigh20 = 5,
        
        /// <summary>
        /// Indicates the base relocation applies the 32-bit address of a symbol to a consecutive MOVW/MOVT instruction pair. 
        /// </summary>
        /// <remarks>
        /// This relocation is meaningful only when the machine type is Thumb.
        /// </remarks>
        ThumbMov32 = 7,
        
        /// <summary>
        /// Indicates the base relocation applies to the low 12 bits of a 32-bit absolute address formed in RISC-V
        /// I-type instruction format. 
        /// </summary>
        /// <remarks>
        /// This relocation is meaningful only when the machine type is RISC-V.
        /// </remarks>
        RiscVLow12I = 7,
        
        /// <summary>
        /// Indicates the base relocation applies to the low 12 bits of a 32-bit absolute address formed in RISC-V
        /// S-type instruction format. 
        /// </summary>
        /// <remarks>
        /// This relocation is meaningful only when the machine type is RISC-V.
        /// </remarks>
        RiscVLow12S = 8,
        
        /// <summary>
        /// Indicates the base relocation applies to a MIPS16 jump instruction. 
        /// </summary>
        /// <remarks>
        /// This relocation is meaningful only when the machine type is MIPS.
        /// </remarks>
        MipsJmpAddr16 = 9,
        
        /// <summary>
        /// Indicates the base relocation applies the difference to the 64-bit field at offset. 
        /// </summary>
        Dir64 = 10
    }
}