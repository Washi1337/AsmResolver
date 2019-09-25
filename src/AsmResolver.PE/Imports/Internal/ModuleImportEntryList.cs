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
using System.Diagnostics;
using AsmResolver.Lazy;
using AsmResolver.PE.File;

namespace AsmResolver.PE.Imports.Internal
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    internal class ModuleImportEntryList : LazyList<ModuleImportEntryBase>
    {
        private readonly PEFile _peFile;
        private readonly IBinaryStreamReader _reader;

        public ModuleImportEntryList(PEFile peFile, IBinaryStreamReader reader)
        {
            _peFile = peFile ?? throw new ArgumentNullException(nameof(peFile));
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }
        
        protected override void Initialize()
        {
            while (true)
            {
                var entry = ModuleImportEntryInternal.FromReader(_peFile, _reader);
                if (entry == null)
                    break;
                Items.Add(entry);
            }
        }
        
    }
}