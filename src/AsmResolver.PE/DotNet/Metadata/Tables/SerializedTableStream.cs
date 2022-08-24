using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Pdb;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides an implementation of a tables stream that obtains tables from a readable segment in a file.
    /// </summary>
    public class SerializedTableStream : TablesStream, ILazyMetadataStream
    {
        private readonly MetadataReaderContext _context;
        private readonly BinaryStreamReader _reader;
        private readonly ulong _validMask;
        private readonly ulong _sortedMask;
        private readonly uint[] _rowCounts;
        private readonly uint _headerSize;
        private bool _tablesInitialized;

        /// <summary>
        /// Same as <see cref="_rowCounts"/> but may contain row counts from an external tables stream.
        /// This is required for metadata directories containing Portable PDB debug data.
        /// </summary>
        private uint[]? _combinedRowCounts;

        /// <summary>
        /// Contains the initial sizes of every column type.
        /// </summary>
        private IndexSize[]? _indexSizes;

        /// <summary>
        /// Creates a new tables stream with the provided byte array as the raw contents of the stream.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="name">The name of the stream.</param>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedTableStream(MetadataReaderContext context, string name, byte[] rawData)
            : this(context, name, ByteArrayDataSource.CreateReader(rawData))
        {
        }

        /// <summary>
        /// Creates a new tables stream with the provided file segment reader as the raw contents of the stream.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="name">The name of the stream.</param>
        /// <param name="reader">The raw contents of the stream.</param>
        public SerializedTableStream(MetadataReaderContext context, string name, in BinaryStreamReader reader)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _reader = reader;

            Offset = reader.Offset;
            Rva = reader.Rva;

            var headerReader = _reader.Fork();
            Reserved = headerReader.ReadUInt32();
            MajorVersion = headerReader.ReadByte();
            MinorVersion = headerReader.ReadByte();
            Flags = (TablesStreamFlags) headerReader.ReadByte();
            Log2LargestRid = headerReader.ReadByte();
            _validMask = headerReader.ReadUInt64();
            _sortedMask = headerReader.ReadUInt64();
            _rowCounts = ReadRowCounts(ref headerReader);

            if (HasExtraData)
                ExtraData = headerReader.ReadUInt32();

            _headerSize = headerReader.RelativeOffset;
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override BinaryStreamReader CreateReader() => _reader.Fork();

        private uint[] ReadRowCounts(ref BinaryStreamReader reader)
        {
            uint[] result = new uint[(int) TableIndex.Max];

            for (TableIndex i = 0; i < TableIndex.Max; i++)
            {
                result[(int) i] = HasTable(_validMask, i)
                    ? reader.ReadUInt32()
                    : 0;
            }

            return result;
        }

        /// <inheritdoc />
        public void Initialize(IMetadata parentMetadata)
        {
            if (parentMetadata.TryGetStream(out PdbStream? pdbStream))
            {
                // Metadata that contains a PDB stream should use the row counts provided in the pdb stream
                // for computing the size of a column.
                _combinedRowCounts = new uint[_rowCounts.Length];
                ExternalRowCounts = new uint[(int) TableIndex.Document];

                for (int i = 0; i < (int) TableIndex.Document; i++)
                {
                    _combinedRowCounts[i] = pdbStream.TypeSystemRowCounts[i];
                    ExternalRowCounts[i] = pdbStream.TypeSystemRowCounts[i];
                }

                for (int i = (int) TableIndex.Document; i < (int) TableIndex.Max; i++)
                    _combinedRowCounts[i] = _rowCounts[i];

            }
            else
            {
                // Otherwise, just use the original row counts array.
                _combinedRowCounts = _rowCounts;
            }

            _indexSizes = InitializeIndexSizes();
        }

        /// <inheritdoc />
        protected override uint GetColumnSize(ColumnType columnType)
        {
            if (_tablesInitialized)
                return base.GetColumnSize(columnType);
            if (_indexSizes is null)
                throw new InvalidOperationException("Serialized tables stream is not fully initialized yet.");
            if ((int) columnType >= _indexSizes.Length)
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

                // HasCustomDebugInformation
                GetCodedIndexSize(TableIndex.Method, TableIndex.Field, TableIndex.TypeRef, TableIndex.TypeDef,
                    TableIndex.Param, TableIndex.InterfaceImpl, TableIndex.MemberRef, TableIndex.Module,
                    TableIndex.DeclSecurity, TableIndex.Property, TableIndex.Event, TableIndex.StandAloneSig,
                    TableIndex.ModuleRef, TableIndex.TypeSpec, TableIndex.Assembly, TableIndex.AssemblyRef,
                    TableIndex.File, TableIndex.ExportedType, TableIndex.ManifestResource, TableIndex.GenericParam,
                    TableIndex.GenericParamConstraint, TableIndex.MethodSpec, TableIndex.Document,
                    TableIndex.LocalScope, TableIndex.LocalVariable, TableIndex.LocalConstant, TableIndex.ImportScope)
            });

            return result.ToArray();
        }

        private IndexSize GetCodedIndexSize(params TableIndex[] tables)
        {
            if (_combinedRowCounts is null)
                throw new InvalidOperationException("Serialized tables stream is not fully initialized yet.");

            int tableIndexBitCount = (int) Math.Ceiling(Math.Log(tables.Length, 2));
            int maxSmallTableMemberCount = ushort.MaxValue >> tableIndexBitCount;

            return tables.Select(t => _combinedRowCounts[(int) t]).All(c => c < maxSmallTableMemberCount)
                ? IndexSize.Short
                : IndexSize.Long;
        }

        /// <inheritdoc />
        protected override IList<IMetadataTable?> GetTables()
        {
            uint offset = _headerSize;
            var tables = new IMetadataTable?[]
            {
                CreateNextTable(TableIndex.Module, ref offset, ModuleDefinitionRow.FromReader),
                CreateNextTable(TableIndex.TypeRef, ref offset, TypeReferenceRow.FromReader),
                CreateNextTable(TableIndex.TypeDef, ref offset, TypeDefinitionRow.FromReader),
                CreateNextTable(TableIndex.FieldPtr, ref offset, FieldPointerRow.FromReader),
                CreateNextTable(TableIndex.Field, ref offset, FieldDefinitionRow.FromReader),
                CreateNextTable(TableIndex.MethodPtr, ref offset, MethodPointerRow.FromReader),
                CreateNextTable(TableIndex.Method, ref offset, MethodDefinitionRow.FromReader),
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
                CreateNextTable(TableIndex.FieldRva, ref offset, FieldRvaRow.FromReader),
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
                null,
                null,
                null,
                CreateNextTable(TableIndex.Document, ref offset, DocumentRow.FromReader),
                CreateNextTable(TableIndex.MethodDebugInformation, ref offset, MethodDebugInformationRow.FromReader),
                CreateNextTable(TableIndex.LocalScope, ref offset, LocalScopeRow.FromReader),
                CreateNextTable(TableIndex.LocalVariable, ref offset, LocalVariableRow.FromReader),
                CreateNextTable(TableIndex.LocalConstant, ref offset, LocalConstantRow.FromReader),
                CreateNextTable(TableIndex.ImportScope, ref offset, ImportScopeRow.FromReader),
                CreateNextTable(TableIndex.StateMachineMethod, ref offset, StateMachineMethodRow.FromReader),
                CreateNextTable(TableIndex.CustomDebugInformation, ref offset, CustomDebugInformationRow.FromReader),
            };
            _tablesInitialized = true;
            return tables;
        }

        private BinaryStreamReader CreateNextRawTableReader(TableIndex currentIndex, ref uint currentOffset)
        {
            int index = (int) currentIndex;
            uint rawSize = TableLayouts[index].RowSize * _rowCounts[index];
            var tableReader = _reader.ForkRelative(currentOffset, rawSize);
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
                CreateNextRawTableReader(index, ref offset),
                index,
                TableLayouts[(int) index],
                readRow);
        }

        private SerializedMetadataTable<TRow> CreateNextTable<TRow>(
            TableIndex index,
            ref uint offset,
            SerializedMetadataTable<TRow>.ReadRowExtendedDelegate readRow)
            where TRow : struct, IMetadataRow
        {
            return new SerializedMetadataTable<TRow>(
                _context,
                CreateNextRawTableReader(index, ref offset),
                index,
                TableLayouts[(int) index],
                readRow);
        }

    }
}
