using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a user-defined type symbol in a PDB symbol stream.
/// </summary>
public class UserDefinedTypeSymbol : CodeViewSymbol
{
    /// <summary>
    /// Defines a new user-defined type.
    /// </summary>
    /// <param name="name">The name of the type.</param>
    /// <param name="typeIndex">The type index.</param>
    public UserDefinedTypeSymbol(Utf8String name, uint typeIndex)
    {
        Name = name;
        TypeIndex = typeIndex;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.Udt;

    /// <summary>
    /// Gets or sets the name of the type.
    /// </summary>
    public Utf8String Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the index associated to the type.
    /// </summary>
    public uint TypeIndex
    {
        get;
        set;
    }

    internal new static UserDefinedTypeSymbol FromReader(ref BinaryStreamReader reader)
    {
        uint typeIndex = reader.ReadUInt32();
        var name = new Utf8String(reader.ReadToEnd());

        return new UserDefinedTypeSymbol(name, typeIndex);
    }

    /// <inheritdoc />
    public override string ToString() => $"{CodeViewSymbolType}: [{TypeIndex}] {Name}";
}
