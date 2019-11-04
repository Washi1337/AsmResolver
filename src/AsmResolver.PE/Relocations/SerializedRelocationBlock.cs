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

using System.Collections.Generic;
using AsmResolver.PE.File;

namespace AsmResolver.PE.Relocations
{
    /// <summary>
    /// Provides an implementation of a relocation block that is stored in a PE file.
    /// </summary>
    public class SerializedRelocationBlock : RelocationBlock
    {
        private readonly PEFile _peFile;
        private readonly uint _offset;
        private readonly uint _size;

        /// <summary>
        /// Reads a relocation block from an input stream.
        /// </summary>
        /// <param name="peFile">The PE file containing the relocation block.</param>
        /// <param name="reader">The input stream.</param>
        public SerializedRelocationBlock(PEFile peFile, IBinaryStreamReader reader)
        {
            _peFile = peFile;
            PageRva = reader.ReadUInt32();
            
            _size = reader.ReadUInt32() - sizeof(uint) * 2;
            _offset = reader.FileOffset;
            
            reader.FileOffset += _size;
        }

        /// <inheritdoc />
        protected override IList<RelocationEntry> GetEntries()
        {
            var result = new List<RelocationEntry>();
            var reader = _peFile.CreateReaderAtFileOffset(_offset, _size);

            uint count = _size / sizeof(ushort);
            for (int i = 0; i < count; i++) 
                result.Add(new RelocationEntry(reader.ReadUInt16()));

            return result;
        }
        
    }
}