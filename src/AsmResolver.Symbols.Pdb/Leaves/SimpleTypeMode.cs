namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides members defining all possible modes that a simple type in a PDB image can be set to.
/// </summary>
/// <remarks>
/// Reference: https://llvm.org/docs/PDB/TpiStream.html
/// </remarks>
public enum SimpleTypeMode
{
    /// <summary>
    /// Indicates the type is not a pointer.
    /// </summary>
    Direct = 0,

    /// <summary>
    /// Indicates the type is a near pointer.
    /// </summary>
    NearPointer = 1,

    /// <summary>
    /// Indicates the type is a far pointer.
    /// </summary>
    FarPointer = 2,

    /// <summary>
    /// Indicates the type is a huge pointer.
    /// </summary>
    HugePointer = 3,

    /// <summary>
    /// Indicates the type is a 32 bit near pointer.
    /// </summary>
    NearPointer32 = 4,

    /// <summary>
    /// Indicates the type is a 32 bit far pointer.
    /// </summary>
    FarPointer32 = 5,

    /// <summary>
    /// Indicates the type is a 64 bit near pointer.
    /// </summary>
    NearPointer64 = 6,

    /// <summary>
    /// Indicates the type is a 128 bit near pointer.
    /// </summary>
    NearPointer128 = 7,
}
