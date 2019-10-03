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
        private const int MaxTableCount = (int) MetadataTableIndex.GenericParamConstraint;

//        private IList<IMetadataTable> _tables = new List<IMetadataTable>(MaxTableCount);
        
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
        } = 2;

        /// <summary>
        /// Gets or sets the minor version number of the schema.
        /// </summary>
        public byte MinorVersion
        {
            get;
            set;
        } = 0;

        /// <summary>
        /// Gets or sets the flags of the tables stream.
        /// </summary>
        public TablesStreamFlags Flags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating each string index in the tables stream is a 4 byte integer instead of a
        /// 2 byte integer.
        /// </summary>
        public bool HasLongStringIndices
        {
            get => (Flags & TablesStreamFlags.LongStringIndices) != 0;
            set => Flags = (Flags & ~TablesStreamFlags.LongStringIndices)
                           | (value ? TablesStreamFlags.LongStringIndices : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating each GUID index in the tables stream is a 4 byte integer instead of a
        /// 2 byte integer.
        /// </summary>
        public bool HasLongGuidIndices
        {
            get => (Flags & TablesStreamFlags.LongGuidIndices) != 0;
            set => Flags = (Flags & ~TablesStreamFlags.LongGuidIndices)
                           | (value ? TablesStreamFlags.LongGuidIndices : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating each blob index in the tables stream is a 4 byte integer instead of a
        /// 2 byte integer.
        /// </summary>
        public bool HasLongBlobIndices
        {
            get => (Flags & TablesStreamFlags.LongBlobIndices) != 0;
            set => Flags = (Flags & ~TablesStreamFlags.LongBlobIndices)
                           | (value ? TablesStreamFlags.LongBlobIndices : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the tables were created with an extra bit in columns.
        /// </summary>
        public bool HasPaddingBit
        {
            get => (Flags & TablesStreamFlags.LongBlobIndices) != 0;
            set => Flags = (Flags & ~TablesStreamFlags.LongBlobIndices)
                           | (value ? TablesStreamFlags.LongBlobIndices : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the tables stream contains only deltas.
        /// </summary>
        public bool HasDeltaOnly
        {
            get => (Flags & TablesStreamFlags.DeltaOnly) != 0;
            set => Flags = (Flags & ~TablesStreamFlags.DeltaOnly)
                           | (value ? TablesStreamFlags.DeltaOnly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the tables stream persists an extra 4 bytes of data.
        /// </summary>
        public bool HasExtraData
        {
            get => (Flags & TablesStreamFlags.ExtraData) != 0;
            set => Flags = (Flags & ~TablesStreamFlags.ExtraData)
                           | (value ? TablesStreamFlags.ExtraData : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the tables stream may contain _Delete tokens.
        /// This only occurs in ENC metadata.
        /// </summary>
        public bool HasDeletedTokens
        {
            get => (Flags & TablesStreamFlags.HasDelete) != 0;
            set => Flags = (Flags & ~TablesStreamFlags.HasDelete)
                           | (value ? TablesStreamFlags.HasDelete : 0);
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

        /// <summary>
        /// Gets or sets the extra 4 bytes data that is persisted after the row counts of the tables stream.
        /// </summary>
        /// <remarks>
        /// This value is not specified by the ECMA-335 and is only present when <see cref="HasExtraData"/> is
        /// set to <c>true</c>. Writing to this value does not automatically update <see cref="HasExtraData"/>,
        /// and is only persisted in the final output if <see cref="HasExtraData"/> is set to <c>true</c>.
        /// </remarks>
        public uint ExtraData
        {
            get;
            set;
        }

        /// <inheritdoc />
        public virtual IBinaryStreamReader CreateReader() => throw new NotSupportedException();
        
    }
}