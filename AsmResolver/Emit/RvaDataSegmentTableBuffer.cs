using System;

namespace AsmResolver.Emit
{
    public class RvaDataSegmentTableBuffer : FileSegmentBuilder
    {
        private readonly IOffsetConverter _converter;

        public RvaDataSegmentTableBuffer(IOffsetConverter converter)
        {
            if (converter == null)
                throw new ArgumentNullException("converter");
            _converter = converter;
        }
        

        public void AddSegment(RvaDataSegment segment)
        {
            Segments.Add(segment);
        }

        public override void UpdateOffsets(EmitContext context)
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                if (i == 0)
                    Segments[i].StartOffset = StartOffset;
                else
                    Segments[i].StartOffset = Segments[i - 1].StartOffset + Segments[i - 1].GetPhysicalLength();

                var dataSegment = Segments[i] as RvaDataSegment;
                if (dataSegment != null)
                {
                    if (dataSegment.ShouldBeAligned)
                        dataSegment.StartOffset = Align((uint) dataSegment.StartOffset, 4);

                    dataSegment.Rva = (uint) _converter.FileOffsetToRva(dataSegment.StartOffset);
                }
            }
        }
    }
}