using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Leaves.Serialized;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a single leaf record in a TPI or IPI stream.
/// </summary>
public abstract class CodeViewLeaf
{
    /// <summary>
    /// Initializes an empty CodeView leaf.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the leaf.</param>
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

        return FromReaderNoHeader(context, typeIndex, ref dataReader);
    }

    internal static CodeViewLeaf FromReaderNoHeader(
        PdbReaderContext context,
        uint typeIndex,
        ref BinaryStreamReader dataReader)
    {
        var kind = (CodeViewLeafKind) dataReader.ReadUInt16();
        return kind switch
        {
            CodeViewLeafKind.FieldList => new SerializedFieldList(context, typeIndex, dataReader),
            CodeViewLeafKind.Enum => new SerializedEnumType(context, typeIndex, dataReader),
            CodeViewLeafKind.Enumerate => new SerializedEnumerateField(context, typeIndex, ref dataReader),
            CodeViewLeafKind.Pointer => new SerializedPointerType(context, typeIndex, dataReader),
            _ => new UnknownCodeViewType(kind, dataReader.ReadToEnd())
        };
    }
}
