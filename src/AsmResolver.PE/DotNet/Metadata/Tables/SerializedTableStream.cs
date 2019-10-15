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
            
            _layouts = InitializeTableLayouts(_rowCounts);
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

        private TableLayout[] InitializeTableLayouts(IList<uint> rowCounts)
        {
            var result = new[]
            {
                new TableLayout(
                    new ColumnLayout("Generation", ColumnType.UInt16),
                    new ColumnLayout("Name", ColumnType.String, (uint) StringIndexSize),
                    new ColumnLayout("Mvid", ColumnType.Guid, (uint) GuidIndexSize),
                    new ColumnLayout("EncId", ColumnType.Guid, (uint) GuidIndexSize),
                    new ColumnLayout("EncBaseId", ColumnType.Guid, (uint) GuidIndexSize)),
            };
            
            return result;
        }

        protected override IList<IMetadataTable> GetTables()
        {
            uint offset = _contents.FileOffset + _headerSize;
            return new IMetadataTable[]
            {
                new SerializedMetadataTable<ModuleDefinitionRow>(
                    CreateNextRawTableReader(TableIndex.Module, ref offset), _layouts[0], ModuleDefinitionRow.FromReader),
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