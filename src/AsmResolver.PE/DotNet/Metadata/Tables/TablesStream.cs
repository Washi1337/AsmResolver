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
        
        public TablesStream()
        {
            _tables = new LazyVariable<IList<IMetadataTable>>(GetTables);
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

        /// <inheritdoc />
        public virtual IBinaryStreamReader CreateReader() => throw new NotSupportedException();

        /// <summary>
        /// Obtains the collection of tables in the tables stream.
        /// </summary>
        /// <returns>The tables, including empty tables if there are any.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Tables"/> property.
        /// </remarks>
        protected virtual IList<IMetadataTable> GetTables()
        {
            return new IMetadataTable[]
            {
                new MetadataTable<ModuleDefinitionRow>(), 
                new MetadataTable<TypeReferenceRow>(), 
                new MetadataTable<TypeDefinitionRow>(), 
                new MetadataTable<FieldPointerRow>(), 
                new MetadataTable<FieldDefinitionRow>(), 
                new MetadataTable<MethodPointerRow>(), 
                new MetadataTable<MethodDefinitionRow>(), 
                new MetadataTable<ParameterPointerRow>(), 
                new MetadataTable<ParameterDefinitionRow>(), 
                new MetadataTable<InterfaceImplementationRow>(), 
                new MetadataTable<MemberReferenceRow>(), 
                new MetadataTable<ConstantRow>(), 
                new MetadataTable<CustomAttributeRow>(), 
                new MetadataTable<FieldMarshalRow>(), 
                new MetadataTable<SecurityDeclarationRow>(), 
                new MetadataTable<ClassLayoutRow>(), 
                new MetadataTable<FieldLayoutRow>(), 
                new MetadataTable<StandAloneSignatureRow>(), 
                new MetadataTable<EventMapRow>(), 
                new MetadataTable<EventDefinitionRow>(), 
                new MetadataTable<PropertyMapRow>(), 
                new MetadataTable<PropertyPointerRow>(), 
                new MetadataTable<PropertyDefinitionRow>(), 
                new MetadataTable<MethodSemanticsRow>(), 
                new MetadataTable<MethodImplementationRow>(), 
                new MetadataTable<ModuleReferenceRow>(), 
                new MetadataTable<TypeSpecificationRow>(), 
                new MetadataTable<ImplementationMapRow>(), 
                new MetadataTable<FieldRvaRow>(), 
                new MetadataTable<EncLogRow>(), 
                new MetadataTable<EncMapRow>(), 
                new MetadataTable<AssemblyDefinitionRow>(), 
                new MetadataTable<AssemblyProcessorRow>(), 
                new MetadataTable<AssemblyOSRow>(), 
                new MetadataTable<AssemblyReferenceRow>(), 
                new MetadataTable<AssemblyRefProcessorRow>(), 
                new MetadataTable<AssemblyRefOSRow>(), 
                new MetadataTable<FileReferenceRow>(), 
                new MetadataTable<ExportedTypeRow>(), 
                new MetadataTable<ManifestResourceRow>(), 
                new MetadataTable<NestedClassRow>(), 
                new MetadataTable<GenericParameterRow>(), 
                new MetadataTable<MethodSpecificationRow>(), 
                new MetadataTable<GenericParameterConstraintRow>(), 
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

        protected TableLayout[] InitializeTableLayouts()
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