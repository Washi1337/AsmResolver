using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using TUP.AsmResolver.PE;
using TUP.AsmResolver.PE.Readers;
using TUP.AsmResolver.NET.Specialized;
namespace TUP.AsmResolver.NET
{
    public class TablesHeap : Heap
    {
        internal MetaDataStream stream;
        internal Structures.METADATA_TABLE_HEADER header;
        internal NETTableReader tablereader;
        internal MetaDataTable[] tables;
        internal int tablecount;

        


        internal TablesHeap()
        {
            tables = new MetaDataTable[45];
            
        }

        public static TablesHeap FromStream(MetaDataStream stream)
        {
            NETTableReader reader = new NETTableReader(stream);
            
            return reader.tableheap;
            
        }


        public byte MajorVersion
        {
            get { return header.MajorVersion; }
            set
            {
                PeImage image = stream.netheader.assembly.peImage;
                image.SetOffset(StreamOffset + Structures.DataOffsets[typeof(Structures.METADATA_TABLE_HEADER)][1]);
                image.writer.Write(value);
                header.MajorVersion = value;
            }
        }
        public byte MinorVersion
        {
            get { return header.MinorVersion; }
            set
            {
                PeImage image = stream.netheader.assembly.peImage;
                image.SetOffset(StreamOffset + Structures.DataOffsets[typeof(Structures.METADATA_TABLE_HEADER)][2]);
                image.writer.Write(value);
                header.MinorVersion = value;
            }
        }
        public byte HeapOffsetSizes
        {
            get { return header.HeapOffsetSizes; }
            set
            {
                PeImage image = stream.netheader.assembly.peImage;
                image.SetOffset(StreamOffset + Structures.DataOffsets[typeof(Structures.METADATA_TABLE_HEADER)][3]);
                image.writer.Write(value);
                header.HeapOffsetSizes = value;
            }
        }
        public ulong MaskSorted
        {
            get { return header.MaskSorted; }
            set
            {
                PeImage image = stream.netheader.assembly.peImage;
                image.SetOffset(StreamOffset + Structures.DataOffsets[typeof(Structures.METADATA_TABLE_HEADER)][6]);
                image.writer.Write(value);
                header.MaskSorted = value;
            }
        }
        public ulong MaskValid
        {
            get { return header.MaskValid; }
            set
            {
                PeImage image = stream.netheader.assembly.peImage;
                image.SetOffset(StreamOffset + Structures.DataOffsets[typeof(Structures.METADATA_TABLE_HEADER)][5]);
                image.writer.Write(value);
                header.MaskValid = value;
            }
        }
        
        public MetaDataTable[] Tables
        {
            get { return tables.ToArray(); }
        }
        public int TableCount
        {
            get { return tablecount; }
        }
        public bool HasTable(MetaDataTableType table)
        {
            return ((this.MaskValid >> (byte)table) & (ulong)1) == 1;
        }

        public void ApplyChanges()
        {
            foreach (MetaDataTable table in tables)
                table.ApplyChanges();
        }

        public MetaDataTable GetTable(MetaDataTableType type)
        {
            if (!HasTable(type))
                return null;
            return tables.FirstOrDefault(t => t != null && t.type == type);
        }

        public override void Dispose()
        {
            foreach (var table in tables)
                if (table != null)
                    table.Dispose();

            Array.Clear(tables, 0, tables.Length);
            tables = null;
            
            tablereader.Dispose();
        }
    }
}
