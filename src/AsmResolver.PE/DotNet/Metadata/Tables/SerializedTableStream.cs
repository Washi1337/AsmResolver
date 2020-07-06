using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides an implementation of a tables stream that obtains tables from a readable segment in a file.  
    /// </summary>
    public class SerializedTableStream : TablesStream
    {
        private readonly IReadableSegment _contents;
        private readonly ISegmentReferenceResolver _referenceResolver;
        private readonly ulong _validMask;
        private readonly ulong _sortedMask;
        private readonly uint[] _rowCounts;
        private readonly IndexSize[] _indexSizes;
        private readonly uint _headerSize;
        private bool _tablesInitialized;

        /// <summary>
        /// Creates a new tables stream based on a byte array.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="rawData">The raw contents of the stream.</param>
        /// <param name="referenceResolver">The instance that resolves references to other segments in the file to segments.</param>
        public SerializedTableStream(string name, byte[] rawData, ISegmentReferenceResolver referenceResolver)
            : this(name, new DataSegment(rawData), referenceResolver)
        {
        }

        /// <summary>
        /// Creates a new tables stream based on a segment in a file.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="contents">The raw contents of the stream.</param>
        /// <param name="referenceResolver">The instance that resolves references to other segments in the file to segments.</param>
        public SerializedTableStream(string name, IReadableSegment contents, ISegmentReferenceResolver referenceResolver)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _contents = contents ?? throw new ArgumentNullException(nameof(contents));
            _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));

            var reader = contents.CreateReader();
            Reserved = reader.ReadUInt32();
            MajorVersion = reader.ReadByte();
            MinorVersion = reader.ReadByte();
            Flags = (TablesStreamFlags) reader.ReadByte();
            Log2LargestRid = reader.ReadByte();
            _validMask = reader.ReadUInt64();
            _sortedMask = reader.ReadUInt64();

            _rowCounts = ReadRowCounts(reader);

            if (HasExtraData)
                ExtraData = reader.ReadUInt32();

            _headerSize = reader.FileOffset - reader.StartPosition;

            _indexSizes = InitializeIndexSizes();
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override IBinaryStreamReader CreateReader()
        {
            return _contents.CreateReader();
        }

        private uint[] ReadRowCounts(IBinaryStreamReader reader)
        {
            const TableIndex maxTableIndex = TableIndex.GenericParamConstraint;
            
            var result = new uint[(int) maxTableIndex + 1 ];
            for (TableIndex i = 0; i <= maxTableIndex; i++)
                result[(int) i] = HasTable(_validMask, i) ? reader.ReadUInt32() : 0;

            return result;
        }

        /// <inheritdoc />
        protected override uint GetColumnSize(ColumnType columnType)
        {
            if (_tablesInitialized || (int) columnType >= _indexSizes.Length)
                return base.GetColumnSize(columnType);
            return (uint) _indexSizes[(int) columnType];
        }

        private IndexSize[] InitializeIndexSizes()
        {
            const ColumnType maxColumnType = ColumnType.String;

            var result = new List<IndexSize>((int) maxColumnType);

            // Add index sizes for each table:
            foreach (uint t in _rowCounts)
                result.Add(t > 0xFFFF ? IndexSize.Long : IndexSize.Short);

            // Add index sizes for each coded index:
            result.AddRange(new[]
            {
                // TypeDefOrRef
                GetCodedIndexSize(TableIndex.TypeDef, TableIndex.TypeRef, TableIndex.TypeSpec),

                // HasConstant
                GetCodedIndexSize(TableIndex.Field, TableIndex.Param, TableIndex.Property),

                // HasCustomAttribute
                GetCodedIndexSize(
                    TableIndex.Method, TableIndex.Field, TableIndex.TypeRef, TableIndex.TypeDef,
                    TableIndex.Param, TableIndex.InterfaceImpl, TableIndex.MemberRef, TableIndex.Module,
                    TableIndex.DeclSecurity, TableIndex.Property, TableIndex.Event, TableIndex.StandAloneSig,
                    TableIndex.ModuleRef, TableIndex.TypeSpec, TableIndex.Assembly, TableIndex.AssemblyRef,
                    TableIndex.File, TableIndex.ExportedType, TableIndex.ManifestResource, TableIndex.GenericParam,
                    TableIndex.GenericParamConstraint, TableIndex.MethodSpec),

                // HasFieldMarshal
                GetCodedIndexSize(TableIndex.Field, TableIndex.Param),

                // HasDeclSecurity
                GetCodedIndexSize(TableIndex.TypeDef, TableIndex.Method, TableIndex.Assembly),

                // MemberRefParent
                GetCodedIndexSize(
                    TableIndex.TypeDef, TableIndex.TypeRef, TableIndex.ModuleRef,
                    TableIndex.Method, TableIndex.TypeSpec),

                // HasSemantics
                GetCodedIndexSize(TableIndex.Event, TableIndex.Property),

                // MethodDefOrRef
                GetCodedIndexSize(TableIndex.Method, TableIndex.MemberRef),

                // MemberForwarded
                GetCodedIndexSize(TableIndex.Field, TableIndex.Method),

                // Implementation
                GetCodedIndexSize(TableIndex.File, TableIndex.AssemblyRef, TableIndex.ExportedType),

                // CustomAttributeType
                GetCodedIndexSize(0, 0, TableIndex.Method, TableIndex.MemberRef, 0),

                // ResolutionScope
                GetCodedIndexSize(TableIndex.Module, TableIndex.ModuleRef, TableIndex.AssemblyRef, TableIndex.TypeRef),
                
                // TypeOrMethodDef
                GetCodedIndexSize(TableIndex.TypeDef, TableIndex.Method),
            });

            return result.ToArray();
        }

        private IndexSize GetCodedIndexSize(params TableIndex[] tables)
        {
            int tableIndexBitCount = (int) Math.Ceiling(Math.Log(tables.Length, 2));
            int maxSmallTableMemberCount = ushort.MaxValue >> tableIndexBitCount;

            return tables.Select(t => _rowCounts[(int) t]).All(c => c < maxSmallTableMemberCount)
                ? IndexSize.Short
                : IndexSize.Long;
        }

        /// <inheritdoc />
        protected override IList<IMetadataTable> GetTables()
        {
            uint offset = _contents.FileOffset + _headerSize;
            var tables = new IMetadataTable[]
            {
                CreateNextTable(TableIndex.Module, ref offset, ModuleDefinitionRow.FromReader),
                CreateNextTable(TableIndex.TypeRef, ref offset, TypeReferenceRow.FromReader),
                CreateNextTable(TableIndex.TypeDef, ref offset, TypeDefinitionRow.FromReader),
                CreateNextTable(TableIndex.FieldPtr, ref offset, FieldPointerRow.FromReader),
                CreateNextTable(TableIndex.Field, ref offset, FieldDefinitionRow.FromReader),
                CreateNextTable(TableIndex.MethodPtr, ref offset, MethodPointerRow.FromReader),
                CreateNextTable(TableIndex.Method, ref offset, MethodDefinitionRow.FromReader, _referenceResolver),
                CreateNextTable(TableIndex.ParamPtr, ref offset, ParameterPointerRow.FromReader),
                CreateNextTable(TableIndex.Param, ref offset, ParameterDefinitionRow.FromReader),
                CreateNextTable(TableIndex.InterfaceImpl, ref offset, InterfaceImplementationRow.FromReader),
                CreateNextTable(TableIndex.MemberRef, ref offset, MemberReferenceRow.FromReader),
                CreateNextTable(TableIndex.Constant, ref offset, ConstantRow.FromReader),
                CreateNextTable(TableIndex.CustomAttribute, ref offset, CustomAttributeRow.FromReader),
                CreateNextTable(TableIndex.FieldMarshal, ref offset, FieldMarshalRow.FromReader),
                CreateNextTable(TableIndex.DeclSecurity, ref offset, SecurityDeclarationRow.FromReader),
                CreateNextTable(TableIndex.ClassLayout, ref offset, ClassLayoutRow.FromReader),
                CreateNextTable(TableIndex.FieldLayout, ref offset, FieldLayoutRow.FromReader),
                CreateNextTable(TableIndex.StandAloneSig, ref offset, StandAloneSignatureRow.FromReader),
                CreateNextTable(TableIndex.EventMap, ref offset, EventMapRow.FromReader),
                CreateNextTable(TableIndex.EventPtr, ref offset, EventPointerRow.FromReader),
                CreateNextTable(TableIndex.Event, ref offset, EventDefinitionRow.FromReader),
                CreateNextTable(TableIndex.PropertyMap, ref offset, PropertyMapRow.FromReader),
                CreateNextTable(TableIndex.PropertyPtr, ref offset, PropertyPointerRow.FromReader),
                CreateNextTable(TableIndex.Property, ref offset, PropertyDefinitionRow.FromReader),
                CreateNextTable(TableIndex.MethodSemantics, ref offset, MethodSemanticsRow.FromReader),
                CreateNextTable(TableIndex.MethodImpl, ref offset, MethodImplementationRow.FromReader),
                CreateNextTable(TableIndex.ModuleRef, ref offset, ModuleReferenceRow.FromReader),
                CreateNextTable(TableIndex.TypeSpec, ref offset, TypeSpecificationRow.FromReader),
                CreateNextTable(TableIndex.ImplMap, ref offset, ImplementationMapRow.FromReader),
                CreateNextTable(TableIndex.FieldRva, ref offset, FieldRvaRow.FromReader, _referenceResolver),
                CreateNextTable(TableIndex.EncLog, ref offset, EncLogRow.FromReader),
                CreateNextTable(TableIndex.EncMap, ref offset, EncMapRow.FromReader),
                CreateNextTable(TableIndex.Assembly, ref offset, AssemblyDefinitionRow.FromReader),
                CreateNextTable(TableIndex.AssemblyProcessor, ref offset, AssemblyProcessorRow.FromReader),
                CreateNextTable(TableIndex.AssemblyOS, ref offset, AssemblyOSRow.FromReader),
                CreateNextTable(TableIndex.AssemblyRef, ref offset, AssemblyReferenceRow.FromReader),
                CreateNextTable(TableIndex.AssemblyRefProcessor, ref offset, AssemblyRefProcessorRow.FromReader),
                CreateNextTable(TableIndex.AssemblyRefOS, ref offset, AssemblyRefOSRow.FromReader),
                CreateNextTable(TableIndex.File, ref offset, FileReferenceRow.FromReader),
                CreateNextTable(TableIndex.ExportedType, ref offset, ExportedTypeRow.FromReader),
                CreateNextTable(TableIndex.ManifestResource, ref offset, ManifestResourceRow.FromReader),
                CreateNextTable(TableIndex.NestedClass, ref offset, NestedClassRow.FromReader),
                CreateNextTable(TableIndex.GenericParam, ref offset, GenericParameterRow.FromReader),
                CreateNextTable(TableIndex.MethodSpec, ref offset, MethodSpecificationRow.FromReader),
                CreateNextTable(TableIndex.GenericParamConstraint, ref offset, GenericParameterConstraintRow.FromReader),
            };
            _tablesInitialized = true;
            return tables;
        }

        private IBinaryStreamReader CreateNextRawTableReader(TableIndex currentIndex, ref uint currentOffset)
        {
            int index = (int) currentIndex;
            uint rawSize = TableLayouts[index].RowSize * _rowCounts[index];
            var tableReader = _contents.CreateReader(currentOffset, rawSize);
            currentOffset += rawSize;
            return tableReader;
        }

        private SerializedMetadataTable<TRow> CreateNextTable<TRow>(
            TableIndex index,
            ref uint offset,
            SerializedMetadataTable<TRow>.ReadRowDelegate readRow)
            where TRow : struct, IMetadataRow
        {
            return new SerializedMetadataTable<TRow>(
                CreateNextRawTableReader(index, ref offset), index, TableLayouts[(int) index], readRow);
        }

        private SerializedMetadataTable<TRow> CreateNextTable<TRow>(
            TableIndex index,
            ref uint offset,
            SerializedMetadataTable<TRow>.ReadRowExtendedDelegate readRow,
            ISegmentReferenceResolver referenceResolver)
            where TRow : struct, IMetadataRow
        {
            return new SerializedMetadataTable<TRow>(
                CreateNextRawTableReader(index, ref offset), index, TableLayouts[(int) index], readRow, referenceResolver);
        }
        
    }
}