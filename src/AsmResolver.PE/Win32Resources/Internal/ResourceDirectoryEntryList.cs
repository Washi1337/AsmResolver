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

using System.Diagnostics;
using AsmResolver.Lazy;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Win32Resources.Internal
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    internal class ResourceDirectoryEntryList : LazyList<IResourceDirectoryEntry>
    {
        private readonly PEFile _peFile;
        private readonly uint _entriesOffset;
        private readonly int _namedEntries;
        private readonly int _idEntries;

        public ResourceDirectoryEntryList(PEFile peFile, uint entriesOffset, int namedEntries, int idEntries)
        {
            _namedEntries = namedEntries;
            _idEntries = idEntries;
            _peFile = peFile;
            _entriesOffset = entriesOffset;
        }

        protected override void Initialize()
        {
            var entriesReader = _peFile.CreateReaderAtFileOffset(
                _entriesOffset,
                (uint) ((_namedEntries + _idEntries) * ResourceDirectoryEntry.EntrySize));

            uint baseRva = _peFile.OptionalHeader.DataDirectories[OptionalHeader.ResourceDirectoryIndex].VirtualAddress;

            for (int i = 0; i < _namedEntries + _idEntries; i++)
            {
                var rawEntry = new ResourceDirectoryEntry(_peFile, entriesReader, i < _namedEntries);
                var entryReader = _peFile.CreateReaderAtRva(baseRva + rawEntry.DataOrSubDirOffset);

                var entry = rawEntry.IsSubDirectory
                    ? (IResourceDirectoryEntry) new ResourceDirectoryInternal(_peFile, rawEntry, entryReader)
                    : new ResourceDataInternal(_peFile, rawEntry, entryReader);

                Items.Add(entry);
            }
        }

    }
}