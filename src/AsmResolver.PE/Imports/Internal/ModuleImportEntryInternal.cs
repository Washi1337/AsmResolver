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
        private readonly uint _nameRva;
        
        private string _name;

        public static ModuleImportEntryInternal FromReader(PEFile peFile, IBinaryStreamReader reader)
        {
            uint lookupRva = reader.ReadUInt32();
            uint timeDateStamp = reader.ReadUInt32();
            uint forwarderChain = reader.ReadUInt32();
            uint nameRva = reader.ReadUInt32();
            uint addressRva = reader.ReadUInt32();

            if (lookupRva == 0 && timeDateStamp == 0 && forwarderChain == 0 && nameRva == 0 && addressRva == 0)
                return null;
            
            return new ModuleImportEntryInternal(
                peFile,
                lookupRva,
                timeDateStamp,
                forwarderChain,
                nameRva,
                addressRva);
        }
        
        public ModuleImportEntryInternal(
            PEFile peFile,
            uint lookupRva, 
            uint timeDateStamp,
            uint forwarderChain,
            uint nameRva,
            uint addressRva)
        {
            _peFile = peFile;
            _nameRva = nameRva;
            TimeDateStamp = timeDateStamp;
            ForwarderChain = forwarderChain;

            Members = new MemberImportEntryList(peFile, lookupRva, addressRva);
        }

        public override string Name
        {
            get => _name ?? (_name = _peFile.CreateReaderAtRva(_nameRva).ReadAsciiString());
            set => _name = value;
        }

        public override IList<MemberImportEntry> Members
        {
            get;
        }
        
    }
}