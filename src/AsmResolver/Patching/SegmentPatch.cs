namespace AsmResolver.Patching;

public class SegmentPatch : IPatch
{
    public SegmentPatch(uint offset, ISegment segment)
    {
        Offset = offset;
        Segment = segment;
    }

    public uint Offset
    {
        get;
    }

    public ISegment Segment
    {
        get;
    }

    /// <inheritdoc />
    public void UpdateOffsets(in RelocationParameters parameters)
    {
        if (Segment.CanUpdateOffsets)
            Segment.UpdateOffsets(parameters.WithAdvance(Offset));
    }

    /// <inheritdoc />
    public void Apply(in PatchContext context)
    {
        context.Writer.Offset = Segment.Offset + Offset;
        Segment.Write(context.Writer);
    }
}
