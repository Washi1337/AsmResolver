using System;
using System.Runtime.Serialization;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Describes the exception that occurs upon encountering an invalid or incomplete blob signature. 
    /// </summary>
    [Serializable]
    public class InvalidBlobSignatureException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="InvalidBlobSignatureException"/>.
        /// </summary>
        /// <param name="signature">The invalid or incomplete signature.</param>
        public InvalidBlobSignatureException(BlobSignature signature)
            : this(signature, "Blob signature is invalid or incomplete.", null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="InvalidBlobSignatureException"/>.
        /// </summary>
        /// <param name="signature">The invalid or incomplete signature.</param>
        /// <param name="message">The error message.</param>
        public InvalidBlobSignatureException(BlobSignature signature, string message)
            : this(signature, message, null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="InvalidBlobSignatureException"/>.
        /// </summary>
        /// <param name="signature">The invalid or incomplete signature.</param>
        /// <param name="inner">The inner cause of the invalid blob signature.</param>
        public InvalidBlobSignatureException(BlobSignature signature, Exception inner)
            : this(signature, $"Blob signature {signature.SafeToString()} is invalid or incomplete.", inner)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="InvalidBlobSignatureException"/>.
        /// </summary>
        /// <param name="signature">The invalid or incomplete signature.</param>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner cause of the invalid blob signature.</param>
        public InvalidBlobSignatureException(BlobSignature signature, string message, Exception inner)
            : base(message, inner)
        {
            Signature = signature;
        }

        /// <summary>
        /// Gets the invalid or incomplete blob signature.
        /// </summary>
        public BlobSignature Signature
        {
            get;
        }
    }
    
}