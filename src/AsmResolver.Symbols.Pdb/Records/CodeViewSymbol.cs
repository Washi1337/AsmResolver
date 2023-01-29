using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Records.Serialized;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a single symbol record within the symbol record stream of a PDB file.
/// </summary>
public abstract class CodeViewSymbol
{
    /// <summary>
    /// Gets the type of symbol this record encodes.
    /// </summary>
    public abstract CodeViewSymbolType CodeViewSymbolType
    {
        get;
    }

    /// <summary>
    /// Reads a single symbol record from the input stream.
    /// </summary>
    /// <param name="context">The reading context in which the symbol is situated in.</param>
    /// <param name="reader">The input stream.</param>
    /// <returns>The read symbol.</returns>
    public static CodeViewSymbol FromReader(PdbReaderContext context, ref BinaryStreamReader reader)
    {
        ushort length = reader.ReadUInt16();
        var type = (CodeViewSymbolType) reader.ReadUInt16();
        var dataReader = reader.ForkRelative(reader.RelativeOffset, (uint) (length - 2));
        reader.Offset += (ulong) (length - 2);

        return type switch
        {
            CodeViewSymbolType.Pub32 => new SerializedPublicSymbol(dataReader),
            CodeViewSymbolType.Udt => new SerializedUserDefinedTypeSymbol(context, dataReader),
            CodeViewSymbolType.Constant => new SerializedConstantSymbol(context, dataReader),
            CodeViewSymbolType.ProcRef => new SerializedProcedureReferenceSymbol(dataReader, false),
            CodeViewSymbolType.LProcRef => new SerializedProcedureReferenceSymbol(dataReader, true),
            _ => new UnknownSymbol(type, dataReader.ReadToEnd())
        };
    }

    private protected T? GetLeafRecord<T>(PdbReaderContext context, uint typeIndex) where T : CodeViewLeaf
    {
        return context.ParentImage.TryGetLeafRecord(typeIndex, out var leaf)
            ? leaf switch
            {
                T t => t,
                UnknownCodeViewLeaf unknownLeaf => context.Parameters.ErrorListener.BadImageAndReturn<T>(
                    $"{CodeViewSymbolType} references a leaf at {typeIndex:X8} that is of an unknown type {unknownLeaf.LeafKind}."),
                _ => context.Parameters.ErrorListener.BadImageAndReturn<T>(
                    $"{CodeViewSymbolType} references a leaf at {typeIndex:X8} that is of an unexpected type ({leaf.LeafKind}).")
            }
            : context.Parameters.ErrorListener.BadImageAndReturn<T>(
                $"{CodeViewSymbolType} contains an invalid type index {typeIndex:X8}.");
    }
}
