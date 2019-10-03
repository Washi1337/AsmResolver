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
using System.Collections.Generic;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Relocations;
using AsmResolver.PE.Win32Resources;

namespace AsmResolver.PE
{
    public class SerializedPEImage : PEImage
    {
        private readonly PEFile _peFile;

        public SerializedPEImage(PEFile peFile)
        {
            _peFile = peFile ?? throw new ArgumentNullException(nameof(peFile));
        }

        protected override IList<IModuleImportEntry> GetImports()
        {
            var dataDirectory = _peFile.OptionalHeader.DataDirectories[OptionalHeader.ImportDirectoryIndex];
            return dataDirectory.IsPresentInPE
                ? (IList<IModuleImportEntry>) new SerializedModuleImportEntryList(_peFile, dataDirectory)
                : new List<IModuleImportEntry>();
        }

        protected override IResourceDirectory GetResources()
        {
            var dataDirectory = _peFile.OptionalHeader.DataDirectories[OptionalHeader.ResourceDirectoryIndex];
            if (!dataDirectory.IsPresentInPE || !_peFile.TryCreateDataDirectoryReader(dataDirectory, out var reader))
                return null;

            return new SerializedResourceDirectory(_peFile, null, reader, 0);
        }

        protected override IList<IRelocationBlock> GetRelocations()
        {
            var dataDirectory = _peFile.OptionalHeader.DataDirectories[OptionalHeader.BaseRelocationDirectoryIndex];
            return dataDirectory.IsPresentInPE
                ? new SerializedRelocationBlockList(_peFile, dataDirectory)
                : (IList<IRelocationBlock>) new List<IRelocationBlock>();
        }

        protected override IDotNetDirectory GetDotNetDirectory()
        {
            var dataDirectory = _peFile.OptionalHeader.DataDirectories[OptionalHeader.ClrDirectoryIndex];
            if (!dataDirectory.IsPresentInPE || !_peFile.TryCreateDataDirectoryReader(dataDirectory, out var reader))
                return null;
            
            return new SerializedDotNetDirectory(_peFile, reader);
        }
    }
}