using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Builder
{
    /// <summary>
    /// Provides a mechanism for building a table of data entries in a resource directory.
    /// </summary>
    public class ResourceDataTableBuffer : ResourceTableBuffer<ResourceData>
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
        public override uint GetEntrySize(ResourceData entry) => SerializedResourceData.ResourceDataEntrySize;

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            for (int i = 0; i < Entries.Count; i++)
                WriteDataEntry(writer, Entries[i]);
        }

        private static void WriteDataEntry(BinaryStreamWriter writer, ResourceData entry)
        {
            if (entry.Contents is null)
            {
                writer.WriteUInt64(0);
            }
            else
            {
                writer.WriteUInt32(entry.Contents.Rva);
                writer.WriteUInt32(entry.Contents.GetPhysicalSize());
            }

            writer.WriteUInt32(entry.CodePage);
            writer.WriteUInt32(0);
        }

    }
}
