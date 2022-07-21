using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Leaves.Serialized;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a single type record in a TPI or IPI stream.
/// </summary>
public abstract class CodeViewLeaf
{
    protected CodeViewLeaf(uint typeIndex)
    {
        TypeIndex = typeIndex;
    }

    /// <summary>
    /// Gets the type kind this record encodes.
    /// </summary>
    public abstract CodeViewLeafKind LeafKind
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

    internal static CodeViewLeaf FromReader(PdbReaderContext context, uint typeIndex, ref BinaryStreamReader reader)
    {
        ushort length = reader.ReadUInt16();
        var dataReader = reader.Fork();
        reader.Offset += length;

        var kind = (CodeViewLeafKind) dataReader.ReadUInt16();

        return kind switch
        {
            CodeViewLeafKind.Enum => new SerializedEnumType(context, typeIndex, dataReader),
            _ => new UnknownCodeViewType(kind, dataReader.ReadToEnd())
        };
    }
}
