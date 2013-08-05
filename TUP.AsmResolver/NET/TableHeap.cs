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
        internal Structures.METADATA_TABLE_HEADER _header;
        internal NETTableReader _tablereader;
        internal MetaDataTable[] _tables;
        internal int _tablecount;
        internal byte[] _bytes;

        internal TablesHeap(NETHeader netheader, int headeroffset, Structures.METADATA_STREAM_HEADER rawHeader, string name)
            : base(netheader, headeroffset, rawHeader, name)
        {
        }

        internal override void Initialize()
        {
            _tables = new MetaDataTable[45];
            _tablereader = new NETTableReader(this);
        }
        
        internal override void MakeEmpty()
        {
            base.Dispose();
        }

        public override void Dispose()
        {
            foreach (var table in _tables)
                if (table != null)
                    table.Dispose();

            Array.Clear(_tables, 0, _tables.Length);

            _tablereader.Dispose();
            ClearCache();
            base.Dispose();
        }

        public override void ClearCache()
        {

        }

        public byte MajorVersion
        {
            get { return _header.MajorVersion; }
            set
            {
                PeImage image = _netheader._assembly._peImage;
                image.SetOffset(StreamOffset + Structures.DataOffsets[typeof(Structures.METADATA_TABLE_HEADER)][1]);
                image.Writer.Write(value);
                _header.MajorVersion = value;
            }
        }

        public byte MinorVersion
        {
            get { return _header.MinorVersion; }
            set
            {
                PeImage image = _netheader._assembly._peImage;
                image.SetOffset(StreamOffset + Structures.DataOffsets[typeof(Structures.METADATA_TABLE_HEADER)][2]);
                image.Writer.Write(value);
                _header.MinorVersion = value;
            }
        }

        public byte HeapOffsetSizes
        {
            get { return _header.HeapOffsetSizes; }
            set
            {
                PeImage image = _netheader._assembly._peImage;
                image.SetOffset(StreamOffset + Structures.DataOffsets[typeof(Structures.METADATA_TABLE_HEADER)][3]);
                image.Writer.Write(value);
                _header.HeapOffsetSizes = value;
            }
        }

        public ulong MaskSorted
        {
            get { return _header.MaskSorted; }
            set
            {
                PeImage image = _netheader._assembly._peImage;
                image.SetOffset(StreamOffset + Structures.DataOffsets[typeof(Structures.METADATA_TABLE_HEADER)][6]);
                image.Writer.Write(value);
                _header.MaskSorted = value;
            }
        }

        public ulong MaskValid
        {
            get { return _header.MaskValid; }
            set
            {
                PeImage image = _netheader._assembly._peImage;
                image.SetOffset(StreamOffset + Structures.DataOffsets[typeof(Structures.METADATA_TABLE_HEADER)][5]);
                image.Writer.Write(value);
                _header.MaskValid = value;
            }
        }
        
        public MetaDataTable[] Tables
        {
            get { return _tables.ToArray(); }
        }

        public int TableCount
        {
            get { return _tablecount; }
        }

        public bool HasTable(MetaDataTableType table)
        {
            return ((this.MaskValid >> (byte)table) & (ulong)1) == 1;
        }

        public void ApplyChanges()
        {
            foreach (MetaDataTable table in _tables)
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
            return _tables[(int)type];
        }

        public void AddTable(MetaDataTableType type)
        {
            MetaDataTable table = new MetaDataTable(this, true);
            table._type = type;
            _tables[(int)type] = table;
            MaskValid |= ((ulong)1 << (int)type);
        }

        public override byte[] Contents
        {
            get
            {
                if (_bytes == null)
                    _bytes = base.Contents;
                return _bytes;
            }
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
