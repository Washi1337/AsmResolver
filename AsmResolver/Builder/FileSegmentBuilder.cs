using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Builder
{
    public class FileSegmentBuilder : FileSegment
    {
        public FileSegmentBuilder()
        {
            Segments = new List<FileSegment>();
        }

        public FileSegmentBuilder(params FileSegment[] segments)
        {
            Segments = new List<FileSegment>(segments);
        }

        public IList<FileSegment> Segments
        {
            get;
            protected set;
        }

        public virtual void Build(BuildingContext context)
        {
            foreach (var builder in Segments.OfType<FileSegmentBuilder>())
                builder.Build(context);
        }

        public virtual void UpdateOffsets(BuildingContext context)
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                if (i == 0)
                    Segments[i].StartOffset = StartOffset;
                else
                    Segments[i].StartOffset = Segments[i - 1].StartOffset + Segments[i - 1].GetPhysicalLength();
                
                var builder = Segments[i] as FileSegmentBuilder;
                if (builder != null)
                    builder.UpdateOffsets(context);
            }
        }

        public virtual void UpdateReferences(BuildingContext context)
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
}
