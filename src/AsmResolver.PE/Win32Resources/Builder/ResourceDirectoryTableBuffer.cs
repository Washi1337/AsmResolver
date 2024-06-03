using System;
using System.Linq;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Builder
{
    /// <summary>
    /// Provides a mechanism for building a table of directory entries in a resource directory.
    /// </summary>
    public class ResourceDirectoryTableBuffer : ResourceTableBuffer<ResourceDirectory>
    {
        private readonly ResourceTableBuffer<string> _nameTable;
        private readonly ResourceTableBuffer<ResourceData> _dataEntryTable;

        /// <summary>
        /// Creates a new resource directory table buffer.
        /// </summary>
        /// <param name="parentBuffer">The resource directory segment that contains the table buffer.</param>
        /// <param name="nameTable">The table containing the names of each named entry.</param>
        /// <param name="dataEntryTable">The table containing the structures of each data entry.</param>
        public ResourceDirectoryTableBuffer(
            ISegment parentBuffer,
            ResourceTableBuffer<string> nameTable,
            ResourceTableBuffer<ResourceData> dataEntryTable)
            : base(parentBuffer)
        {
            _nameTable = nameTable ?? throw new ArgumentNullException(nameof(nameTable));
            _dataEntryTable = dataEntryTable ?? throw new ArgumentNullException(nameof(dataEntryTable));
        }

        /// <inheritdoc />
        public override uint GetEntrySize(ResourceDirectory entry) =>
            SerializedResourceDirectory.ResourceDirectorySize
            + (uint) entry.Entries.Count * ResourceDirectoryEntry.EntrySize;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            foreach (var entry in Entries)
                WriteDirectory(writer, entry);
        }

        private void WriteDirectory(IBinaryStreamWriter writer, ResourceDirectory directory)
        {
            ushort namedEntries = (ushort) directory.Entries.Count(e => e.Name != null);
            ushort idEntries = (ushort) (directory.Entries.Count - namedEntries);

            writer.WriteUInt32(directory.Characteristics);
            writer.WriteUInt32(directory.TimeDateStamp);
            writer.WriteUInt16(directory.MajorVersion);
            writer.WriteUInt16(directory.MinorVersion);
            writer.WriteUInt16(namedEntries);
            writer.WriteUInt16(idEntries);

            foreach (var entry in directory.Entries)
                WriteEntry(writer, entry);
        }

        private void WriteEntry(IBinaryStreamWriter writer, IResourceEntry entry)
        {
            writer.WriteUInt32(entry.Name != null
                ? _nameTable.GetEntryOffset(entry.Name) | 0x8000_0000
                : entry.Id);

            writer.WriteUInt32(entry.IsDirectory
                ? (GetEntryOffset((ResourceDirectory) entry) | 0x8000_0000)
                : _dataEntryTable.GetEntryOffset((ResourceData) entry));
        }

    }
}
