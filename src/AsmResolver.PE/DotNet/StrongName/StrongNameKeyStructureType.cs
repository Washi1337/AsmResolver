namespace AsmResolver.PE.DotNet.StrongName
{
    /// <summary>
    /// Provides members for all predefined types of public key structures used by cryptographic service providers (CSP).
    /// </summary>
    public enum StrongNameKeyStructureType : byte
    {
        /// <summary>
        /// The key is a session key.
        /// </summary>
        SimpleBlob = 0x1,

        /// <summary>
        /// The key is a public key.
        /// </summary>
        PublicKeyBlob = 0x6,

        /// <summary>
        /// The key is a public/private key pair.
        /// </summary>
        PrivateKeyBlob = 0x7,

        /// <summary>
        /// The key is a session key.
        /// </summary>
        PlainTextKeyBlob = 0x8,

        /// <summary>
        /// The key is a session key.
        /// </summary>
        OpaqueKeyBlob = 0x9,

        /// <summary>
        /// The key is a public key.
        /// </summary>
        PublicKeyBlobEx = 0xA,

        /// <summary>
        /// The key is a symmetric key.
        /// </summary>
        SymmetricWrapKeyBlob = 0xB,
        
        /// <summary>
        /// The BLOB is a key state BLOB.
        /// </summary>
        KeyStateBlob = 0xC,
    }
}