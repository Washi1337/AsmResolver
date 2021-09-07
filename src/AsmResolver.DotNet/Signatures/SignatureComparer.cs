using System.Collections.Generic;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides a mechanism for comparing signatures of members defined in a .NET assembly by their contents.
    /// </summary>
    public partial class SignatureComparer :
        IEqualityComparer<byte[]>
    {
        private const int ElementTypeOffset = 24;

        /// <inheritdoc />
        public bool Equals(byte[]? x, byte[]? y) => ByteArrayEqualityComparer.Instance.Equals(x, y);

        /// <inheritdoc />
        public int GetHashCode(byte[] obj) => ByteArrayEqualityComparer.Instance.GetHashCode(obj);
    }
}
