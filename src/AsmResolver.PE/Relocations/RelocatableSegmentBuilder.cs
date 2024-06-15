using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.Relocations;

/// <summary>
/// Provides a mechanism for joining relocatable segments into one single segment.
/// </summary>
public class RelocatableSegmentBuilder : ISegment, IEnumerable<ISegment>
{
    private readonly SegmentBuilder _contents = new();
    private readonly List<BaseRelocation> _relocations = new();

    /// <inheritdoc />
    public ulong Offset => _contents.Offset;

    /// <inheritdoc />
    public uint Rva => _contents.Rva;

    /// <inheritdoc />
    public bool CanUpdateOffsets => _contents.CanUpdateOffsets;

    /// <summary>
    /// Gets the number of segments that were concatenated.
    /// </summary>
    public int Count => _contents.Count;

    /// <summary>
    /// Adds the provided segment with no alignment.
    /// </summary>
    /// <param name="segment">The segment to add.</param>
    public void Add(ISegment segment) => Add(segment, 1);

    /// <summary>
    /// Adds the provided segment to the offset that is the next multiple of the provided alignment.
    /// </summary>
    /// <param name="segment">The segment to add.</param>
    /// <param name="alignment">The alignment of the segment.</param>
    public void Add(ISegment segment, uint alignment) => _contents.Add(segment, alignment);

    /// <summary>
    /// Adds the provided relocatable segment with no alignment.
    /// </summary>
    /// <param name="segment">The segment to add.</param>
    public void Add(RelocatableSegment segment) => Add(segment, 1);

    /// <summary>
    /// Adds the provided relocatable segment to the offset that is the next multiple of the provided alignment.
    /// </summary>
    /// <param name="segment">The segment to add.</param>
    /// <param name="alignment">The alignment of the segment.</param>
    public void Add(RelocatableSegment segment, uint alignment)
    {
        _relocations.AddRange(segment.Relocations);
        _contents.Add(segment.Segment, alignment);
    }

    /// <summary>
    /// Collects all base relocations that are required when adding the final segment to a PE image.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<BaseRelocation> GetRequiredBaseRelocations() => _relocations;

    /// <inheritdoc />
    public uint GetPhysicalSize() => _contents.GetPhysicalSize();

    /// <inheritdoc />
    public void Write(BinaryStreamWriter writer) => _contents.Write(writer);

    /// <inheritdoc />
    public uint GetVirtualSize() => _contents.GetVirtualSize();

    /// <inheritdoc />
    public void UpdateOffsets(in RelocationParameters parameters) => _contents.UpdateOffsets(in parameters);

    /// <inheritdoc />
    public IEnumerator<ISegment> GetEnumerator() => _contents.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _contents).GetEnumerator();
}
