using System.Collections.Generic;

namespace AsmResolver.DotNet.Blob
{
    /// <summary>
    /// Provides a mechanism for comparing signatures of members defined in a .NET assembly by their contents. 
    /// </summary>
    public partial class SignatureComparer :
        IEqualityComparer<byte[]>
    {
        private const int ElementTypeOffset = 24;
        
        /// <inheritdoc />
        public bool Equals(byte[] x, byte[] y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null) || x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public int GetHashCode(byte[] obj)
        {
            int checksum = 0;
            for (int i = 0; i < obj.Length; i++)
                checksum ^= obj[i];
            return checksum;
        }
    }
}