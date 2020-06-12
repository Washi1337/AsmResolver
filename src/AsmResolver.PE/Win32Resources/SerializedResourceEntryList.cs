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
using AsmResolver.Collections;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides an implementation of a lazy-initialized list of resource directory entries, read from an existing PE file.
    /// </summary>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class SerializedResourceEntryList : OwnedCollection<IResourceDirectory, IResourceEntry>
    {
        private readonly PEFile _peFile;
        private readonly IWin32ResourceDataReader _dataReader;
        private readonly uint _entriesOffset;
        private readonly int _namedEntries;
        private readonly int _idEntries;
        private readonly int _depth;

        /// <summary>
        /// Prepares a new lazy-initializes resource entry list.
        /// </summary>
        /// <param name="owner">The directory owning the list.</param>
        /// <param name="peFile">The PE file containing the resources.</param>
        /// <param name="dataReader">The instance responsible for reading and interpreting the data.</param>
        /// <param name="entriesOffset">The offset to the entries of the list.</param>
        /// <param name="namedEntries">The number of named entries.</param>
        /// <param name="idEntries">The number of unnamed entries.</param>
        /// <param name="depth">The current depth of the resource directory tree structure.</param>
        public SerializedResourceEntryList(IResourceDirectory owner, PEFile peFile, IWin32ResourceDataReader dataReader,
            uint entriesOffset, int namedEntries, int idEntries, int depth)
            : base(owner)
        {
            _namedEntries = namedEntries;
            _idEntries = idEntries;
            _depth = depth;
            _peFile = peFile;
            _dataReader = dataReader;
            _entriesOffset = entriesOffset;
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            var entriesReader = _peFile.CreateReaderAtFileOffset(
                _entriesOffset,
                (uint) ((_namedEntries + _idEntries) * ResourceDirectoryEntry.EntrySize));

            uint baseRva = _peFile.OptionalHeader.DataDirectories[OptionalHeader.ResourceDirectoryIndex].VirtualAddress;

            for (int i = 0; i < _namedEntries + _idEntries; i++)
            {
                var rawEntry = new ResourceDirectoryEntry(_peFile, entriesReader);
                _peFile.TryCreateReaderAtRva(baseRva + rawEntry.DataOrSubDirOffset, out var entryReader);

                var entry = rawEntry.IsSubDirectory
                    ? (IResourceEntry) new SerializedResourceDirectory(_peFile, _dataReader, rawEntry, entryReader, _depth)
                    : new SerializedResourceData(_peFile, _dataReader, rawEntry, entryReader);

                Items.Add(entry);
            }
        }

    }
}