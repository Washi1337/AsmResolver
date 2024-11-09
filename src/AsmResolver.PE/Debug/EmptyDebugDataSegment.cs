using System.Diagnostics;
using AsmResolver.IO;

namespace AsmResolver.PE.Debug;

/// <summary>
/// Represents the contents of an empty debug data entry.
/// </summary>
public sealed class EmptyDebugDataSegment : IDebugDataSegment
{
    /// <summary>
    /// Creates a new empty debug data segment for the provided debug data content.
    /// </summary>
    /// <param name="type">The content type.</param>
    public EmptyDebugDataSegment(DebugDataType type)
    {
        Type = type;
    }

    /// <inheritdoc />
    public DebugDataType Type { get; }

    bool ISegment.CanUpdateOffsets => false;

    ulong IOffsetProvider.Offset => 0;

    uint IOffsetProvider.Rva => 0;

    /// <inheritdoc />
    void ISegment.UpdateOffsets(in RelocationParameters parameters)
    {
    }

    /// <inheritdoc />
    uint IWritable.GetPhysicalSize() => 0;

    /// <inheritdoc />
    uint ISegment.GetVirtualSize() => 0;

    void IWritable.Write(BinaryStreamWriter writer)
    {
    }
}
