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

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides an implementation of a resource directory that was read from an existing PE file.
    /// </summary>
    public class SerializedResourceDirectory : ResourceDirectory
    {
        /// <summary>
        /// Indicates the maximum depth of sub directories a resource directory can have before AsmResolver aborts
        /// reading the resource tree branch. 
        /// </summary>
        public const int MaxDepth = 10;
            
        private readonly PEFile _peFile;
        private readonly ushort _namedEntries;
        private readonly ushort _idEntries;
        private readonly uint _entriesOffset;
        private readonly int _depth;

        /// <summary>
        /// Reads a single resource directory from an input stream.
        /// </summary>
        /// <param name="peFile">The PE file containing the resource.</param>
        /// <param name="entry">The entry to read. If this value is <c>null</c>, the root directory is assumed.</param>
        /// <param name="reader">The input stream.</param>
        /// <param name="depth">
        /// The current depth of the resource directory tree structure.
        /// If this value exceeds <see cref="MaxDepth"/>, this class will not initialize any entries.
        /// </param>
        public SerializedResourceDirectory(PEFile peFile, ResourceDirectoryEntry? entry, IBinaryStreamReader reader, 
            int depth = 0)
        {
            _peFile = peFile;
            _depth = depth;

            if (entry.HasValue)
            {
                var value = entry.Value;
                if (value.IsByName)
                    Name = value.Name;
                else
                    Id = value.IdOrNameOffset;
            }

            if (reader != null)
            {
                Characteristics = reader.ReadUInt32();
                TimeDateStamp = reader.ReadUInt32();
                MajorVersion = reader.ReadUInt16();
                MinorVersion = reader.ReadUInt16();

                _namedEntries = reader.ReadUInt16();
                _idEntries = reader.ReadUInt16();
                _entriesOffset = reader.FileOffset;

                reader.FileOffset =
                    (uint) (reader.FileOffset + (_namedEntries + _idEntries) * ResourceDirectoryEntry.EntrySize);
            }
        }

        /// <inheritdoc />
        protected override IList<IResourceEntry> GetEntries()
        {
            if (_namedEntries + _idEntries == 0 // Optimisation + check for invalid resource directory offset. 
                || _depth >= MaxDepth           // Prevent self loops.
            )
            {
                return new List<IResourceEntry>();
            }

            return new SerializedResourceEntryList(_peFile, _entriesOffset, _namedEntries, _idEntries, _depth + 1);
        }

    }
}