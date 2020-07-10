namespace AsmResolver.PE.DotNet.StrongName
{
    // Reference
    // https://docs.microsoft.com/en-us/windows/win32/seccrypto/alg-id
    
    /// <summary>
    /// Provides members for identifying cryptographic algorithms supported by AsmResolver. 
    /// </summary>
    public enum SignatureAlgorithm : uint
    {
        /// <summary>
        /// RSA public key signature algorithm. This algorithm is supported by the Microsoft Base Cryptographic Provider.
        /// </summary>
        RsaSign = 0x00002400
    }
}