using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Leaves.Serialized;
using static AsmResolver.Symbols.Pdb.Leaves.CodeViewLeafKind;

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
            Array => new SerializedArrayType(context, typeIndex, dataReader),
            ArgList => new SerializedArgumentList(context, typeIndex, dataReader),
            BClass => new SerializedBaseClass(context, typeIndex, ref dataReader),
            Class => new SerializedClassType(Class, context, typeIndex, dataReader),
            Enum => new SerializedEnumType(context, typeIndex, dataReader),
            Enumerate => new SerializedEnumerateField(context, typeIndex, ref dataReader),
            CodeViewLeafKind.FieldList => new SerializedFieldList(context, typeIndex, dataReader),
            Interface => new SerializedClassType(Interface, context, typeIndex, dataReader),
            Member => new SerializedInstanceDataMember(context, typeIndex, ref dataReader),
            Method => new SerializedOverloadedMethod(context, typeIndex, ref dataReader),
            CodeViewLeafKind.MethodList => new SerializedMethodList(context, typeIndex, dataReader),
            MFunction => new SerializedMemberFunction(context, typeIndex, dataReader),
            Modifier => new SerializedModifierType(context, typeIndex, dataReader),
            NestType or NestTypeEx => new SerializedNestedType(context, typeIndex, ref dataReader),
            OneMethod => new SerializedNonOverloadedMethod(context, typeIndex, ref dataReader),
            Pointer => new SerializedPointerType(context, typeIndex, dataReader),
            Structure => new SerializedClassType(Structure, context, typeIndex, dataReader),
            Union => new SerializedUnionType(context, typeIndex, dataReader),
            VTShape => new SerializedVTableShape(context, typeIndex, dataReader),
            _ => new UnknownCodeViewLeaf(kind, dataReader.ReadToEnd())
        };
    }

    internal static object ReadNumeric(ref BinaryStreamReader reader)
    {
        var kind = (CodeViewLeafKind) reader.ReadUInt16();
        return kind switch
        {
            < Numeric => (object) (uint) kind,
            Char => (char) reader.ReadByte(),
            Short => reader.ReadInt16(),
            UShort => reader.ReadUInt16(),
            Long => reader.ReadInt32(),
            ULong => reader.ReadUInt32(),
            QuadWord => reader.ReadInt64(),
            UQuadWord => reader.ReadUInt64(),
            _ => 0
        };
    }
}
