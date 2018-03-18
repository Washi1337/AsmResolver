using System;
using System.Collections.Generic;
using AsmResolver.Emit;

namespace AsmResolver.Net.Emit
{
    public class VTableFixupsBuffer
    {
        private readonly SimpleFileSegmentBuilder _entriesTable = new SimpleFileSegmentBuilder();

        private readonly IDictionary<VTableHeader, DataSegment> _headerToTables =
            new Dictionary<VTableHeader, DataSegment>();

        public VTableFixupsBuffer(VTablesDirectory directory)
        {
            Directory = directory;
            foreach (var header in directory.VTableHeaders)
                AddVTableHeader(header);
        }

        public VTablesDirectory Directory
        {
            get;
            private set;
        }

        public FileSegment EntriesTable
        {
            get { return _entriesTable; }
        }

        public void AddVTableHeader(VTableHeader header)
        {
            if (!header.Is32Bit && !header.Is64Bit)
                throw new ArgumentException("VTable header must be marked either 32 bits or 64 bits.");
            
            if (header.Is32Bit && header.Is64Bit)
                throw new ArgumentException("VTable header cannot be 32 bits and 64 bits simultanuously.");
            
            var rawTable = new byte[header.Table.Count * (header.Is32Bit ? sizeof(uint) : sizeof(ulong))];
            for (int index = 0; index < header.Table.Count; index++)
            {
                var entry = header.Table[index];
                var rawEntry = header.Is64Bit
                    ? BitConverter.GetBytes((ulong) entry.MetadataToken.ToUInt32())
                    : BitConverter.GetBytes(entry.MetadataToken.ToUInt32());
                Buffer.BlockCopy(rawEntry, 0, rawTable, index * rawEntry.Length, rawEntry.Length);
            }

            var tableSegment = new DataSegment(rawTable);
            _entriesTable.Segments.Add(tableSegment);
            _headerToTables[header] = tableSegment;
        }

        public void UpdateTableRvas(EmitContext context)
        {
            foreach (var pair in _headerToTables)
                pair.Key.Rva = (uint) context.Builder.Assembly.FileOffsetToRva(pair.Value.StartOffset);
        }
    }
}