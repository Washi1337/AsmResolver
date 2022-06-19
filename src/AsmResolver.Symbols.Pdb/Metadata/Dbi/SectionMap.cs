using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Dbi;

/// <summary>
/// Represents a single entry in the Section Map sub stream of the DBI stream.
/// </summary>
public class SectionMap : IWritable
{
    /// <summary>
    /// Gets or sets the attributes assigned to this section map.
    /// </summary>
    public SectionMapAttributes Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the logical overlay number of this section map.
    /// </summary>
    public ushort LogicalOverlayNumber
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the group index into the descriptor array.
    /// </summary>
    public ushort Group
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the frame index.
    /// </summary>
    public ushort Frame
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the byte offset of the segment or group name in string table, or 0xFFFF if no name was assigned.
    /// </summary>
    public ushort SectionName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the byte offset of the class in the string table, or 0xFFFF if no name was assigned..
    /// </summary>
    public ushort ClassName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the byte offset of the logical segment within physical segment. If group is set in flags, this is the offset of the group.
    /// </summary>
    public uint Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of bytes that the segment or group consists of.
    /// </summary>
    public uint SectionLength
    {
        get;
        set;
    }

    /// <summary>
    /// Parses a single section map from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <returns>The parsed section map.</returns>
    public static SectionMap FromReader(ref BinaryStreamReader reader)
    {
        return new SectionMap
        {
            Attributes = (SectionMapAttributes) reader.ReadUInt16(),
            LogicalOverlayNumber = reader.ReadUInt16(),
            Group = reader.ReadUInt16(),
            Frame = reader.ReadUInt16(),
            SectionName = reader.ReadUInt16(),
            ClassName = reader.ReadUInt16(),
            Offset = reader.ReadUInt32(),
            SectionLength = reader.ReadUInt32()
        };
    }

    /// <inheritdoc />
    public uint GetPhysicalSize()
    {
        return sizeof(ushort) // Attributes
               + sizeof(ushort) // Ovl
               + sizeof(ushort) // Group
               + sizeof(ushort) // Frame
               + sizeof(ushort) // SectionName
               + sizeof(ushort) // ClassName
               + sizeof(uint) // Offset
               + sizeof(uint) // SectionLength
            ;
    }

    /// <inheritdoc />
    public void Write(IBinaryStreamWriter writer)
    {
        writer.WriteUInt16((ushort) Attributes);
        writer.WriteUInt16(LogicalOverlayNumber);
        writer.WriteUInt16(Group);
        writer.WriteUInt16(Frame);
        writer.WriteUInt16(SectionName);
        writer.WriteUInt16(ClassName);
        writer.WriteUInt32(Offset);
        writer.WriteUInt32(SectionLength);
    }
}
