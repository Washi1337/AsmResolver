namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Represents the type of checksum that is stored in a CodeView C13 checksums section.
/// </summary>
public enum C13FileChecksumType : byte
{
    /// <summary>
    /// Indicates no checksum was stored.
    /// </summary>
    None,

    /// <summary>
    /// Indicates the checksum is an MD5 digest.
    /// </summary>
    Md5,

    /// <summary>
    /// Indicates the checksum is a SHA1 digest.
    /// </summary>
    Sha1,

    /// <summary>
    /// Indicates the checksum is a SHA256 digest.
    /// </summary>
    Sha256
}
