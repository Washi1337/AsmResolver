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
using System.Text;

namespace AsmResolver.PE.DotNet.Metadata
{
    public class SerializedMetadata : Metadata
    {
        private readonly IBinaryStreamReader _streamEntriesReader;
        private readonly IBinaryStreamReader _streamContentsReader;
        private readonly int _numberOfStreams;

        public SerializedMetadata(IBinaryStreamReader reader)
        {
            if (reader == null) 
                throw new ArgumentNullException(nameof(reader));

            _streamContentsReader = reader.Fork();
            
            var signature = (MetadataSignature) reader.ReadUInt32();
            switch (signature)
            {
                case MetadataSignature.Bsjb:
                    // BSJB header is the default header.
                    break;
                case MetadataSignature.Moc:
                    throw new NotSupportedException("Old +MOC metadata header format is not supported.");
                default:
                    throw new BadImageFormatException($"Invalid metadata header ({(uint) signature:X8}).");
            }

            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            Reserved = reader.ReadUInt32();

            int versionLength = reader.ReadInt32();
            if (!reader.CanRead(versionLength))
                throw new BadImageFormatException($"Invalid version length in metadata header ({versionLength} characters).");

            var versionBytes = new byte[versionLength];
            reader.ReadBytes(versionBytes, 0, versionBytes.Length);
            VersionString = Encoding.ASCII.GetString(versionBytes);

            Flags = reader.ReadUInt16();
            _numberOfStreams = reader.ReadInt16();
            _streamEntriesReader = reader.Fork();
        }

        protected override IList<IMetadataStream> GetStreams()
        {
            if (_numberOfStreams == 0)
                return base.GetStreams();
            
            return new MetadataStreamList(
                _streamContentsReader.Fork(),
                _streamEntriesReader.Fork(),
                _numberOfStreams);
        }

    }
}