using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net
{
    public class VTablesDirectory : FileSegment
    {
        public static VTablesDirectory FromReadingContext(ReadingContext readingContext)
        {
            return new VTablesDirectory()
            {
                _readingContext = readingContext
            };
        }

        private ReadingContext _readingContext;
        private List<VTableHeader> _tableHeaders;
         
        public IList<VTableHeader> VTableHeaderHeaders
        {
            get
            {
                if (_tableHeaders != null)
                    return _tableHeaders;
                _tableHeaders = new List<VTableHeader>();
                if (_readingContext != null)
                {
                    while (_readingContext.Reader.Position < _readingContext.Reader.StartPosition + _readingContext.Reader.Length)
                    {
                        _tableHeaders.Add(VTableHeader.FromReadingContext(_readingContext));
                    }
                }

                return _tableHeaders;
            }
        }
        
        public override uint GetPhysicalLength()
        {
            return (uint) VTableHeaderHeaders.Sum(x => x.GetPhysicalLength());
        }

        public override void Write(WritingContext context)
        {
            foreach (var table in VTableHeaderHeaders)
                table.Write(context);
        }
    }

    public class VTableHeader : FileSegment
    {
        public static VTableHeader FromReadingContext(ReadingContext readingContext)
        {
            var tableHeader = new VTableHeader();
            tableHeader.Rva = readingContext.Reader.ReadUInt32();
            ushort size = readingContext.Reader.ReadUInt16();
            tableHeader.Attributes = (VTableAttributes) readingContext.Reader.ReadUInt16();

            long fileOffset = readingContext.Assembly.RvaToFileOffset(tableHeader.Rva);
            var tokensReader = readingContext.Reader.CreateSubReader(fileOffset, size * (tableHeader.Is32Bit ? sizeof (int) : sizeof (long)));

            for (int i = 0; i < size; i++)
            {
                var token = new MetadataToken(tokensReader.ReadUInt32());
                MetadataRow row;
                if (readingContext.Assembly.NetDirectory.MetadataHeader.GetStream<TableStream>()
                    .TryResolveRow(token, out row))
                {
                    tableHeader.Table.Add(row);
                }

                if (tableHeader.Is64Bit)
                    tokensReader.ReadUInt32();
            }

            return tableHeader;
        }

        public VTableHeader()
        {
            Table = new List<MetadataRow>();
        }

        public uint Rva
        {
            get;
            set;
        }
        
        public VTableAttributes Attributes
        {
            get;
            set;
        }

        public bool Is32Bit
        {
            get { return (Attributes & VTableAttributes.Is32Bit) != 0; }
        }

        public bool Is64Bit
        {
            get { return (Attributes & VTableAttributes.Is64Bit) != 0; }
        }

        public bool IsFromUnmanaged
        {
            get { return (Attributes & VTableAttributes.FromUnmanaged) != 0; }
        }

        public bool IsCallMostDerived
        {
            get { return (Attributes & VTableAttributes.CallMostDerived) != 0; }
        }

        public IList<MetadataRow> Table
        {
            get;
            private set;
        }

        public override uint GetPhysicalLength()
        {
            return 1 * sizeof (uint)
                   + 2 * sizeof (ushort);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteUInt32(Rva);
            writer.WriteUInt16((ushort)Table.Count);
            writer.WriteUInt16((ushort)Attributes);
        }
    }

    [Flags]
    public enum VTableAttributes : ushort
    {
        Is32Bit = 0x1,

        Is64Bit = 0x2,

        FromUnmanaged = 0x4,

        CallMostDerived = 0x10,
    }
}
