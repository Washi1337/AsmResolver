using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="FrameProcedureSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedFrameProcedureSymbol : FrameProcedureSymbol
{
    /// <summary>
    /// Reads a frame proceudre symbol from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedFrameProcedureSymbol(BinaryStreamReader reader)
    {
        FrameSize = reader.ReadUInt32();
        PaddingSize = reader.ReadUInt32();
        PaddingOffset = reader.ReadInt32();
        CalleeSavesSize = reader.ReadUInt32();
        ExceptionHandlerOffset = reader.ReadInt32();
        ExceptionHandlerSection = reader.ReadUInt16();
        Attributes = (FrameProcedureAttributes) reader.ReadUInt32();
    }
}
