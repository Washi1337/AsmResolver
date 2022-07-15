using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a single symbol record within the symbol record stream of a PDB file.
/// </summary>
public abstract class SymbolRecord
{
    /// <summary>
    /// Gets the type of symbol this record encodes.
    /// </summary>
    public abstract SymbolType SymbolType
    {
        get;
    }

    /// <summary>
    /// Reads a single symbol record from the input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <returns>The read symbol.</returns>
    public static SymbolRecord FromReader(ref BinaryStreamReader reader)
    {
        ushort length = reader.ReadUInt16();
        var type = (SymbolType) reader.ReadUInt16();
        var dataReader = reader.ForkRelative(reader.RelativeOffset, (uint) (length - 2));
        reader.Offset += (ulong) (length - 2);

        return type switch
        {
            SymbolType.Pub32 => PublicSymbol.FromReader(ref dataReader),
            SymbolType.Udt => UserDefinedTypeSymbol.FromReader(ref dataReader),
            _ => new UnknownSymbol(type, dataReader.ReadToEnd())
        };
    }
}
