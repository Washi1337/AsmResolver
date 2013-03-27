using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using TUP.AsmResolver.PE;
using TUP.AsmResolver.PE.Readers;
using TUP.AsmResolver.PE.Writers;
using TUP.AsmResolver.NET.Specialized;

namespace TUP.AsmResolver.NET
{
    public class TablesHeap : MetaDataStream
    {
        internal Structures.METADATA_TABLE_HEADER header;
        internal NETTableReader tablereader;
        internal MetaDataTable[] tables;
        internal int tablecount;


        internal TablesHeap(NETHeader netheader, int headeroffset, Structures.METADATA_STREAM_HEADER rawHeader, string name)
            : base(netheader, headeroffset, rawHeader, name)
        {
        }

        internal override void Initialize()
        {
            tables = new MetaDataTable[45];

            NETTableReader reader = new NETTableReader(this);
            reader.ReadTables();
        }

        internal void Reconstruct()
        {
            foreach (MetaDataStream stream in netheader.MetaDataStreams)
                stream.ClearCache();

            NETTableReconstructor reconstructor = new NETTableReconstructor(this);

            // Rebuild all tables and update heaps
            reconstructor.Reconstruct();
        }

        internal override void MakeEmpty()
        {
            base.Dispose();
        }

        public override void Dispose()
        {
            foreach (var table in tables)
                if (table != null)
                    table.Dispose();

            Array.Clear(tables, 0, tables.Length);

            tablereader.Dispose();
            ClearCache();
            base.Dispose();
        }

        public override void ClearCache()
        {

        }

        public byte MajorVersion
        {
            get { return header.MajorVersion; }
            set
            {
                PeImage image = netheader.assembly.peImage;
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
                PeImage image = netheader.assembly.peImage;
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
                PeImage image = netheader.assembly.peImage;
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
                PeImage image = netheader.assembly.peImage;
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
                PeImage image = netheader.assembly.peImage;
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
                if (table != null)
                    table.ApplyChanges();
        }

        public MetaDataTable GetTable(MetaDataTableType type)
        {
            return GetTable(type, false);
        }

        public MetaDataTable GetTable(MetaDataTableType type, bool addIfNotPresent)
        {
            if (!HasTable(type))
            {
                if (addIfNotPresent)
                    AddTable(type);
                else
                    return null;
            }
            return tables[(int)type];
        }

        public void AddTable(MetaDataTableType type)
        {
            MetaDataTable table = new MetaDataTable(this);
            table.type = type;
            tables[(int)type] = table;
            MaskValid |= ((ulong)1 << (int)type);
        }


        public MetaDataTableGroup TypeDefOrRef;
        public MetaDataTableGroup HasConstant;
        public MetaDataTableGroup HasCustomAttribute;
        public MetaDataTableGroup HasFieldMarshall;
        public MetaDataTableGroup HasDeclSecurity;
        public MetaDataTableGroup MemberRefParent;
        public MetaDataTableGroup HasSemantics;
        public MetaDataTableGroup MethodDefOrRef;
        public MetaDataTableGroup MemberForwarded;
        public MetaDataTableGroup Implementation;
        public MetaDataTableGroup CustomAttributeType;
        public MetaDataTableGroup ResolutionScope;
        public MetaDataTableGroup TypeOrMethod;

    }
}
