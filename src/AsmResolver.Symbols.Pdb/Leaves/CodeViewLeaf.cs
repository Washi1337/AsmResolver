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
            Array => new SerializedArrayTypeRecord(context, typeIndex, dataReader),
            ArgList => new SerializedArgumentListLeaf(context, typeIndex, dataReader),
            BClass => new SerializedBaseClassField(context, typeIndex, ref dataReader),
            Class or Interface or Structure => new SerializedClassTypeRecord(kind, context, typeIndex, dataReader),
            BitField => new SerializedBitFieldTypeRecord(context, typeIndex, dataReader),
            Enum => new SerializedEnumTypeRecord(context, typeIndex, dataReader),
            Enumerate => new SerializedEnumerateField(context, typeIndex, ref dataReader),
            FieldList => new SerializedFieldListLeaf(context, typeIndex, dataReader),
            Member => new SerializedInstanceDataField(context, typeIndex, ref dataReader),
            Method => new SerializedOverloadedMethod(context, typeIndex, ref dataReader),
            MethodList => new SerializedMethodListLeaf(context, typeIndex, dataReader),
            MFunction => new SerializedMemberFunctionLeaf(context, typeIndex, dataReader),
            Modifier => new SerializedModifierTypeRecord(context, typeIndex, dataReader),
            NestType or NestTypeEx => new SerializedNestedTypeField(context, typeIndex, ref dataReader),
            OneMethod => new SerializedNonOverloadedMethod(context, typeIndex, ref dataReader),
            Pointer => new SerializedPointerTypeRecord(context, typeIndex, dataReader),
            Procedure => new SerializedProcedureTypeRecord(context, typeIndex, dataReader),
            StMember => new SerializedStaticDataField(context, typeIndex, ref dataReader),
            Union => new SerializedUnionTypeRecord(context, typeIndex, dataReader),
            VFuncTab => new SerializedVTableField(context, typeIndex, ref dataReader),
            VTShape => new SerializedVTableShapeLeaf(context, typeIndex, dataReader),
            VBClass or IVBClass => new SerializedVBaseClassField(context, typeIndex, ref dataReader, kind == IVBClass),
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
            Real32 => reader.ReadSingle(),
            Real64 => reader.ReadDouble(),
            _ => 0
        };
    }
}
