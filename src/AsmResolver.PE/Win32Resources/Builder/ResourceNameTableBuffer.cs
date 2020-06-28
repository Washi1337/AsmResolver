using System.Text;

namespace AsmResolver.PE.Win32Resources.Builder
{
    /// <summary>
    /// Provides a mechanism for building a table of names that can be used in a resource directory.
    /// </summary>
    public class ResourceNameTableBuffer : ResourceTableBuffer<string>
    {
        /// <summary>
        /// Creates a new name table buffer.
        /// </summary>
        /// <param name="parentBuffer">The resource directory segment that contains the name table buffer.</param>
        public ResourceNameTableBuffer(ISegment parentBuffer) 
            : base(parentBuffer)
        {
        }

        /// <inheritdoc />
        public override uint GetEntrySize(string entry) => sizeof(ushort) + (uint) Encoding.Unicode.GetByteCount(entry);

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            foreach (string name in Entries)
            {;
                writer.WriteUInt16((ushort) name.Length);
                var buffer = Encoding.Unicode.GetBytes(name);
                writer.WriteBytes(buffer, 0, buffer.Length);
            }
        }
    }
}