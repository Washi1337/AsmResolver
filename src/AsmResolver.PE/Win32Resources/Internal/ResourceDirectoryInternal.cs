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

namespace AsmResolver.PE.Win32Resources.Internal
{
    internal class ResourceDirectoryInternal : ResourceDirectoryBase
    {
        private readonly PEFile _peFile;
        private ushort _namedEntries;
        private ushort _idEntries;
        private IBinaryStreamReader _entriesReader;
        private string _name;

        internal ResourceDirectoryInternal(PEFile peFile, ResourceDirectoryEntry entry, IBinaryStreamReader reader)
        {
            _peFile = peFile;

            if (entry != null)
            {
                if (entry.IsByName)
                    Name = entry.Name;
                else
                    Id = entry.IdOrNameOffset;
            }

            Characteristics = reader.ReadUInt32();
            TimeDateStamp = reader.ReadUInt32();
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            
            _namedEntries = reader.ReadUInt16();
            _idEntries = reader.ReadUInt16();
            _entriesReader = reader.Fork();
            
            reader.FileOffset = (uint) (reader.FileOffset + (_namedEntries + _idEntries) * ResourceDirectoryEntry.EntrySize);
        }

        protected override IList<IResourceDirectoryEntry> GetEntries()
        {
            return new ResourceDirectoryEntryList(_peFile, _entriesReader, _namedEntries, _idEntries);
        }
        
    }
}