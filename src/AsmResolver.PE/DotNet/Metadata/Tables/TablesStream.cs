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
using System.Linq;
using System.Threading;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.PE.DotNet.Metadata.Tables
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
        private const int MaxTableCount = (int) TableIndex.GenericParamConstraint;

        private readonly LazyVariable<IList<IMetadataTable>> _tables;
        private readonly LazyVariable<IList<TableLayout>> _layouts;

        public TablesStream()
        {
            _layouts = new LazyVariable<IList<TableLayout>>(GetTableLayouts);
            _tables = new LazyVariable<IList<IMetadataTable>>(GetTables);
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

        protected IList<TableLayout> TableLayouts => _layouts.Value;
        
        /// <inheritdoc />
        public virtual IBinaryStreamReader CreateReader() => throw new NotSupportedException();

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            FileOffset = newFileOffset;
            Rva = newRva;
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
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

        public uint GetVirtualSize() => GetPhysicalSize();

        /// <inheritdoc />
        public virtual void Write(IBinaryStreamWriter writer)
        {
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

        protected virtual ulong ComputeSortedBitmask()
        {
            // TODO: make more configurable (maybe add IMetadataTable.IsSorted property?).
            
            return 0x000016003301FA00;
        }

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
        
        protected virtual void WriteRowCounts(IBinaryStreamWriter writer, ulong validBitmask)
        {
            for (TableIndex i = 0; i < (TableIndex) Tables.Count; i++)
            {
                if (HasTable(validBitmask, i))
                    writer.WriteInt32(GetTable(i).Count);
            }
        }

        protected virtual void WriteTables(IBinaryStreamWriter writer, ulong validBitmask)
        {
            for (TableIndex i = 0; i < (TableIndex) Tables.Count; i++)
            {
                if (HasTable(validBitmask, i))
                    GetTable(i).Write(writer);
            }
        }

        protected static bool HasTable(ulong validMask, TableIndex table)
        {
            return ((validMask >> (int) table) & 1) != 0;
        }

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
                new MetadataTable<ModuleDefinitionRow>(layouts[0]), 
                new MetadataTable<TypeReferenceRow>(layouts[1]), 
                new MetadataTable<TypeDefinitionRow>(layouts[2]), 
                new MetadataTable<FieldPointerRow>(layouts[3]), 
                new MetadataTable<FieldDefinitionRow>(layouts[4]), 
                new MetadataTable<MethodPointerRow>(layouts[5]), 
                new MetadataTable<MethodDefinitionRow>(layouts[6]), 
                new MetadataTable<ParameterPointerRow>(layouts[7]), 
                new MetadataTable<ParameterDefinitionRow>(layouts[8]), 
                new MetadataTable<InterfaceImplementationRow>(layouts[9]), 
                new MetadataTable<MemberReferenceRow>(layouts[10]), 
                new MetadataTable<ConstantRow>(layouts[11]), 
                new MetadataTable<CustomAttributeRow>(layouts[12]), 
                new MetadataTable<FieldMarshalRow>(layouts[13]), 
                new MetadataTable<SecurityDeclarationRow>(layouts[14]), 
                new MetadataTable<ClassLayoutRow>(layouts[15]), 
                new MetadataTable<FieldLayoutRow>(layouts[16]), 
                new MetadataTable<StandAloneSignatureRow>(layouts[17]), 
                new MetadataTable<EventMapRow>(layouts[18]), 
                new MetadataTable<EventPointerRow>(layouts[19]),
                new MetadataTable<EventDefinitionRow>(layouts[20]), 
                new MetadataTable<PropertyMapRow>(layouts[21]), 
                new MetadataTable<PropertyPointerRow>(layouts[22]), 
                new MetadataTable<PropertyDefinitionRow>(layouts[23]), 
                new MetadataTable<MethodSemanticsRow>(layouts[24]), 
                new MetadataTable<MethodImplementationRow>(layouts[25]), 
                new MetadataTable<ModuleReferenceRow>(layouts[26]), 
                new MetadataTable<TypeSpecificationRow>(layouts[27]), 
                new MetadataTable<ImplementationMapRow>(layouts[28]), 
                new MetadataTable<FieldRvaRow>(layouts[29]), 
                new MetadataTable<EncLogRow>(layouts[30]), 
                new MetadataTable<EncMapRow>(layouts[31]), 
                new MetadataTable<AssemblyDefinitionRow>(layouts[32]), 
                new MetadataTable<AssemblyProcessorRow>(layouts[33]), 
                new MetadataTable<AssemblyOSRow>(layouts[34]), 
                new MetadataTable<AssemblyReferenceRow>(layouts[35]), 
                new MetadataTable<AssemblyRefProcessorRow>(layouts[36]), 
                new MetadataTable<AssemblyRefOSRow>(layouts[37]), 
                new MetadataTable<FileReferenceRow>(layouts[38]), 
                new MetadataTable<ExportedTypeRow>(layouts[39]), 
                new MetadataTable<ManifestResourceRow>(layouts[40]),
                new MetadataTable<NestedClassRow>(layouts[41]), 
                new MetadataTable<GenericParameterRow>(layouts[42]), 
                new MetadataTable<MethodSpecificationRow>(layouts[43]), 
                new MetadataTable<GenericParameterConstraintRow>(layouts[44]), 
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

        private IndexSize GetStreamIndexSize(int bitIndex) => (IndexSize) (((((int) Flags >> bitIndex) & 1) + 1) * 2);

        private void SetStreamIndexSize(int bitIndex, IndexSize newSize)
        {
            Flags = (TablesStreamFlags) (((int) Flags & ~(1 << bitIndex))
                                         | (newSize == IndexSize.Long ? 1 << bitIndex : 0));
        }

        protected virtual IndexSize GetColumnSize(ColumnType columnType)
        {
            return IndexSize.Long;
        }

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
        
    }
}