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

namespace AsmResolver.PE.Imports.Internal
{
    internal class ModuleImportEntryInternal : ModuleImportEntryBase
    {
        private readonly PEFile _peFile;
        private readonly uint _lookupRva;
        private readonly uint _addressRva;
        
        public ModuleImportEntryInternal(PEFile peFile, IBinaryStreamReader reader)
        {
            _peFile = peFile;
            _lookupRva = reader.ReadUInt32();
            TimeDateStamp = reader.ReadUInt32();
            ForwarderChain = reader.ReadUInt32();
            uint nameRva = reader.ReadUInt32();
            if (nameRva != 0)
                Name = _peFile.CreateReaderAtRva(nameRva).ReadAsciiString();
            _addressRva = reader.ReadUInt32();
        }

        public bool IsEmpty =>
            _lookupRva == 0
            && TimeDateStamp == 0
            && ForwarderChain == 0
            && Name == null
            && _addressRva == 0;

        protected override IList<MemberImportEntry> GetMembers()
        {
            if (IsEmpty)
                return new List<MemberImportEntry>();
            return new MemberImportEntryList(_peFile, _lookupRva, _addressRva);
        }

    }
}