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
using AsmResolver.Collections;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Imports.Internal
{
    internal class MemberImportEntryList : LazyList<MemberImportEntry>
    {
        private readonly PEFile _peFile;
        private readonly uint _lookupRva;
        private readonly uint _addressRva;
        private readonly bool _is32Bit;
        
        public MemberImportEntryList(PEFile peFile, uint lookupRva, uint addressRva)
        {
            _is32Bit = peFile.OptionalHeader.Magic == OptionalHeaderMagic.Pe32;
            _peFile = peFile;
            _lookupRva = lookupRva;
            _addressRva = addressRva;
        }

        private IList<ulong> ReadEntries(uint rva)
        {
            var result = new List<ulong>();
            var itemReader = _peFile.CreateReaderAtRva(rva);
            
            ulong currentItem = 0;
            while (true)
            {
                currentItem = itemReader.ReadNativeInt(_is32Bit);
                if (currentItem == 0)
                    break;
                result.Add(currentItem);
            }

            return result;
        }

        protected override void Initialize()
        {
            ulong ordinalMask = _is32Bit
                ? 0x8000_0000ul
                : 0x8000_0000_0000_0000ul;
                
            var lookupItems = ReadEntries(_lookupRva);
            var addresses = ReadEntries(_addressRva);

            for (int i = 0; i < lookupItems.Count; i++)
            {
                MemberImportEntry entry;
                
                ulong lookupItem = lookupItems[i];
                if ((lookupItem & ordinalMask) != 0)
                {
                    entry = new MemberImportEntry((ushort) (lookupItem & 0xFFFF));
                }
                else
                {
                    uint hintNameRva = (uint) (lookupItem & 0xFFFFFFFF);
                    var reader = _peFile.CreateReaderAtRva(hintNameRva);
                    entry = new MemberImportEntry(reader.ReadUInt16(), reader.ReadAsciiString());
                }

                entry.Address = addresses[i];

                Items.Add(entry);
            }
        }

    }
}