using AsmResolver.IO;
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
            CodeViewSymbolType.Compile2 => new SerializedCompile2Symbol(dataReader),
            CodeViewSymbolType.Compile3 => new SerializedCompile3Symbol(dataReader),
            CodeViewSymbolType.Constant => new SerializedConstantSymbol(context, dataReader),
            CodeViewSymbolType.LProcRef => new SerializedProcedureReferenceSymbol(dataReader, true),
            CodeViewSymbolType.ObjName => new SerializedObjectNameSymbol(dataReader),
            CodeViewSymbolType.ProcRef => new SerializedProcedureReferenceSymbol(dataReader, false),
            CodeViewSymbolType.Pub32 => new SerializedPublicSymbol(dataReader),
            CodeViewSymbolType.Udt => new SerializedUserDefinedTypeSymbol(context, dataReader),
            _ => new UnknownSymbol(type, dataReader.ReadToEnd())
        };
    }
}
