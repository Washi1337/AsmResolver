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
        /// <summary>
        /// Rounds the provided unsigned integer up to the nearest multiple of the provided alignment.
        /// </summary>
        /// <param name="value">The value to align.</param>
        /// <param name="alignment">The alignment. Must be a power of 2.</param>
        /// <returns>The aligned value.</returns>
        public static uint Align(this uint value, uint alignment)
        {
            alignment--;
            return (value + alignment) & ~alignment;
        }

        /// <summary>
        /// Rounds the provided unsigned integer up to the nearest multiple of the provided alignment.
        /// </summary>
        /// <param name="value">The value to align.</param>
        /// <param name="alignment">The alignment. Must be a power of 2.</param>
        /// <returns>The aligned value.</returns>
        public static ulong Align(this ulong value, ulong alignment)
        {
            alignment--;
            return (value + alignment) & ~alignment;
        }

        /// <summary>
        /// Computes the number of bytes the provided integer would require after compressing it using the integer
        /// compression as specified in ECMA-335.
        /// </summary>
        /// <param name="value">The integer to determine the compressed size of.</param>
        /// <returns>The number of bytes the value would require.</returns>
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
