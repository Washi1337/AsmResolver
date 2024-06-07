using System.Collections;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Msf.Builder;

/// <summary>
/// Represents a block within a MSF file that contains information on which blocks in the MSF file are free to use.
/// </summary>
public class FreeBlockMap : SegmentBase
{
    /// <summary>
    /// Creates a new empty free block map.
    /// </summary>
    /// <param name="blockSize">The size of a single block in the MSF file.</param>
    public FreeBlockMap(uint blockSize)
    {
        BitField = new BitArray((int) blockSize * 8, true);
    }

    /// <summary>
    /// Gets the bit field indicating which blocks in the MSF file are free to use.
    /// </summary>
    public BitArray BitField
    {
        get;
    }

    /// <inheritdoc />
    public override uint GetPhysicalSize() => (uint) (BitField.Count / 8);

    /// <inheritdoc />
    public override void Write(BinaryStreamWriter writer)
    {
        byte[] data = new byte[BitField.Count / 8];
        BitField.CopyTo(data, 0);
        writer.WriteBytes(data);
    }
}
