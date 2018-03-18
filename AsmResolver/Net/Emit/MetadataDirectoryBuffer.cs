using System;
using System.Linq;
using AsmResolver.Emit;

namespace AsmResolver.Net.Emit
{
    public class MetadataDirectoryBuffer : FileSegmentBuilder
    {
        private readonly SimpleFileSegmentBuilder _streamTable = new SimpleFileSegmentBuilder();

        public MetadataDirectoryBuffer(MetadataHeader header)
        {
            if (header == null)
                throw new ArgumentNullException("header");
            
            Segments.Add(Header = header);
            Segments.Add(_streamTable);

            foreach (var stream in header.StreamHeaders)
                AddMetadataStream(stream.Stream);
        }

        public MetadataHeader Header
        {
            get;
            private set;
        }

        public void AddMetadataStream(MetadataStream stream)
        {
            _streamTable.Segments.Add(stream);
        }

        public override void UpdateReferences(EmitContext context)
        {
            base.UpdateReferences(context);
            UpdateStreamHeaders();
        }
        
        private void UpdateStreamHeaders()
        {
            uint currentOffset = Header.GetPhysicalLength();
            foreach (var header in Header.StreamHeaders)
            {
                header.Offset = currentOffset;
                header.Size = header.Stream.GetPhysicalLength();
                currentOffset += header.Size;
            }
        }
    }
}