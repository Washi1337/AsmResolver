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
using AsmResolver.Lazy;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides an implementation of a lazy-initialized list of metadata streams present in a metadata directory.
    /// </summary>
    public class MetadataStreamList : LazyList<IMetadataStream>
    {
        private readonly IBinaryStreamReader _metadataReader;
        private readonly IBinaryStreamReader _entriesReader;
        private readonly int _numberOfStreams;
        private readonly IMetadataStreamReader _metadataStreamReader;

        /// <summary>
        /// Prepares a new lazy-initialized metadata stream list.
        /// </summary>
        /// <param name="metadataReader">The input stream containing the metadata directory.</param>
        /// <param name="reader">The input stream containing the metadata stream entries.</param>
        /// <param name="numberOfStreams">The number of streams.</param>
        /// <param name="metadataStreamReader"></param>
        public MetadataStreamList(IBinaryStreamReader metadataReader, IBinaryStreamReader reader, int numberOfStreams, 
            IMetadataStreamReader metadataStreamReader)
        {
            _metadataReader = metadataReader ?? throw new ArgumentNullException(nameof(metadataReader));
            _entriesReader = reader ?? throw new ArgumentNullException(nameof(reader));
            _numberOfStreams = numberOfStreams;
            _metadataStreamReader = metadataStreamReader;
        }

        /// <inheritdoc />
        public override int Count => IsInitialized ? Items.Count : _numberOfStreams; 

        /// <inheritdoc />
        protected override void Initialize()
        {
            var headers = new MetadataStreamHeader[_numberOfStreams];
            for (int i = 0; i < _numberOfStreams; i++)
                headers[i] = MetadataStreamHeader.FromReader(_entriesReader);

            for (int i = 0; i < _numberOfStreams; i++)
            {
                var header = headers[i];
                var streamReader = _metadataReader.Fork(_metadataReader.FileOffset + header.Offset, headers[i].Size);
                Items.Add(_metadataStreamReader.ReadStream(header, streamReader));
            }
        }
        
    }
}