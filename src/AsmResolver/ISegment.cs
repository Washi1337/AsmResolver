namespace AsmResolver
{
    /// <summary>
    /// Represents a single chunk of data residing in a file or memory space.
    /// </summary>
    public interface ISegment : IOffsetProvider, IWritable
    {
        /// <summary>
        /// Computes the number of bytes the segment will contain when it is mapped into memory.
        /// </summary>
        /// <returns>The number of bytes.</returns>
        uint GetVirtualSize();
        
    }

    public static partial class Extensions
    {
        public static uint Align(this uint value, uint alignment)
        {
            alignment--;
            return (value + alignment) & ~alignment;
        }

        public static uint GetCompressedSize(this uint value)
        {
            if (value < 0x80)
                return sizeof(byte);
            if (value < 0x4000)
                return sizeof(ushort);
            return sizeof(uint);
        }
    }
}