using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents the metadata stream containing tables defining each member in a .NET assembly. 
    /// </summary>
    public class TablesStream : IMetadataStream
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
        
        private const int MaxTableCount = (int) TableIndex.GenericParamConstraint;

        private readonly IDictionary<CodedIndex, IndexEncoder> _indexEncoders;
        private readonly LazyVariable<IList<IMetadataTable>> _tables;
        private readonly LazyVariable<IList<TableLayout>> _layouts;

        /// <summary>
        /// Creates a new, empty tables stream.
        /// </summary>
        public TablesStream()
        {
            _layouts = new LazyVariable<IList<TableLayout>>(GetTableLayouts);
            _tables = new LazyVariable<IList<IMetadataTable>>(GetTables);
            _indexEncoders = CreateIndexEncoders();
        }

        /// <inheritdoc />
        public uint FileOffset
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public uint Rva
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

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
        /// Gets a collection of all tables in the tables stream.
        /// </summary>
        /// <remarks>
        /// This collection always contains all tables, in the same order as <see cref="TableIndex"/> defines, regardless
        /// of whether a table actually has elements or not.
        /// </remarks>
        protected IList<IMetadataTable> Tables => _tables.Value;

        /// <summary>
        /// Gets the layout of all tables in the stream.
        /// </summary>
        protected IList<TableLayout> TableLayouts => _layouts.Value;
        
        /// <inheritdoc />
        public virtual IBinaryStreamReader CreateReader() => throw new NotSupportedException();

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            FileOffset = newFileOffset;
            Rva = newRva;
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
                Tables[i].UpdateTableLayout(layouts[i]);
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
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
        public uint GetVirtualSize() => GetPhysicalSize();

        /// <inheritdoc />
        public virtual void Write(IBinaryStreamWriter writer)
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
                if (Tables[i].Count > 0)
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
            // TODO: make more configurable (maybe add IMetadataTable.IsSorted property?).
            
            return 0x000016003301FA00;
        }

        /// <summary>
        /// Gets a value indicating the total number of tables in the stream.
        /// </summary>
        /// <param name="validBitmask">The valid bitmask, indicating all present tables in the stream.</param>
        /// <returns>The number of tables.</returns>
        protected virtual int GetTablesCount(ulong validBitmask)
        {
            int count = 0;
            for (TableIndex i = 0; i < (TableIndex) Tables.Count; i++)
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
        protected virtual void WriteRowCounts(IBinaryStreamWriter writer, ulong validBitmask)
        {
            for (TableIndex i = 0; i < (TableIndex) Tables.Count; i++)
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
        protected virtual void WriteTables(IBinaryStreamWriter writer, ulong validBitmask)
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
        /// Obtains the collection of tables in the tables stream.
        /// </summary>
        /// <returns>The tables, including empty tables if there are any.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Tables"/> property.
        /// </remarks>
        protected virtual IList<IMetadataTable> GetTables()
        {
            var layouts = TableLayouts;
            return new IMetadataTable[]
            {
                new MetadataTable<ModuleDefinitionRow>(TableIndex.Module, layouts[0]), 
                new MetadataTable<TypeReferenceRow>(TableIndex.TypeRef, layouts[1]), 
                new MetadataTable<TypeDefinitionRow>(TableIndex.TypeDef, layouts[2]), 
                new MetadataTable<FieldPointerRow>(TableIndex.FieldPtr, layouts[3]), 
                new MetadataTable<FieldDefinitionRow>(TableIndex.Field, layouts[4]), 
                new MetadataTable<MethodPointerRow>(TableIndex.Method, layouts[5]), 
                new MetadataTable<MethodDefinitionRow>(TableIndex.Method, layouts[6]), 
                new MetadataTable<ParameterPointerRow>(TableIndex.ParamPtr, layouts[7]), 
                new MetadataTable<ParameterDefinitionRow>(TableIndex.Param, layouts[8]), 
                new MetadataTable<InterfaceImplementationRow>(TableIndex.InterfaceImpl, layouts[9]), 
                new MetadataTable<MemberReferenceRow>(TableIndex.MemberRef, layouts[10]), 
                new MetadataTable<ConstantRow>(TableIndex.Constant, layouts[11]), 
                new MetadataTable<CustomAttributeRow>(TableIndex.CustomAttribute, layouts[12]), 
                new MetadataTable<FieldMarshalRow>(TableIndex.FieldMarshal, layouts[13]), 
                new MetadataTable<SecurityDeclarationRow>(TableIndex.DeclSecurity, layouts[14]), 
                new MetadataTable<ClassLayoutRow>(TableIndex.ClassLayout, layouts[15]), 
                new MetadataTable<FieldLayoutRow>(TableIndex.FieldLayout, layouts[16]), 
                new MetadataTable<StandAloneSignatureRow>(TableIndex.StandAloneSig, layouts[17]), 
                new MetadataTable<EventMapRow>(TableIndex.EventMap, layouts[18]), 
                new MetadataTable<EventPointerRow>(TableIndex.EventPtr, layouts[19]),
                new MetadataTable<EventDefinitionRow>(TableIndex.Event, layouts[20]), 
                new MetadataTable<PropertyMapRow>(TableIndex.PropertyMap, layouts[21]), 
                new MetadataTable<PropertyPointerRow>(TableIndex.PropertyPtr, layouts[22]), 
                new MetadataTable<PropertyDefinitionRow>(TableIndex.Property, layouts[23]), 
                new MetadataTable<MethodSemanticsRow>(TableIndex.MethodSemantics, layouts[24]), 
                new MetadataTable<MethodImplementationRow>(TableIndex.MethodImpl, layouts[25]), 
                new MetadataTable<ModuleReferenceRow>(TableIndex.ModuleRef, layouts[26]), 
                new MetadataTable<TypeSpecificationRow>(TableIndex.TypeSpec, layouts[27]), 
                new MetadataTable<ImplementationMapRow>(TableIndex.ImplMap, layouts[28]), 
                new MetadataTable<FieldRvaRow>(TableIndex.FieldRva, layouts[29]), 
                new MetadataTable<EncLogRow>(TableIndex.EncLog, layouts[30]), 
                new MetadataTable<EncMapRow>(TableIndex.EncMap, layouts[31]), 
                new MetadataTable<AssemblyDefinitionRow>(TableIndex.Assembly, layouts[32]), 
                new MetadataTable<AssemblyProcessorRow>(TableIndex.AssemblyProcessor, layouts[33]), 
                new MetadataTable<AssemblyOSRow>(TableIndex.AssemblyOS, layouts[34]), 
                new MetadataTable<AssemblyReferenceRow>(TableIndex.AssemblyRef, layouts[35]), 
                new MetadataTable<AssemblyRefProcessorRow>(TableIndex.AssemblyRefProcessor, layouts[36]), 
                new MetadataTable<AssemblyRefOSRow>(TableIndex.AssemblyRefProcessor, layouts[37]), 
                new MetadataTable<FileReferenceRow>(TableIndex.File, layouts[38]), 
                new MetadataTable<ExportedTypeRow>(TableIndex.ExportedType, layouts[39]), 
                new MetadataTable<ManifestResourceRow>(TableIndex.ManifestResource, layouts[40]),
                new MetadataTable<NestedClassRow>(TableIndex.NestedClass, layouts[41]), 
                new MetadataTable<GenericParameterRow>(TableIndex.GenericParam, layouts[42]), 
                new MetadataTable<MethodSpecificationRow>(TableIndex.MethodSpec, layouts[43]), 
                new MetadataTable<GenericParameterConstraintRow>(TableIndex.GenericParamConstraint, layouts[44]), 
            };
        }

        private Dictionary<CodedIndex, IndexEncoder> CreateIndexEncoders()
        {
            return new Dictionary<CodedIndex, IndexEncoder>
            {
                [CodedIndex.TypeDefOrRef] = new IndexEncoder(this,
                    TableIndex.TypeDef, TableIndex.TypeRef, TableIndex.TypeSpec),
                [CodedIndex.HasConstant] = new IndexEncoder(this,
                    TableIndex.Field, TableIndex.Param, TableIndex.Property),
                [CodedIndex.HasCustomAttribute] = new IndexEncoder(this,
                    TableIndex.Method, TableIndex.Field, TableIndex.TypeRef, TableIndex.TypeDef,
                    TableIndex.Param, TableIndex.InterfaceImpl, TableIndex.MemberRef, TableIndex.Module,
                    TableIndex.DeclSecurity, TableIndex.Property, TableIndex.Event, TableIndex.StandAloneSig,
                    TableIndex.ModuleRef, TableIndex.TypeSpec, TableIndex.Assembly, TableIndex.AssemblyRef,
                    TableIndex.File, TableIndex.ExportedType, TableIndex.ManifestResource, TableIndex.GenericParam,
                    TableIndex.GenericParamConstraint, TableIndex.MethodSpec),
                [CodedIndex.HasFieldMarshal] = new IndexEncoder(this,
                    TableIndex.Field, TableIndex.Param),
                [CodedIndex.HasDeclSecurity] = new IndexEncoder(this,
                    TableIndex.TypeDef, TableIndex.Method, TableIndex.Assembly),
                [CodedIndex.MemberRefParent] = new IndexEncoder(this,
                    TableIndex.TypeDef, TableIndex.TypeRef, TableIndex.ModuleRef,
                    TableIndex.Method, TableIndex.TypeSpec),
                [CodedIndex.HasSemantics] = new IndexEncoder(this,
                    TableIndex.Event, TableIndex.Property),
                [CodedIndex.MethodDefOrRef] = new IndexEncoder(this,
                    TableIndex.Method, TableIndex.MemberRef),
                [CodedIndex.MemberForwarded] = new IndexEncoder(this,
                    TableIndex.Field, TableIndex.Method),
                [CodedIndex.Implementation] = new IndexEncoder(this,
                    TableIndex.File, TableIndex.AssemblyRef, TableIndex.ExportedType),
                [CodedIndex.CustomAttributeType] = new IndexEncoder(this,
                    0, 0, TableIndex.Method, TableIndex.MemberRef, 0),
                [CodedIndex.ResolutionScope] = new IndexEncoder(this,
                    TableIndex.Module, TableIndex.ModuleRef, TableIndex.AssemblyRef, TableIndex.TypeRef),
                [CodedIndex.TypeOrMethodDef] = new IndexEncoder(this,
                    TableIndex.TypeDef, TableIndex.Method)
            };
        }
        
        /// <summary>
        /// Gets a table by its table index. 
        /// </summary>
        /// <param name="index">The table index.</param>
        /// <returns>The table.</returns>
        public virtual IMetadataTable GetTable(TableIndex index) => Tables[(int) index];

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
            return (MetadataTable<TRow>) Tables[(int) index];
        }

        private IndexSize GetStreamIndexSize(int bitIndex) => (IndexSize) (((((int) Flags >> bitIndex) & 1) + 1) * 2);

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
                if (columnType <= ColumnType.GenericParamConstraint)
                    return (uint) Tables[(int) columnType].IndexSize;
                if (columnType <= ColumnType.TypeOrMethodDef)
                    return (uint) GetIndexEncoder((CodedIndex) columnType).IndexSize;
            }

            switch (columnType)
            {
                case ColumnType.Blob:
                    return (uint) BlobIndexSize;
                case ColumnType.String:
                    return (uint) StringIndexSize;
                case ColumnType.Guid:
                    return (uint) GuidIndexSize;
                case ColumnType.Byte:
                    return sizeof(byte);
                case ColumnType.UInt16:
                    return sizeof(ushort);
                case ColumnType.UInt32:
                    return sizeof(uint);
                default:
                    return sizeof(uint);
            }
        }

        /// <summary>
        /// Gets an encoder/decoder for a particular coded index.
        /// </summary>
        /// <param name="index">The type of coded index to encode/decode.</param>
        /// <returns>The encoder.</returns>
        public IndexEncoder GetIndexEncoder(CodedIndex index) => _indexEncoders[index];

        /// <summary>
        /// Gets an ordered collection of the current table layouts.
        /// </summary>
        /// <returns>The table layouts.</returns>
        protected TableLayout[] GetTableLayouts()
        {
            var result = new[]
            {
                new TableLayout(
                    new ColumnLayout("Generation", ColumnType.UInt16),
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                    new ColumnLayout("Mvid", ColumnType.Guid, GuidIndexSize),
                    new ColumnLayout("EncId", ColumnType.Guid, GuidIndexSize),
                    new ColumnLayout("EncBaseId", ColumnType.Guid, GuidIndexSize)),
                new TableLayout(
                    new ColumnLayout("ResolutionScope", ColumnType.ResolutionScope,
                        GetColumnSize(ColumnType.ResolutionScope)),
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                    new ColumnLayout("Namespace", ColumnType.Guid, StringIndexSize)),
                new TableLayout(
                    new ColumnLayout("Flags", ColumnType.UInt32),
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                    new ColumnLayout("Namespace", ColumnType.String, StringIndexSize),
                    new ColumnLayout("Extends", ColumnType.TypeDefOrRef,
                        GetColumnSize(ColumnType.TypeDefOrRef)),
                    new ColumnLayout("FieldList", ColumnType.Field, GetColumnSize(ColumnType.Field)),
                    new ColumnLayout("MethodList", ColumnType.Method, GetColumnSize(ColumnType.Method))),
                new TableLayout(
                    new ColumnLayout("Field", ColumnType.Field, GetColumnSize(ColumnType.Field))),
                new TableLayout(
                    new ColumnLayout("Flags", ColumnType.UInt16),
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                    new ColumnLayout("Signature", ColumnType.Blob, BlobIndexSize)),
                new TableLayout(
                    new ColumnLayout("Method", ColumnType.Method, GetColumnSize(ColumnType.Method))),
                new TableLayout(
                    new ColumnLayout("RVA", ColumnType.UInt32),
                    new ColumnLayout("ImplFlags", ColumnType.UInt16),
                    new ColumnLayout("Flags", ColumnType.UInt16),
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                    new ColumnLayout("Signature", ColumnType.Blob, BlobIndexSize),
                    new ColumnLayout("ParamList", ColumnType.Param, GetColumnSize(ColumnType.Param))),
                new TableLayout(
                    new ColumnLayout("Parameter", ColumnType.Param, GetColumnSize(ColumnType.Param))),
                new TableLayout(
                    new ColumnLayout("Flags", ColumnType.UInt16),
                    new ColumnLayout("Sequence", ColumnType.UInt16),
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize)),
                new TableLayout(
                    new ColumnLayout("Class", ColumnType.TypeDef, GetColumnSize(ColumnType.TypeDef)),
                    new ColumnLayout("Interface", ColumnType.TypeDefOrRef, GetColumnSize(ColumnType.TypeDefOrRef))),
                new TableLayout(
                    new ColumnLayout("Parent", ColumnType.MemberRefParent, GetColumnSize(ColumnType.MemberRefParent)),
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                    new ColumnLayout("Signature", ColumnType.Blob, BlobIndexSize)),
                new TableLayout(
                    new ColumnLayout("Type", ColumnType.Byte),
                    new ColumnLayout("Padding", ColumnType.Byte),
                    new ColumnLayout("Parent", ColumnType.HasConstant, GetColumnSize(ColumnType.HasConstant)),
                    new ColumnLayout("Value", ColumnType.Blob, BlobIndexSize)), 
                new TableLayout(
                    new ColumnLayout("Parent", ColumnType.HasCustomAttribute, GetColumnSize(ColumnType.HasCustomAttribute)),
                    new ColumnLayout("Type", ColumnType.CustomAttributeType, GetColumnSize(ColumnType.CustomAttributeType)),
                    new ColumnLayout("Value", ColumnType.Blob, BlobIndexSize)),
                new TableLayout(
                    new ColumnLayout("Parent", ColumnType.HasFieldMarshal, GetColumnSize(ColumnType.HasFieldMarshal)),
                    new ColumnLayout("NativeType", ColumnType.Blob, BlobIndexSize)),
                new TableLayout(
                    new ColumnLayout("Action", ColumnType.UInt16),
                    new ColumnLayout("Parent", ColumnType.HasDeclSecurity, GetColumnSize(ColumnType.HasDeclSecurity)),
                    new ColumnLayout("PermissionSet", ColumnType.Blob, BlobIndexSize)),
                new TableLayout(
                    new ColumnLayout("PackingSize", ColumnType.UInt16),
                    new ColumnLayout("ClassSize", ColumnType.UInt32),
                    new ColumnLayout("Parent", ColumnType.TypeDef, GetColumnSize(ColumnType.TypeDef))),
                new TableLayout(
                    new ColumnLayout("Offset", ColumnType.UInt32),
                    new ColumnLayout("Field", ColumnType.TypeDef, GetColumnSize(ColumnType.Field))),
                new TableLayout(
                    new ColumnLayout("Signature", ColumnType.Blob, BlobIndexSize)),
                new TableLayout(
                    new ColumnLayout("Parent", ColumnType.TypeDef, GetColumnSize(ColumnType.TypeDef)),
                    new ColumnLayout("EventList", ColumnType.Event, GetColumnSize(ColumnType.Event))),
                new TableLayout(
                    new ColumnLayout("Event", ColumnType.Event, GetColumnSize(ColumnType.Event))),
                new TableLayout(
                    new ColumnLayout("Flags", ColumnType.UInt16),
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                    new ColumnLayout("EventType", ColumnType.TypeDefOrRef, GetColumnSize(ColumnType.TypeDefOrRef))),
                new TableLayout(
                    new ColumnLayout("Parent", ColumnType.TypeDef, GetColumnSize(ColumnType.TypeDef)),
                    new ColumnLayout("PropertyList", ColumnType.Event, GetColumnSize(ColumnType.Property))),
                new TableLayout(
                    new ColumnLayout("Property", ColumnType.Property, GetColumnSize(ColumnType.Property))),
                new TableLayout(
                    new ColumnLayout("Flags", ColumnType.UInt16),
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                    new ColumnLayout("PropertyType", ColumnType.Blob, BlobIndexSize)),
                new TableLayout(
                    new ColumnLayout("Semantic", ColumnType.UInt16),
                    new ColumnLayout("Method", ColumnType.Method, GetColumnSize(ColumnType.Method)),
                    new ColumnLayout("Association", ColumnType.HasSemantics, GetColumnSize(ColumnType.HasSemantics))),
                new TableLayout(
                    new ColumnLayout("Class", ColumnType.TypeDef, GetColumnSize(ColumnType.TypeDef)),
                    new ColumnLayout("MethodBody", ColumnType.MethodDefOrRef, GetColumnSize(ColumnType.MethodDefOrRef)),
                    new ColumnLayout("MethodDeclaration", ColumnType.MethodDefOrRef, GetColumnSize(ColumnType.MethodDefOrRef))),
                new TableLayout(
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize)),
                new TableLayout(
                    new ColumnLayout("Signature", ColumnType.Blob, BlobIndexSize)),
                new TableLayout(
                    new ColumnLayout("MappingFlags", ColumnType.UInt16),
                    new ColumnLayout("MemberForwarded", ColumnType.MemberForwarded, GetColumnSize(ColumnType.MemberForwarded)),
                    new ColumnLayout("ImportName", ColumnType.String, StringIndexSize),
                    new ColumnLayout("ImportScope", ColumnType.ModuleRef, GetColumnSize(ColumnType.ModuleRef))),
                new TableLayout(
                    new ColumnLayout("RVA", ColumnType.UInt32),
                    new ColumnLayout("Field", ColumnType.Field, GetColumnSize(ColumnType.Field))),
                new TableLayout(
                    new ColumnLayout("Token", ColumnType.UInt32),
                    new ColumnLayout("FuncCode", ColumnType.UInt32)),
                new TableLayout(
                    new ColumnLayout("Token", ColumnType.UInt32)),
                new TableLayout(
                    new ColumnLayout("HashAlgId", ColumnType.UInt32),
                    new ColumnLayout("MajorVersion", ColumnType.UInt16),
                    new ColumnLayout("MinorVersion", ColumnType.UInt16),
                    new ColumnLayout("BuildNumber", ColumnType.UInt16),
                    new ColumnLayout("RevisionNumber", ColumnType.UInt16),
                    new ColumnLayout("Flags", ColumnType.UInt32),
                    new ColumnLayout("PublicKey", ColumnType.Blob, BlobIndexSize),
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                    new ColumnLayout("Culture", ColumnType.String, StringIndexSize)),
                new TableLayout(
                    new ColumnLayout("Processor", ColumnType.UInt32)),
                new TableLayout(
                    new ColumnLayout("PlatformId", ColumnType.UInt32),
                    new ColumnLayout("MajorVersion", ColumnType.UInt32),
                    new ColumnLayout("MinorVersion", ColumnType.UInt32)),
                new TableLayout(
                    new ColumnLayout("MajorVersion", ColumnType.UInt16),
                    new ColumnLayout("MinorVersion", ColumnType.UInt16),
                    new ColumnLayout("BuildNumber", ColumnType.UInt16),
                    new ColumnLayout("RevisionNumber", ColumnType.UInt16),
                    new ColumnLayout("Flags", ColumnType.UInt32),
                    new ColumnLayout("PublicKeyOrToken", ColumnType.Blob, BlobIndexSize),
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                    new ColumnLayout("Culture", ColumnType.String, StringIndexSize),
                    new ColumnLayout("HashValue", ColumnType.Blob, BlobIndexSize)),
                new TableLayout(
                    new ColumnLayout("Processor", ColumnType.UInt32),
                    new ColumnLayout("AssemblyRef", ColumnType.AssemblyRef, GetColumnSize(ColumnType.AssemblyRef))),
                new TableLayout(
                    new ColumnLayout("PlatformId", ColumnType.UInt32),
                    new ColumnLayout("MajorVersion", ColumnType.UInt32),
                    new ColumnLayout("MinorVersion", ColumnType.UInt32),
                    new ColumnLayout("AssemblyRef", ColumnType.AssemblyRef, GetColumnSize(ColumnType.AssemblyRef))),
                new TableLayout(
                    new ColumnLayout("Flags", ColumnType.UInt32),
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                    new ColumnLayout("HashValue", ColumnType.Blob, BlobIndexSize)),
                new TableLayout(
                    new ColumnLayout("Flags", ColumnType.UInt32),
                    new ColumnLayout("TypeDefId", ColumnType.UInt32),
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                    new ColumnLayout("Namespace", ColumnType.String, StringIndexSize),
                    new ColumnLayout("Implementation", ColumnType.Implementation, GetColumnSize(ColumnType.Implementation))),
                new TableLayout(
                    new ColumnLayout("Offset", ColumnType.UInt32),
                    new ColumnLayout("Flags", ColumnType.UInt32),
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                    new ColumnLayout("Implementation", ColumnType.Implementation, GetColumnSize(ColumnType.Implementation))),
                new TableLayout(
                    new ColumnLayout("NestedClass", ColumnType.TypeDef, GetColumnSize(ColumnType.TypeDef)),
                    new ColumnLayout("EnclosingClass", ColumnType.TypeDef, GetColumnSize(ColumnType.TypeDef))),
                new TableLayout(
                    new ColumnLayout("Number", ColumnType.UInt16),
                    new ColumnLayout("Flags", ColumnType.UInt16),
                    new ColumnLayout("Owner", ColumnType.TypeOrMethodDef, GetColumnSize(ColumnType.TypeOrMethodDef)),
                    new ColumnLayout("EnclosingClass", ColumnType.String, StringIndexSize)),
                new TableLayout(
                    new ColumnLayout("Method", ColumnType.Method, GetColumnSize(ColumnType.Method)),
                    new ColumnLayout("Instantiation", ColumnType.Blob, BlobIndexSize)),
                new TableLayout(
                    new ColumnLayout("Owner", ColumnType.GenericParam, GetColumnSize(ColumnType.GenericParam)),
                    new ColumnLayout("Constraint", ColumnType.TypeDefOrRef, GetColumnSize(ColumnType.TypeDefOrRef))),
            };
            
            return result;
        }

        /// <summary>
        /// Gets the range of metadata tokens referencing fields that a type defines. 
        /// </summary>
        /// <param name="typeDefRid">The row identifier of the type definition to obtain the fields from.</param>
        /// <returns>The range of metadata tokens.</returns>
        public MetadataRange GetFieldRange(uint typeDefRid) =>
            GetMemberRange(TableIndex.TypeDef, typeDefRid, 4,
                TableIndex.Field, TableIndex.FieldPtr);

        /// <summary>
        /// Gets the range of metadata tokens referencing methods that a type defines. 
        /// </summary>
        /// <param name="typeDefRid">The row identifier of the type definition to obtain the methods from.</param>
        /// <returns>The range of metadata tokens.</returns>
        public MetadataRange GetMethodRange(uint typeDefRid) =>
            GetMemberRange(TableIndex.TypeDef, typeDefRid, 5,
                TableIndex.Method, TableIndex.MethodPtr);
        
        /// <summary>
        /// Gets the range of metadata tokens referencing parameters that a method defines. 
        /// </summary>
        /// <param name="methodDefRid">The row identifier of the method definition to obtain the parameters from.</param>
        /// <returns>The range of metadata tokens.</returns>
        public MetadataRange GetParameterRange(uint methodDefRid) =>
            GetMemberRange(TableIndex.Method, methodDefRid, 5,
                TableIndex.Param, TableIndex.ParamPtr);

        /// <summary>
        /// Gets the range of metadata tokens referencing properties that a property map row defines. 
        /// </summary>
        /// <param name="propertyMapRid">The row identifier of the property map to obtain the properties from.</param>
        /// <returns>The range of metadata tokens.</returns>
        public MetadataRange GetPropertyRange(uint propertyMapRid) =>
            GetMemberRange(TableIndex.PropertyMap, propertyMapRid, 1,
                TableIndex.Property, TableIndex.PropertyPtr);

        /// <summary>
        /// Gets the range of metadata tokens referencing events that a event map row defines. 
        /// </summary>
        /// <param name="eventMapRid">The row identifier of the event map to obtain the events from.</param>
        /// <returns>The range of metadata tokens.</returns>
        public MetadataRange GetEventRange(uint eventMapRid) =>
            GetMemberRange(TableIndex.EventMap, eventMapRid, 1,
                TableIndex.Event, TableIndex.EventPtr);

        private MetadataRange GetMemberRange(TableIndex ownerTableIndex, uint ownerRid, int ownerColumnIndex,
            TableIndex memberTableIndex, TableIndex redirectTableIndex)
        {
            int index = (int) (ownerRid - 1);
            
            // Check if valid owner RID.
            var ownerTable = GetTable(ownerTableIndex);
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
                return new RedirectedMetadataRange(redirectTable, memberTableIndex, startRid, endRid);
            
            // If not, its a simple range.
            return new ContinuousMetadataRange(memberTableIndex, startRid, endRid);
        }
    }
}