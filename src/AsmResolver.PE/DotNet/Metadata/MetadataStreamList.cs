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
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Guid;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.UserStrings;

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
        private readonly ISegmentReferenceResolver _referenceResolver;

        /// <summary>
        /// Prepares a new lazy-initialized metadata stream list.
        /// </summary>
        /// <param name="metadataReader">The input stream containing the metadata directory.</param>
        /// <param name="reader">The input stream containing the metadata stream entries.</param>
        /// <param name="numberOfStreams">The number of streams.</param>
        /// <param name="referenceResolver">The instance to use for resolving virtual addresses to other segments in the file.</param>
        public MetadataStreamList(IBinaryStreamReader metadataReader, IBinaryStreamReader reader, int numberOfStreams, 
            ISegmentReferenceResolver referenceResolver)
        {
            _metadataReader = metadataReader ?? throw new ArgumentNullException(nameof(metadataReader));
            _entriesReader = reader ?? throw new ArgumentNullException(nameof(reader));
            _numberOfStreams = numberOfStreams;
            _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
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
                Items.Add(ReadStream(header, streamReader));
            }
        }

        /// <summary>
        /// Reads a single stream from the metadata directory.
        /// </summary>
        /// <param name="header">The header of the stream to read.</param>
        /// <param name="reader">The reader that spans the contents of the stream.</param>
        /// <returns>The stream.</returns>
        /// <remarks>
        /// This method is called for every metadata stream header upon initializing the list.
        /// </remarks>
        protected virtual IMetadataStream ReadStream(MetadataStreamHeader header, IBinaryStreamReader reader)
        {
            switch (header.Name)
            {
                case TablesStream.CompressedStreamName:
                case TablesStream.EncStreamName:
                    return new SerializedTableStream(header.Name,DataSegment.FromReader(reader), _referenceResolver);
                    
                case StringsStream.DefaultName:
                    return new SerializedStringsStream(header.Name, DataSegment.FromReader(reader));

                case UserStringsStream.DefaultName:
                    return new SerializedUserStringsStream(header.Name, DataSegment.FromReader(reader));

                case BlobStream.DefaultName:
                    return new SerializedBlobStream(header.Name, DataSegment.FromReader(reader));

                case GuidStream.DefaultName:
                    return new SerializedGuidStream(header.Name, DataSegment.FromReader(reader));

                default:
                    return new CustomMetadataStream(header.Name, DataSegment.FromReader(reader));
            }
        }
        
    }
}