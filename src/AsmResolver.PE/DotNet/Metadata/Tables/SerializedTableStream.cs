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

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    public class SerializedTableStream : TablesStream
    {
        private readonly IReadableSegment _contents;
        private readonly ulong _validMask;
        private readonly ulong _sortedMask;

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

            var rowCounts = ReadRowCounts(reader);
            // TODO: initialize tables using row counts.

            if (HasExtraData)
                ExtraData = reader.ReadUInt32();
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

        private IList<uint> ReadRowCounts(IBinaryStreamReader reader)
        {
            var result = new List<uint>();

            const TableIndex maxTableIndex = TableIndex.GenericParamConstraint;
            for (TableIndex i = 0; i <= maxTableIndex; i++)
            {
                if (HasTable(i)) 
                    result.Add(reader.ReadUInt32());
            }
            
            return result;
        }
        
    }
}