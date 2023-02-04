using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Records.Serialized;
using static AsmResolver.Symbols.Pdb.Records.CodeViewSymbolType;

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
            BuildInfo => new SerializedBuildInfoSymbol(context, dataReader),
            Compile2 => new SerializedCompile2Symbol(dataReader),
            Compile3 => new SerializedCompile3Symbol(dataReader),
            Constant => new SerializedConstantSymbol(context, dataReader),
            Local => new SerializedLocalSymbol(context, dataReader),
            LProcRef => new SerializedProcedureReferenceSymbol(dataReader, true),
            ObjName => new SerializedObjectNameSymbol(dataReader),
            ProcRef => new SerializedProcedureReferenceSymbol(dataReader, false),
            Pub32 => new SerializedPublicSymbol(dataReader),
            Udt => new SerializedUserDefinedTypeSymbol(context, dataReader),
            _ => new UnknownSymbol(type, dataReader.ReadToEnd())
        };
    }
}
