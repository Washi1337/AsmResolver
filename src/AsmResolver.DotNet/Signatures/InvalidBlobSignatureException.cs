using System;
using System.Runtime.Serialization;

namespace AsmResolver.DotNet.Signatures
{
    [Serializable]
    public class InvalidBlobSignatureException : Exception
    {
        public InvalidBlobSignatureException()
        {
        }

        public InvalidBlobSignatureException(BlobSignature signature)
            : this(signature, "Blob signature is invalid or incomplete.", null)
        {
        }

        public InvalidBlobSignatureException(BlobSignature signature, string message)
            : this(signature, message, null)
        {
        }

        public InvalidBlobSignatureException(BlobSignature signature, Exception inner)
            : this(signature, $"Blob signature {signature.SafeToString()} is invalid or incomplete.", inner)
        {
        }

        public InvalidBlobSignatureException(BlobSignature signature, string message, Exception inner)
            : base(message, inner)
        {
            Signature = signature;
        }

        public BlobSignature Signature
        {
            get;
        }
    }
    
}