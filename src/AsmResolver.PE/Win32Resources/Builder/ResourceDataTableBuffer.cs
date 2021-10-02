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
            for (int i = 0; i < Entries.Count; i++)
                WriteDataEntry(writer, Entries[i]);
        }

        private static void WriteDataEntry(IBinaryStreamWriter writer, IResourceData entry)
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
