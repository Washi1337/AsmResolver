using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="LabelTypeRecord"/> that is read from a PDB image.
/// </summary>
public class SerializedLabelTypeRecord : LabelTypeRecord
{
    /// <summary>
    /// Reads a label type from the provided input stream.
    /// </summary>
    /// <param name="typeIndex">The index to assign to the type.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedLabelTypeRecord(uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        Mode = (LabelAddressingMode) reader.ReadUInt16();
    }
}
