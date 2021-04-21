using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Builder
{
    /// <summary>
    /// Provides a mechanism for building a table of data entries in a resource directory.
    /// </summary>
    public class ResourceDataTableBuffer : ResourceTableBuffer<IResourceData>
    {
        /// <summary>
        /// Creates a new data table buffer.
        /// </summary>
        /// <param name="parentBuffer">The resource directory segment that contains the table buffer.</param>
        public ResourceDataTableBuffer(ISegment parentBuffer)
            : base(parentBuffer)
        {
        }

        /// <inheritdoc />
        public override uint GetEntrySize(IResourceData entry) => SerializedResourceData.ResourceDataEntrySize;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            foreach (var entry in Entries)
                WriteDataEntry(writer, entry);
        }

        private static void WriteDataEntry(IBinaryStreamWriter writer, IResourceData entry)
        {
            writer.WriteUInt32(entry.Contents.Rva);
            writer.WriteUInt32(entry.Contents.GetPhysicalSize());
            writer.WriteUInt32(entry.CodePage);
            writer.WriteUInt32(0);
        }

    }
}
