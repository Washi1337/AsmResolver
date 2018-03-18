using AsmResolver.Emit;
using AsmResolver.Net.Cil;

namespace AsmResolver.Net.Emit
{
    public class MethodBodyTableBuffer : SimpleFileSegmentBuilder
    {
        public override uint GetPhysicalLength()
        {
            return (uint) (Segments[Segments.Count - 1].StartOffset - StartOffset + Segments[Segments.Count - 1].GetPhysicalLength());
        }

        public override void UpdateOffsets(EmitContext context)
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                var body = Segments[i];
                
                if (i == 0)
                    body.StartOffset = StartOffset;
                else
                    body.StartOffset = Segments[i - 1].StartOffset + Segments[i - 1].GetPhysicalLength();

                if (body is CilRawFatMethodBody)
                    body.StartOffset = Align((uint) body.StartOffset, 4);
            }
        }
    }
}