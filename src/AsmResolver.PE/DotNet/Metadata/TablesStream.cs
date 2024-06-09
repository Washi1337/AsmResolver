using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Represents the metadata stream containing tables defining each member in a .NET assembly.
    /// </summary>
    public partial class TablesStream : SegmentBase, IMetadataStream
    {
        /// <summary>
        /// The default name of a table stream using the compressed format.
        /// </summary>
        public const string CompressedStreamName = "#~";

        /// <summary>
        /// The default name of a table stream using the Edit-and-Continue, uncompressed format.
        /// </summary>
        public const string EncStreamName = "#-";

        /// <summary>
        /// The default name of a table stream using the minimal format.
        /// </summary>
        public const string MinimalStreamName = "#JTD";

        /// <summary>
        /// The default name of a table stream using the uncompressed format.
        /// </summary>
        public const string UncompressedStreamName = "#Schema";

        private readonly Dictionary<CodedIndex, IndexEncoder> _indexEncoders;
        private readonly LazyVariable<TablesStream, IList<IMetadataTable?>> _tables;
        private readonly LazyVariable<TablesStream, IList<TableLayout>> _layouts;

        /// <summary>
        /// Creates a new, empty tables stream.
        /// </summary>
        public TablesStream()
        {
            _layouts = new LazyVariable<TablesStream, IList<TableLayout>>(x => x.GetTableLayouts());
            _tables = new LazyVariable<TablesStream, IList<IMetadataTable?>>(x => x.GetTables());
            _indexEncoders = CreateIndexEncoders();
        }

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
        }

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
        /// Gets or sets a value indicating each string index in the tables stream is a 4 byte integer instead of a
        /// 2 byte integer.
        /// </summary>
        public IndexSize StringIndexSize
        {
            get => GetStreamIndexSize(0);
            set => SetStreamIndexSize(0, value);
        }

        /// <summary>
        /// Gets or sets a value indicating each GUID index in the tables stream is a 4 byte integer instead of a
        /// 2 byte integer.
        /// </summary>
        public IndexSize GuidIndexSize
        {
            get => GetStreamIndexSize(1);
            set => SetStreamIndexSize(1, value);
        }

        /// <summary>
        /// Gets or sets a value indicating each blob index in the tables stream is a 4 byte integer instead of a
        /// 2 byte integer.
        /// </summary>
        public IndexSize BlobIndexSize
        {
            get => GetStreamIndexSize(2);
            set => SetStreamIndexSize(2, value);
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

        /// <summary>
        /// Gets a value indicating whether the tables stream is assigned with row counts that originate from an
        /// external .NET metadata file.
        /// </summary>
        /// <remarks>
        /// This value is typically set to <c>false</c>, except for Portable PDB metadata table streams.
        /// </remarks>
        [MemberNotNullWhen(true, nameof(ExternalRowCounts))]
        public bool HasExternalRowCounts => ExternalRowCounts is not null;

        /// <summary>
        /// Gets or sets an array of row counts originating from an external .NET metadata file that this table stream
        /// should consider when encoding indices.
        /// </summary>
        /// <remarks>
        /// This value is typically <c>null</c>, except for Portable PDB metadata table streams.
        /// </remarks>
        public uint[]? ExternalRowCounts
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value whether to force large columns in the tables stream.
        /// </summary>
        /// <remarks>
        /// This value is typically <c>false</c>, it is intended to be <c>true</c> for cases when
        /// EnC metadata is used and a stream with the <see cref="MinimalStreamName"/> name is present.
        /// </remarks>
        public bool ForceLargeColumns
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of all tables in the tables stream.
        /// </summary>
        /// <remarks>
        /// This collection always contains all tables, in the same order as <see cref="TableIndex"/> defines, regardless
        /// of whether a table actually has elements or not.
        /// </remarks>
        protected IList<IMetadataTable?> Tables => _tables.GetValue(this);

        /// <summary>
        /// Gets the layout of all tables in the stream.
        /// </summary>
        protected IList<TableLayout> TableLayouts => _layouts.GetValue(this);

        /// <inheritdoc />
        public virtual BinaryStreamReader CreateReader() => throw new NotSupportedException();

        /// <summary>
        /// Obtains the implied table row count for the provided table index.
        /// </summary>
        /// <param name="table">The table index.</param>
        /// <returns>The row count.</returns>
        /// <remarks>
        /// This method takes any external row counts from <see cref="ExternalRowCounts"/> into account.
        /// </remarks>
        public uint GetTableRowCount(TableIndex table)
        {
            return HasExternalRowCounts && (int) table < ExternalRowCounts.Length
                ? ExternalRowCounts[(int) table]
                : (uint) GetTable(table).Count;
        }

        /// <summary>
        /// Obtains the implied table index size for the provided table index.
        /// </summary>
        /// <param name="table">The table index.</param>
        /// <returns>The index size.</returns>
        /// <remarks>
        /// This method takes any external row counts from <see cref="ExternalRowCounts"/> into account.
        /// </remarks>
        public IndexSize GetTableIndexSize(TableIndex table)
        {
            if (ForceLargeColumns)
                return IndexSize.Long;

            return GetTableRowCount(table) > 0xFFFF
                ? IndexSize.Long
                : IndexSize.Short;
        }

        /// <summary>
        /// Updates the layouts of each metadata table, according to the <see cref="Flags"/> property.
        /// </summary>
        /// <remarks>
        /// This method should be called after updating any of the index sizes in any of the <see cref="Flags"/>,
        /// <see cref="StringIndexSize"/>, <see cref="BlobIndexSize"/> or <see cref="GuidIndexSize"/> properties.
        /// </remarks>
        protected void SynchronizeTableLayoutsWithFlags()
        {
            var layouts = GetTableLayouts();
            for (int i = 0; i < Tables.Count; i++)
                Tables[i]?.UpdateTableLayout(layouts[i]);
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            SynchronizeTableLayoutsWithFlags();
            ulong validBitmask = ComputeValidBitmask();
            return (uint) (sizeof(uint)
                           + 4 * sizeof(byte)
                           + sizeof(ulong)
                           + sizeof(ulong)
                           + GetTablesCount(validBitmask) * sizeof(uint)
                           + (HasExtraData ? sizeof(ulong) : 0)
                           + GetTablesSize(validBitmask))
                           + sizeof(uint);
        }

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            SynchronizeTableLayoutsWithFlags();

            writer.WriteUInt32(Reserved);
            writer.WriteByte(MajorVersion);
            writer.WriteByte(MinorVersion);
            writer.WriteByte((byte) Flags);
            writer.WriteByte(Log2LargestRid);
            ulong validBitmask = ComputeValidBitmask();
            writer.WriteUInt64(validBitmask);
            writer.WriteUInt64(ComputeSortedBitmask());

            WriteRowCounts(writer, validBitmask);

            if (HasExtraData)
                writer.WriteUInt32(ExtraData);

            WriteTables(writer, validBitmask);

            writer.WriteUInt32(0);
        }

        /// <summary>
        /// Computes the valid bitmask of the tables stream, which is a 64-bit integer where each bit specifies whether
        /// a table is present in the stream or not.
        /// </summary>
        /// <returns>The valid bitmask.</returns>
        protected virtual ulong ComputeValidBitmask()
        {
            // TODO: make more configurable (maybe add IMetadataTable.IsPresent property?).
            ulong result = 0;

            for (int i = 0; i < Tables.Count; i++)
            {
                if (Tables[i]?.Count > 0)
                    result |= 1UL << i;
            }

            return result;
        }

        /// <summary>
        /// Computes the sorted bitmask of the tables stream, which is a 64-bit integer where each bit specifies whether
        /// a table is sorted or not.
        /// </summary>
        /// <returns>The valid bitmask.</returns>
        protected virtual ulong ComputeSortedBitmask()
        {
            ulong result = 0;

            bool containsTypeSystemData = false;
            bool containsPdbData = false;

            // Determine which tables are marked as sorted.
            for (int i = 0; i < Tables.Count; i++)
            {
                if (Tables[i] is not { } table)
                    continue;

                if (table.IsSorted)
                    result |= 1UL << i;

                if (table.Count > 0)
                {
                    if (i <= (int) TableIndex.MaxTypeSystemTableIndex)
                        containsTypeSystemData = true;
                    else
                        containsPdbData = true;
                }
            }

            const ulong typeSystemMask = (1UL << (int) TableIndex.MaxTypeSystemTableIndex + 1) - 1;
            const ulong pdbMask = ((1UL << (int) TableIndex.Max) - 1) & ~typeSystemMask;

            // Backwards compatibility: Ensure that only the bits are set in the sorted mask if the metadata
            // actually contains the type system and/or pdb tables.
            if (!containsTypeSystemData)
                result &= ~typeSystemMask;
            if (!containsPdbData)
                result &= ~pdbMask;

            return result;
        }

        /// <summary>
        /// Gets a value indicating the total number of tables in the stream.
        /// </summary>
        /// <param name="validBitmask">The valid bitmask, indicating all present tables in the stream.</param>
        /// <returns>The number of tables.</returns>
        protected virtual int GetTablesCount(ulong validBitmask)
        {
            int count = 0;
            for (TableIndex i = 0; i < TableIndex.Max; i++)
            {
                if (HasTable(validBitmask, i))
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Computes the total amount of bytes that each table combined needs.
        /// </summary>
        /// <param name="validBitmask">The valid bitmask, indicating all present tables in the stream.</param>
        /// <returns>The total amount of bytes.</returns>
        protected virtual uint GetTablesSize(ulong validBitmask)
        {
            long size = 0;
            for (TableIndex i = 0; i < (TableIndex) Tables.Count; i++)
            {
                if (HasTable(validBitmask, i))
                {
                    var table = GetTable(i);
                    size += table.Count * table.Layout.RowSize;
                }
            }

            return (uint) size;
        }

        /// <summary>
        /// Writes for each present table the row count to the output stream.
        /// </summary>
        /// <param name="writer">The output stream to write to.</param>
        /// <param name="validBitmask">The valid bitmask, indicating all present tables in the stream.</param>
        protected virtual void WriteRowCounts(BinaryStreamWriter writer, ulong validBitmask)
        {
            for (TableIndex i = 0; i <= TableIndex.Max; i++)
            {
                if (HasTable(validBitmask, i))
                    writer.WriteInt32(GetTable(i).Count);
            }
        }

        /// <summary>
        /// Writes each present table to the output stream.
        /// </summary>
        /// <param name="writer">The output stream to write to.</param>
        /// <param name="validBitmask">The valid bitmask, indicating all present tables in the stream.</param>
        protected virtual void WriteTables(BinaryStreamWriter writer, ulong validBitmask)
        {
            for (TableIndex i = 0; i < (TableIndex) Tables.Count; i++)
            {
                if (HasTable(validBitmask, i))
                    GetTable(i).Write(writer);
            }
        }

        /// <summary>
        /// Determines whether a table is present in the tables stream, based on a valid bitmask.
        /// </summary>
        /// <param name="validMask">The valid bitmask, indicating all present tables in the stream.</param>
        /// <param name="table">The table to verify existence of.</param>
        /// <returns><c>true</c> if the table is present, <c>false</c> otherwise.</returns>
        protected static bool HasTable(ulong validMask, TableIndex table)
        {
            return ((validMask >> (int) table) & 1) != 0;
        }

        /// <summary>
        /// Determines whether a table is sorted in the tables stream, based on a sorted bitmask.
        /// </summary>
        /// <param name="sortedMask">The sorted bitmask, indicating all sorted tables in the stream.</param>
        /// <param name="table">The table to verify.</param>
        /// <returns><c>true</c> if the table is sorted, <c>false</c> otherwise.</returns>
        protected static bool IsSorted(ulong sortedMask, TableIndex table)
        {
            return ((sortedMask >> (int) table) & 1) != 0;
        }

        /// <summary>
        /// Gets a table by its table index.
        /// </summary>
        /// <param name="index">The table index.</param>
        /// <returns>The table.</returns>
        public virtual IMetadataTable GetTable(TableIndex index) =>
            Tables[(int) index] ?? throw new ArgumentOutOfRangeException(nameof(index));

        /// <summary>
        /// Gets a table by its row type.
        /// </summary>
        /// <typeparam name="TRow">The type of rows the table stores.</typeparam>
        /// <returns>The table.</returns>
        public virtual MetadataTable<TRow> GetTable<TRow>()
            where TRow : struct, IMetadataRow
        {
            return Tables.OfType<MetadataTable<TRow>>().First();
        }

        /// <summary>
        /// Gets a table by its row type.
        /// </summary>
        /// <typeparam name="TRow">The type of rows the table stores.</typeparam>
        /// <param name="index">The table index.</param>
        /// <returns>The table.</returns>
        public virtual MetadataTable<TRow> GetTable<TRow>(TableIndex index)
            where TRow : struct, IMetadataRow
        {
            return (MetadataTable<TRow>) (Tables[(int) index] ?? throw new ArgumentOutOfRangeException(nameof(index)));
        }

        private IndexSize GetStreamIndexSize(int bitIndex)
        {
            if (ForceLargeColumns)
                return IndexSize.Long;
            return (IndexSize)(((((int)Flags >> bitIndex) & 1) + 1) * 2);
        }

        private void SetStreamIndexSize(int bitIndex, IndexSize newSize)
        {
            Flags = (TablesStreamFlags) (((int) Flags & ~(1 << bitIndex))
                                         | (newSize == IndexSize.Long ? 1 << bitIndex : 0));
        }

        /// <summary>
        /// Gets a value indicating the size of a column in a table.
        /// </summary>
        /// <param name="columnType">The column type to verify.</param>
        /// <returns>The column size.</returns>
        protected virtual uint GetColumnSize(ColumnType columnType)
        {
            if (_layouts.IsInitialized)
            {
                switch (columnType)
                {
                    case <= ColumnType.CustomDebugInformation:
                        return (uint) GetTableIndexSize((TableIndex) columnType);
                    case <= ColumnType.HasCustomDebugInformation:
                        return (uint) GetIndexEncoder((CodedIndex) columnType).IndexSize;
                }
            }

            return columnType switch
            {
                ColumnType.Blob => (uint) BlobIndexSize,
                ColumnType.String => (uint) StringIndexSize,
                ColumnType.Guid => (uint) GuidIndexSize,
                ColumnType.Byte => sizeof(byte),
                ColumnType.UInt16 => sizeof(ushort),
                ColumnType.UInt32 => sizeof(uint),
                _ => sizeof(uint)
            };
        }

        /// <summary>
        /// Gets an encoder/decoder for a particular coded index.
        /// </summary>
        /// <param name="index">The type of coded index to encode/decode.</param>
        /// <returns>The encoder.</returns>
        public IndexEncoder GetIndexEncoder(CodedIndex index) => _indexEncoders[index];

        /// <summary>
        /// Gets the range of metadata tokens referencing fields that a type defines.
        /// </summary>
        /// <param name="typeDefRid">The row identifier of the type definition to obtain the fields from.</param>
        /// <returns>The range of metadata tokens.</returns>
        public MetadataRange GetFieldRange(uint typeDefRid) =>
            GetMemberRange<TypeDefinitionRow>(TableIndex.TypeDef, typeDefRid, 4,
                TableIndex.Field, TableIndex.FieldPtr);

        /// <summary>
        /// Gets the range of metadata tokens referencing methods that a type defines.
        /// </summary>
        /// <param name="typeDefRid">The row identifier of the type definition to obtain the methods from.</param>
        /// <returns>The range of metadata tokens.</returns>
        public MetadataRange GetMethodRange(uint typeDefRid) =>
            GetMemberRange<TypeDefinitionRow>(TableIndex.TypeDef, typeDefRid, 5,
                TableIndex.Method, TableIndex.MethodPtr);

        /// <summary>
        /// Gets the range of metadata tokens referencing parameters that a method defines.
        /// </summary>
        /// <param name="methodDefRid">The row identifier of the method definition to obtain the parameters from.</param>
        /// <returns>The range of metadata tokens.</returns>
        public MetadataRange GetParameterRange(uint methodDefRid) =>
            GetMemberRange<MethodDefinitionRow>(TableIndex.Method, methodDefRid, 5,
                TableIndex.Param, TableIndex.ParamPtr);

        /// <summary>
        /// Gets the range of metadata tokens referencing properties that a property map row defines.
        /// </summary>
        /// <param name="propertyMapRid">The row identifier of the property map to obtain the properties from.</param>
        /// <returns>The range of metadata tokens.</returns>
        public MetadataRange GetPropertyRange(uint propertyMapRid) =>
            GetMemberRange<PropertyMapRow>(TableIndex.PropertyMap, propertyMapRid, 1,
                TableIndex.Property, TableIndex.PropertyPtr);

        /// <summary>
        /// Gets the range of metadata tokens referencing events that a event map row defines.
        /// </summary>
        /// <param name="eventMapRid">The row identifier of the event map to obtain the events from.</param>
        /// <returns>The range of metadata tokens.</returns>
        public MetadataRange GetEventRange(uint eventMapRid) =>
            GetMemberRange<EventMapRow>(TableIndex.EventMap, eventMapRid, 1,
                TableIndex.Event, TableIndex.EventPtr);

        private MetadataRange GetMemberRange<TOwnerRow>(
            TableIndex ownerTableIndex,
            uint ownerRid,
            int ownerColumnIndex,
            TableIndex memberTableIndex,
            TableIndex redirectTableIndex)
            where TOwnerRow : struct, IMetadataRow
        {
            int index = (int) (ownerRid - 1);

            // Check if valid owner RID.
            var ownerTable = GetTable<TOwnerRow>(ownerTableIndex);
            if (index < 0 || index >= ownerTable.Count)
                return MetadataRange.Empty;

            // Obtain boundaries.
            uint startRid = ownerTable[index][ownerColumnIndex];
            uint endRid = index < ownerTable.Count - 1
                ? ownerTable[index + 1][ownerColumnIndex]
                : (uint) GetTable(memberTableIndex).Count + 1;

            // Check if redirect table is present.
            var redirectTable = GetTable(redirectTableIndex);
            if (redirectTable.Count > 0)
                return new MetadataRange(redirectTable, memberTableIndex, startRid, endRid);

            // If not, its a simple range.
            return new MetadataRange(memberTableIndex, startRid, endRid);
        }
    }
}
