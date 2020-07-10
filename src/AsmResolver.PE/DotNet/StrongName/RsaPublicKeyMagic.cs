namespace AsmResolver.PE.DotNet.StrongName
{
    /// <summary>
    /// Provides members for all valid RSA public key structures supported by AsmResolver.
    /// </summary>
    public enum RsaPublicKeyMagic
    {
        /// <summary>
        /// Indicates the structure is a public key.
        /// </summary>
        Rsa1 = 0x31415352,
        
        /// <summary>
        /// Indicates the structure is a private key.
        /// </summary>
        Rsa2 = 0x32415352
    }
}