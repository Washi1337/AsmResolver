using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver
{
    /// <summary>
    /// Provides an implementation to compare byte arrays for equality.
    /// </summary>
    public class ByteArrayEqualityComparer
        : IEqualityComparer<byte[]>
        , IComparer<byte[]>
#if NET9_0_OR_GREATER
        , IAlternateEqualityComparer<ReadOnlySpan<byte>, byte[]>
#endif
    {
        /// <summary>
        /// Gets the singleton instance of this comparer.
        /// </summary>
        public static ByteArrayEqualityComparer Instance
        {
            get;
        } = new();

        private ByteArrayEqualityComparer()
        {
        }

        /// <inheritdoc />
        public bool Equals(byte[]? x, byte[]? y)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            return x.AsSpan().SequenceEqual(y);
#else
            if (x is null || y is null)
                return x == y;

            return x.SequenceEqual(y);
#endif
        }

        /// <inheritdoc />
        public int GetHashCode(byte[] obj)
        {
            unchecked
            {
                int result = 0;
                foreach (byte b in obj)
                    result = (result * 31) ^ b;
                return result;
            }
        }

        /// <inheritdoc />
        public int Compare(byte[]? x, byte[]? y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            if (x is null)
                return -1;
            if (y is null)
                return 1;

            int length = Math.Min(x.Length, y.Length);
            for (int i = 0; i < length; i++)
            {
                int result = x[i].CompareTo(y[i]);
                if (result != 0)
                    return result;
            }

            return x.Length.CompareTo(y.Length);
        }

#if NET9_0_OR_GREATER
        /// <inheritdoc />
        public bool Equals(ReadOnlySpan<byte> alternate, byte[] other) => alternate.SequenceEqual(other);

        /// <inheritdoc />
        public int GetHashCode(ReadOnlySpan<byte> alternate)
        {
            unchecked
            {
                int result = 0;
                foreach (byte b in alternate)
                    result = (result * 31) ^ b;
                return result;
            }
        }

        /// <inheritdoc />
        public byte[] Create(ReadOnlySpan<byte> alternate) => alternate.ToArray();
#endif
    }
}
