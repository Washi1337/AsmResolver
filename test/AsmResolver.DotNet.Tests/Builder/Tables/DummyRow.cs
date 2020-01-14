using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Tests.Builder.Tables
{
    public struct DummyRow : IMetadataRow
    {
        public DummyRow(uint id)
        {
            Id = id;
        }
        
        public TableIndex TableIndex => TableIndex.Module;
        
        public int Count => 1;

        public uint Id
        {
            get;
        }

        public uint this[int index] => index switch
        {
            0 => Id,
            _ => throw new ArgumentOutOfRangeException()
        };

        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<uint> GetEnumerator()
        {
            return new MetadataRowColumnEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}