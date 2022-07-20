using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Types;

/// <summary>
/// Represents a single type record in a TPI or IPI stream.
/// </summary>
public abstract class CodeViewType
{
    /// <summary>
    /// Gets the type kind this record encodes.
    /// </summary>
    public abstract CodeViewTypeKind TypeKind
    {
        get;
    }

    /// <summary>
    /// Gets the type index the type is associated to.
    /// </summary>
    public uint TypeIndex
    {
        get;
        internal set;
    }

    internal static CodeViewType FromReader(PdbReaderContext context, BinaryStreamReader reader)
    {
        ushort length = reader.ReadUInt16();
        var kind = (CodeViewTypeKind) reader.ReadUInt16();

        return kind switch
        {
            _ => new UnknownCodeViewType(kind, reader.ReadToEnd())
        };
    }
}
