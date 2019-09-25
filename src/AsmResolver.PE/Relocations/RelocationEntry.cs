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
    /// Represents one single relocation that is applied upon loading the PE into memory.
    /// </summary>
    public readonly struct RelocationEntry
    {
        private readonly ushort _value;

        public RelocationEntry(ushort value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the type of the relocation to apply.
        /// </summary>
        public RelocationType RelocationType => (RelocationType) (_value >> 12);

        /// <summary>
        /// Gets the offset (relative to the current relocation block) to the pointer to relocate.
        /// </summary>
        public int Offset => _value & 0xFFF;
    }
}