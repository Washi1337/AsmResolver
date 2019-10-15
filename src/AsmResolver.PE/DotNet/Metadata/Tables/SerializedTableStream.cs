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
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    public class SerializedTableStream : TablesStream
    {
        private readonly IReadableSegment _contents;
        private readonly ulong _validMask;
        private readonly ulong _sortedMask;
        private readonly uint[] _rowCounts;
        private readonly TableLayout[] _layouts;
        private readonly IndexSize[] _indexSizes;
        private readonly uint _headerSize;

        public SerializedTableStream(byte[] rawData)
            : this(new DataSegment(rawData))
        {
        }

        public SerializedTableStream(IReadableSegment contents)
        {
            _contents = contents ?? throw new ArgumentNullException(nameof(contents));

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

            _indexSizes = InitializeCodedIndices();
            _layouts = InitializeTableLayouts();
        }

        public override bool CanRead => true;

        public override IBinaryStreamReader CreateReader()
        {
            return _contents.CreateReader();
        }

        protected bool HasTable(TableIndex table)
        {
            return ((_validMask >> (int) table) & 1) != 0;
        }

        protected bool IsSorted(TableIndex table)
        {
            return ((_sortedMask >> (int) table) & 1) != 0;
        }

        private uint[] ReadRowCounts(IBinaryStreamReader reader)
        {
            const TableIndex maxTableIndex = TableIndex.GenericParamConstraint;
            
            var result = new uint[(int) maxTableIndex + 1];
            for (TableIndex i = 0; i <= maxTableIndex; i++)
                result[(int) i] = HasTable(i) ? reader.ReadUInt32() : 0;

            return result;
        }

        private TableLayout[] InitializeTableLayouts()
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
                        _indexSizes[(int) CodedIndex.ResolutionScope]),
                    new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                    new ColumnLayout("Namespace", ColumnType.Guid, StringIndexSize))
            };
            
            return result;
        }

        private IndexSize[] InitializeCodedIndices()
        {
            return new[]
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
            };
        }

        private IndexSize GetCodedIndexSize(params TableIndex[] tables)
        {
            int tableIndexBitCount = (int) Math.Ceiling(Math.Log(tables.Length, 2));
            int maxSmallTableMemberCount = ushort.MaxValue >> tableIndexBitCount;

            return tables.Select(t => _rowCounts[(int) t]).All(c => c < maxSmallTableMemberCount)
                ? IndexSize.Short
                : IndexSize.Long;
        }

        protected override IList<IMetadataTable> GetTables()
        {
            uint offset = _contents.FileOffset + _headerSize;
            return new IMetadataTable[]
            {
                new SerializedMetadataTable<ModuleDefinitionRow>(
                    CreateNextRawTableReader(TableIndex.Module, ref offset), _layouts[0], ModuleDefinitionRow.FromReader),
                new SerializedMetadataTable<TypeReferenceRow>(
                    CreateNextRawTableReader(TableIndex.TypeRef, ref offset), _layouts[1], TypeReferenceRow.FromReader),
            };
        }

        private IBinaryStreamReader CreateNextRawTableReader(TableIndex currentIndex, ref uint currentOffset)
        {
            int index = (int) currentIndex;
            uint rawSize = _layouts[index].RowSize * _rowCounts[index];
            var tableReader = _contents.CreateReader(currentOffset, rawSize);
            currentOffset += rawSize;
            return tableReader;
        }
        
    }
}