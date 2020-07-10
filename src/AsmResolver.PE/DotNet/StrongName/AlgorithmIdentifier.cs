namespace AsmResolver.PE.DotNet.StrongName
{
    // Reference
    // https://docs.microsoft.com/en-us/windows/win32/seccrypto/alg-id
    
    public enum AlgorithmIdentifier : uint
    {
        /// <summary>
        /// RSA public key signature algorithm. This algorithm is supported by the Microsoft Base Cryptographic Provider.
        /// </summary>
        RsaSign = 0x00002400
    }
}