using System;
using System.Collections.Generic;
using AsmResolver.Builder;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Builder
{
    public sealed class NetResourceDirectoryBuilder : FileSegmentBuilder
    {
        private readonly Dictionary<ManifestResource, FileSegmentBuilder> _resourceSegments =
            new Dictionary<ManifestResource, FileSegmentBuilder>();

        public override void Build(BuildingContext context)
        {
            foreach (var resource in context.Assembly.NetDirectory.
                MetadataHeader.GetStream<TableStream>().GetTable<ManifestResource>())
            {
                if (resource.IsEmbedded)
                    resource.Offset = GetSegmentRelativeOffset(GetResourceSegment(resource));
            }
            base.Build(context);
        }

        public FileSegmentBuilder GetResourceSegment(ManifestResource resource)
        {
            FileSegmentBuilder segment;
            if (!_resourceSegments.TryGetValue(resource, out segment))
            {
                segment = new FileSegmentBuilder();
                segment.Segments.Add(new DataSegment(BitConverter.GetBytes(resource.Data.Length)));
                segment.Segments.Add(new DataSegment(resource.Data));
                _resourceSegments.Add(resource, segment);
                Segments.Add(segment);
            }
            return segment;
        }
    }
}