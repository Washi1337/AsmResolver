// Disable xmldoc warnings.
#pragma warning disable 1591

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides members defining all hash algorithms that can be used to hash public keys of assemblies.
    /// </summary>
    public enum AssemblyHashAlgorithm : uint
    {
        None = 0x0000,
        Md2 = 0x8001,
        Md4 = 0x8002,
        Md5 = 0x8003,
        Sha1 = 0x8004,
        Mac = 0x8005,
        Ripemd = 0x8006,
        Ripemd160 = 0x8007,
        Ssl3Shamd5 = 0x8008,
        Hmac = 0x8009,
        Tls1Prf = 0x800A,
        HashReplaceOwf = 0x800B,
        Sha256 = 0x800C,
        Sha384 = 0x800D,
        Sha512 = 0x800E,
    }
}
