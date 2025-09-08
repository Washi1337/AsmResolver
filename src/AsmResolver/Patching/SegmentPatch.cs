using System.Diagnostics;

namespace AsmResolver.Patching;

/// <summary>
/// Patches an instance of <see cref="ISegment"/> with the contents of a file segment.
/// </summary>
[DebuggerDisplay("Patch {RelativeOffset} with {Segment}")]
public class SegmentPatch : IPatch
{
    /// <summary>
    /// Creates a new segment patch.
    /// </summary>
    /// <param name="relativeOffset">The offset to start writing at.</param>
    /// <param name="segment">The new segment.</param>
    public SegmentPatch(uint relativeOffset, ISegment segment)
    {
        RelativeOffset = relativeOffset;
        Segment = segment;
    }

    /// <summary>
    /// Gets the offset relative to the start of the segment to start writing at.
    /// </summary>
    public uint RelativeOffset
    {
        get;
    }

    /// <summary>
    /// Gets the data to write.
    /// </summary>
    public ISegment Segment
    {
        get;
    }

    /// <inheritdoc />
    public void UpdateOffsets(in RelocationParameters parameters)
    {
        if (Segment.CanUpdateOffsets)
            Segment.UpdateOffsets(parameters.WithAdvance(RelativeOffset));
    }

    /// <inheritdoc />
    public void Apply(in PatchContext context)
    {
        context.Writer.Offset = context.WriterBase + RelativeOffset;
        Segment.Write(context.Writer);
    }
}
