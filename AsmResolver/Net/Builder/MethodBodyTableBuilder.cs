using AsmResolver.Builder;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Msil;

namespace AsmResolver.Net.Builder
{
    public sealed class MethodBodyTableBuilder : FileSegmentBuilder
    {
        public override void Build(BuildingContext context)
        {
            foreach (var method in context.Assembly.NetDirectory.
                MetadataHeader.GetStream<TableStream>().GetTable<MethodDefinition>())
            {
                if (method.MethodBody != null)
                    Segments.Add(method.MethodBody);
            }
            base.Build(context);
        }

        public override void UpdateOffsets(BuildingContext context)
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                if (i == 0)
                    Segments[i].StartOffset = StartOffset;
                else
                    Segments[i].StartOffset = Segments[i - 1].StartOffset + Segments[i - 1].GetPhysicalLength();

                var methodBody = Segments[i] as MethodBody;
                if (methodBody != null && methodBody.IsFat)
                    methodBody.StartOffset = Align((uint)methodBody.StartOffset, 4);
            }
        }

        public override void UpdateReferences(BuildingContext context)
        {
            foreach (var method in context.Assembly.NetDirectory.
                MetadataHeader.GetStream<TableStream>().GetTable<MethodDefinition>())
            {
                if (method.MethodBody != null)
                {
                    method.Rva = method.MetadataRow.Column1 =
                        (uint)context.Assembly.FileOffsetToRva(method.MethodBody.StartOffset);
                }
            }
            base.UpdateReferences(context);
        }

        public override uint GetPhysicalLength()
        {
            return (uint)(Segments[Segments.Count - 1].StartOffset + Segments[Segments.Count - 1].GetPhysicalLength());
        }
    }
}