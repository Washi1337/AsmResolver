using System;
using System.Linq;
using System.Text;

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
        private const string ReservedStringCharacters = "\\\"\t\r\n\b";

        [ThreadStatic]
        private static StringBuilder? _buffer;

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
        public static uint GetCompressedSize(this uint value) => value switch
        {
            < 0x80 => sizeof(byte),
            < 0x4000 => sizeof(ushort),
            _ => sizeof(uint)
        };

        /// <summary>
        /// Computes the number of bytes the provided integer would require after compressing it using the integer
        /// compression using the 7-bit encoding.
        /// </summary>
        /// <param name="value">The integer to determine the compressed size of.</param>
        /// <returns>The number of bytes the value would require.</returns>
        public static uint Get7BitEncodedSize(this uint value) => value switch
        {
            < 0b1000_0000 => 1,
            < 0b100_0000_0000_0000 => 2,
            < 0b10_0000_0000_0000_0000_0000 => 3,
            < 0b10000_0000_0000_0000_0000_0000_0000 => 4,
            _ => 5
        };

        /// <summary>
        /// Converts a string into an escaped string literal.
        /// </summary>
        /// <param name="literal">The string to convert.</param>
        /// <returns>The escaped string.</returns>
        public static string CreateEscapedString(this string literal)
        {
            _buffer ??= new StringBuilder(literal.Length + 2);
            _buffer.Clear();

            _buffer.Append('"');
            foreach (char currentChar in literal)
            {
                if (ReservedStringCharacters.Contains(currentChar))
                    _buffer.Append('\\');
                _buffer.Append(currentChar);
            }
            _buffer.Append('"');

            return _buffer.ToString();
        }
    }
}
