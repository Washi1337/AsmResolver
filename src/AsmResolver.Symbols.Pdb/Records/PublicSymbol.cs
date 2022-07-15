using System.Text;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a public symbol stored in a PDB symbol stream.
/// </summary>
public class PublicSymbol : SymbolRecord
{
    /// <summary>
    /// Creates a new public symbol.
    /// </summary>
    /// <param name="segment">The segment index.</param>
    /// <param name="offset">The offset within the segment the symbol starts at.</param>
    /// <param name="name">The name of the symbol.</param>
    /// <param name="attributes">The attributes associated to the symbol.</param>
    public PublicSymbol(ushort segment, uint offset, string name, PublicSymbolAttributes attributes)
    {
        Segment = segment;
        Offset = offset;
        Name = name;
        Attributes = attributes;
    }

    /// <inheritdoc />
    public override SymbolType SymbolType => SymbolType.Pub32;

    /// <summary>
    /// Gets or sets the file segment index this symbol is located in.
    /// </summary>
    public ushort Segment
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset within the file that this symbol is defined at.
    /// </summary>
    public uint Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets attributes associated to the public symbol.
    /// </summary>
    public PublicSymbolAttributes Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the symbol is a code symbol.
    /// </summary>
    public bool IsCode
    {
        get => (Attributes & PublicSymbolAttributes.Code) != 0;
        set => Attributes = (Attributes & ~PublicSymbolAttributes.Code)
                            | (value ? PublicSymbolAttributes.Code : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the symbol is a function symbol.
    /// </summary>
    public bool IsFunction
    {
        get => (Attributes & PublicSymbolAttributes.Function) != 0;
        set => Attributes = (Attributes & ~PublicSymbolAttributes.Function)
                            | (value ? PublicSymbolAttributes.Function : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the symbol involves managed code.
    /// </summary>
    public bool IsManaged
    {
        get => (Attributes & PublicSymbolAttributes.Managed) != 0;
        set => Attributes = (Attributes & ~PublicSymbolAttributes.Managed)
                            | (value ? PublicSymbolAttributes.Managed : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the symbol involves MSIL code.
    /// </summary>
    public bool IsMsil
    {
        get => (Attributes & PublicSymbolAttributes.Msil) != 0;
        set => Attributes = (Attributes & ~PublicSymbolAttributes.Msil)
                            | (value ? PublicSymbolAttributes.Msil : 0);
    }

    /// <summary>
    /// Gets or sets the name of the symbol.
    /// </summary>
    public string Name
    {
        get;
        set;
    }

    internal new static PublicSymbol FromReader(ref BinaryStreamReader reader)
    {
        var attributes = (PublicSymbolAttributes) reader.ReadUInt32();
        uint offset = reader.ReadUInt32();
        ushort segment = reader.ReadUInt16();
        string name = Encoding.ASCII.GetString(reader.ReadToEnd());

        return new PublicSymbol(segment, offset, name, attributes);
    }

    /// <inheritdoc />
    public override string ToString() => $"{SymbolType}: [{Segment:X4}:{Offset:X8}] {Name}";
}
