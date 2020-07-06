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
using AsmResolver.PE.File;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides an implementation for a single data entry in a Win32 resource directory, that was read from an existing
    /// PE file.
    /// </summary>
    public class SerializedResourceData : ResourceData
    {
        /// <summary>
        /// Indicates the size of a single data entry in a resource directory.
        /// </summary>
        public const uint ResourceDataEntrySize = 4 * sizeof(uint);
        
        private readonly PEFile _peFile;
        private readonly uint _contentsRva;
        private readonly uint _contentsSize;

        /// <summary>
        /// Reads a resource data entry from the provided input stream.
        /// </summary>
        /// <param name="peFile">The PE file containing the resource.</param>
        /// <param name="entry">The entry to read.</param>
        /// <param name="entryReader">The input stream to read the data from.</param>
        public SerializedResourceData(PEFile peFile, ResourceDirectoryEntry entry, IBinaryStreamReader entryReader)
        {
            _peFile = peFile ?? throw new ArgumentNullException(nameof(peFile));

            if (entry.IsByName)
                Name = entry.Name;
            else
                Id = entry.IdOrNameOffset;
            
            _contentsRva = entryReader.ReadUInt32();
            _contentsSize = entryReader.ReadUInt32();
            CodePage = entryReader.ReadUInt32();
        }

        /// <inheritdoc />
        protected override ISegment GetContents()
        {
            return _peFile.TryCreateReaderAtRva(_contentsRva, _contentsSize, out var reader)
                ? DataSegment.FromReader(reader)
                : null;
        }
        
    }
}