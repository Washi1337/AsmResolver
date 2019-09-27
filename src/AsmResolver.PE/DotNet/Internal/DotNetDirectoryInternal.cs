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
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.DotNet.Internal
{
    internal class DotNetDirectoryInternal : DotNetDirectory
    {
        private readonly PEFile _peFile;
        private readonly DataDirectory _metadataDirectory;
        private readonly DataDirectory _resourcesDirectory;
        private readonly DataDirectory _strongNameDirectory;
        private readonly DataDirectory _codeManagerDirectory;
        private readonly DataDirectory _vtableFixupsDirectory;
        private readonly DataDirectory _exportsDirectory;
        private readonly DataDirectory _nativeHeaderDirectory;

        public DotNetDirectoryInternal(PEFile peFile, IBinaryStreamReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            _peFile = peFile ?? throw new ArgumentNullException(nameof(peFile));

            uint cb = reader.ReadUInt32();
            MajorRuntimeVersion = reader.ReadUInt16();
            MinorRuntimeVersion = reader.ReadUInt16();
            _metadataDirectory = DataDirectory.FromReader(reader);
            Flags = (DotNetDirectoryFlags) reader.ReadUInt32();
            Entrypoint = reader.ReadUInt32();
            _resourcesDirectory = DataDirectory.FromReader(reader);
            _strongNameDirectory = DataDirectory.FromReader(reader);
            _codeManagerDirectory = DataDirectory.FromReader(reader);
            _vtableFixupsDirectory = DataDirectory.FromReader(reader);
            _exportsDirectory = DataDirectory.FromReader(reader);
            _nativeHeaderDirectory = DataDirectory.FromReader(reader);

        }

        /// <inheritdoc />
        protected override IReadableSegment GetMetadata()
        {
            if (!_metadataDirectory.IsPresentInPE)
                return null;
            
            var directoryReader = _peFile.CreateDataDirectoryReader(_metadataDirectory);
            return DataSegment.FromReader(directoryReader); // TODO: interpretation instead of raw contents.
        }

        /// <inheritdoc />
        protected override IReadableSegment GetResources()
        {
            if (!_resourcesDirectory.IsPresentInPE)
                return null;
            
            var directoryReader = _peFile.CreateDataDirectoryReader(_resourcesDirectory);
            return DataSegment.FromReader(directoryReader); // TODO: interpretation instead of raw contents.
        }

        /// <inheritdoc />
        protected override IReadableSegment GetStrongName()
        {
            if (!_strongNameDirectory.IsPresentInPE)
                return null;
            
            var directoryReader = _peFile.CreateDataDirectoryReader(_strongNameDirectory);
            return DataSegment.FromReader(directoryReader); // TODO: interpretation instead of raw contents.
        }

        /// <inheritdoc />
        protected override IReadableSegment GetCodeManagerTable()
        {
            if (!_codeManagerDirectory.IsPresentInPE)
                return null;
            
            var directoryReader = _peFile.CreateDataDirectoryReader(_codeManagerDirectory);
            return DataSegment.FromReader(directoryReader); // TODO: interpretation instead of raw contents.
        }

        /// <inheritdoc />
        protected override IReadableSegment GetVTableFixups()
        {
            if (!_vtableFixupsDirectory.IsPresentInPE)
                return null;
            
            var directoryReader = _peFile.CreateDataDirectoryReader(_vtableFixupsDirectory);
            return DataSegment.FromReader(directoryReader); // TODO: interpretation instead of raw contents.
        }

        /// <inheritdoc />
        protected override IReadableSegment GetExportAddressTable()
        {
            if (!_exportsDirectory.IsPresentInPE)
                return null;
            
            var directoryReader = _peFile.CreateDataDirectoryReader(_exportsDirectory);
            return DataSegment.FromReader(directoryReader); // TODO: interpretation instead of raw contents.
        }

        /// <inheritdoc />
        protected override IReadableSegment GetManagedNativeHeader()
        {
            if (!_nativeHeaderDirectory.IsPresentInPE)
                return null;
            
            var directoryReader = _peFile.CreateDataDirectoryReader(_nativeHeaderDirectory);
            return DataSegment.FromReader(directoryReader); // TODO: interpretation instead of raw contents.
        }
        
    }
}