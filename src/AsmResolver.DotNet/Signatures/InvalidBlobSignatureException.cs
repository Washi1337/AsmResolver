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
            : this(signature, null)
        {
        }

        public InvalidBlobSignatureException(BlobSignature signature, Exception inner)
            : base($"Blob signature {signature.SafeToString()} is invalid or incomplete.", inner)
        {
            Signature = signature;
        }
        
        public BlobSignature Signature
        {
            get;
        }
    }
    
}