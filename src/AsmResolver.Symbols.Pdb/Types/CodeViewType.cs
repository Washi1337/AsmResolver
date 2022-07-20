using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Types.Serialized;

namespace AsmResolver.Symbols.Pdb.Types;

/// <summary>
/// Represents a single type record in a TPI or IPI stream.
/// </summary>
public abstract class CodeViewType
{
    protected CodeViewType(uint typeIndex)
    {
        TypeIndex = typeIndex;
    }

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

    internal static CodeViewType FromReader(PdbReaderContext context, uint typeIndex, ref BinaryStreamReader reader)
    {
        ushort length = reader.ReadUInt16();
        var dataReader = reader.Fork();
        reader.Offset += length;

        var kind = (CodeViewTypeKind) dataReader.ReadUInt16();

        return kind switch
        {
            CodeViewTypeKind.Enum => new SerializedEnumType(context, typeIndex, dataReader),
            _ => new UnknownCodeViewType(kind, dataReader.ReadToEnd())
        };
    }
}
