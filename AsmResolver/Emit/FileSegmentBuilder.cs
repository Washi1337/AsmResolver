using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Emit
{
    public abstract class FileSegmentBuilder : FileSegment
    {
        protected FileSegmentBuilder()
        {
            Segments = new List<FileSegment>();
        }

        protected FileSegmentBuilder(params FileSegment[] segments)
        {
            Segments = new List<FileSegment>(segments);
        }

        protected IList<FileSegment> Segments
        {
            get;
            private set;
        }

        public uint GetSegmentRelativeOffset(FileSegment segment)
        {
            uint offset = 0;
            foreach (var existingSegment in Segments)
            {
                if (segment == existingSegment)
                    return offset;
                offset += existingSegment.GetPhysicalLength();
            }
            return offset;
        }
        
        public virtual void UpdateOffsets(EmitContext context)
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                var currentSegment = Segments[i];
                
                if (i == 0)
                    currentSegment.StartOffset = StartOffset;
                else
                    currentSegment.StartOffset = Segments[i - 1].StartOffset + Segments[i - 1].GetPhysicalLength();

                var builder = currentSegment as FileSegmentBuilder;
                if (builder != null)
                    builder.UpdateOffsets(context);
            }
        }

        public virtual void UpdateReferences(EmitContext context)
        {
            foreach (var segment in Segments)
            {
                var builder = segment as FileSegmentBuilder;
                if (builder != null)
                    builder.UpdateReferences(context);
            }
        }

        public override uint GetPhysicalLength()
        {
            return (uint)Segments.Sum(x => x.GetPhysicalLength());
        }

        public override void Write(WritingContext context)
        {
            foreach (var segment in Segments)
            {
                context.Writer.Position = segment.StartOffset;
                segment.Write(context);
            }
        }
    }

    public class SimpleFileSegmentBuilder : FileSegmentBuilder
    {
        public SimpleFileSegmentBuilder()
        {
        }

        public SimpleFileSegmentBuilder(params FileSegment[] segments)
            : base(segments)
        {
        }

        public new IList<FileSegment> Segments
        {
            get { return base.Segments; }
        }
    }
}
