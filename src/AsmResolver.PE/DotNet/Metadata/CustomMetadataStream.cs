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

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Represents a metadata stream with contents in a custom data format.
    /// </summary>
    public class CustomMetadataStream : IMetadataStream
    {
        /// <summary>
        /// Creates a new custom metadata stream.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="data">The raw contents of the stream.</param>
        public CustomMetadataStream(string name, byte[] data)
            : this(name, new DataSegment(data))
        {
        }

        /// <summary>
        /// Creates a new custom metadata stream.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="contents">The raw contents of the stream.</param>
        public CustomMetadataStream(string name, ISegment contents)
        {
            Name = name;
            Contents = contents;
        }

        /// <inheritdoc />
        public uint FileOffset => Contents.FileOffset;

        /// <inheritdoc />
        public uint Rva => Contents.Rva;

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <inheritdoc />
        public string Name
        {
            get;
            set;
        }

        /// <inheritdoc />
        public bool CanRead => Contents is IReadableSegment;

        /// <summary>
        /// Gets or sets the raw contents of the stream.
        /// </summary>
        public ISegment Contents
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IBinaryStreamReader CreateReader()
        {
            if (!CanRead)
                throw new InvalidOperationException("Contents of the metadata stream is not readable.");
            return ((IReadableSegment) Contents).CreateReader();
        }

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva) => Contents.UpdateOffsets(newFileOffset, newRva);

        /// <inheritdoc />
        public uint GetPhysicalSize() => Contents.GetPhysicalSize();

        /// <inheritdoc />
        public uint GetVirtualSize() => GetPhysicalSize();

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer) => Contents.Write(writer);
    }
}