using System;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Builder
{
    /// <summary>
    /// Provides a mechanism for building a new resource directory in a PE file.
    /// </summary>
    public class ResourceDirectoryBuffer : ISegment
    {
        private readonly SegmentBuilder _segments;

        /// <summary>
        /// Creates a new resource directory buffer.
        /// </summary>
        public ResourceDirectoryBuffer()
        {
            NameTable = new ResourceNameTableBuffer(this);
            DataEntryTable = new ResourceDataTableBuffer(this);
            DirectoryTable = new ResourceDirectoryTableBuffer(this, NameTable, DataEntryTable);
            DataTable = new SegmentBuilder();

            _segments = new SegmentBuilder
            {
                DirectoryTable,
                NameTable,
                DataEntryTable,
                DataTable
            };
        }

        /// <inheritdoc />
        public ulong Offset => _segments.Offset;

        /// <inheritdoc />
        public uint Rva => _segments.Rva;

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <summary>
        /// Gets the segment containing the table with all directory entries.
        /// </summary>
        public ResourceTableBuffer<IResourceDirectory> DirectoryTable
        {
            get;
        }

        /// <summary>
        /// Gets the segment containing the table with all data entries.
        /// </summary>
        public ResourceTableBuffer<IResourceData> DataEntryTable
        {
            get;
        }

        /// <summary>
        /// Gets the segment containing the table with the names for all named resource entries.
        /// </summary>
        public ResourceTableBuffer<string> NameTable
        {
            get;
        }

        /// <summary>
        /// Gets the segment containing all raw data segments for each data entry.
        /// </summary>
        public SegmentBuilder DataTable
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether there is any data added to the buffer.
        /// </summary>
        public bool IsEmpty => DirectoryTable.IsEmpty;

        /// <summary>
        /// Adds a resource directory and all its sub entries to the buffer.
        /// </summary>
        /// <param name="directory">The directory to add.</param>
        public void AddDirectory(IResourceDirectory directory)
        {
            DirectoryTable.AddEntry(directory);
            if (directory.Name != null)
                NameTable.AddEntry(directory.Name);

            foreach (var entry in directory.Entries)
                AddEntry(entry);
        }

        private void AddEntry(IResourceEntry entry)
        {
            if (entry.IsDirectory)
                AddDirectory((IResourceDirectory) entry);
            else if (entry.IsData)
                AddDataEntry(entry);
            else
                throw new NotSupportedException();
        }

        private void AddDataEntry(IResourceEntry entry)
        {
            var data = (IResourceData) entry;
            DataEntryTable.AddEntry(data);

            if (data.Contents is not null)
                DataTable.Add(data.Contents, 4);
        }

        /// <inheritdoc />
        public void UpdateOffsets(in RelocationParameters parameters) => _segments.UpdateOffsets(parameters);

        /// <inheritdoc />
        public uint GetPhysicalSize() => _segments.GetPhysicalSize();

        /// <inheritdoc />
        public uint GetVirtualSize() => _segments.GetVirtualSize();

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer) => _segments.Write(writer);
    }
}
