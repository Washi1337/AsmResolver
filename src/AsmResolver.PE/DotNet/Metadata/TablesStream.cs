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
    /// Represents the metadata stream containing tables defining each member in a .NET assembly. 
    /// </summary>
    public class TablesStream : IMetadataStream 
    {
        public const string CompressedStreamName = "#~";
        public const string EncStreamName = "#-";
        public const string MinimalStreamName = "#JTD";
        public const string UncompressedStreamName = "#Schema";

        /// <inheritdoc />
        public string Name
        {
            get;
            set;
        } = CompressedStreamName;
        
        /// <inheritdoc />
        public virtual bool CanRead => false;

        /// <summary>
        /// Reserved, for future use.
        /// </summary>
        /// <remarks>
        /// This field must be set to 0 by the CoreCLR implementation of the runtime. 
        /// </remarks>
        public uint Reserved
        {
            get;
            set;
        } = 0;

        /// <summary>
        /// Gets or sets the major version number of the schema.
        /// </summary>
        public byte MajorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor version number of the schema.
        /// </summary>
        public byte MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the flags of the tables stream.
        /// </summary>
        public TablesStreamFlags Flags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the bit-length of the largest relative identifier (RID) in the table stream.
        /// </summary>
        /// <remarks>
        /// This value is ignored by the CoreCLR implementation of the runtime, and the standard compilers always set
        /// this value to 1. 
        /// </remarks>
        public byte Log2LargestRid
        {
            get;
            protected set;
        } = 1;
        
        // TODO: tables.

        /// <inheritdoc />
        public virtual IBinaryStreamReader CreateReader()
        {
            throw new NotSupportedException();
        }
        
    }
}