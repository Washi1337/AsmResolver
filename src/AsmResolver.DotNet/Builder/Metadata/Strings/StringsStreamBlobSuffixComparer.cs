using System.Collections.Generic;

namespace AsmResolver.DotNet.Builder.Metadata.Strings
{
    /// <summary>
    /// Provides an implementation of a string comparer that groups strings with the same suffix next to each other.
    /// The largest string is ordered first, and any string that is a suffix of this large string will follow in
    /// descending order.
    /// </summary>
    internal sealed class StringsStreamBlobSuffixComparer : IComparer<byte[]>, IComparer<StringsStreamBlob>,  IComparer<KeyValuePair<Utf8String, StringIndex>>
    {
        /// <summary>
        /// Gets the instance of the suffix comparer.
        /// </summary>
        public static StringsStreamBlobSuffixComparer Instance
        {
            get;
        } = new();

        /// <inheritdoc />
        public int Compare(StringsStreamBlob x, StringsStreamBlob y)
        {
            return Compare(x.Blob.GetBytesUnsafe(), y.Blob.GetBytesUnsafe());
        }

        public int Compare(KeyValuePair<Utf8String, StringIndex> x, KeyValuePair<Utf8String, StringIndex> y)
        {
            return Compare(x.Key.GetBytesUnsafe(), y.Key.GetBytesUnsafe());
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

            for (int i = x.Length - 1, j = y.Length - 1; i >= 0 && j >= 0; i--, j--)
            {
                int charComparison = x[i].CompareTo(y[j]);
                if (charComparison != 0)
                    return charComparison;
            }

            return y.Length.CompareTo(x.Length);
        }
    }
}
