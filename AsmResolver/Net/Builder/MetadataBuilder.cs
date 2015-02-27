using System.Collections.Generic;
using System.Linq;
using AsmResolver.Builder;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Builder
{
    public class MetadataBuilder : FileSegmentBuilder
    {
        private sealed class StreamTableBuilder : FileSegmentBuilder
        {
            private readonly Dictionary<MetadataStream, FileSegment> _streams = new Dictionary<MetadataStream, FileSegment>();

            public IEnumerable<MetadataStream> GetStreams()
            {
                return _streams.Keys;
            } 
 
            public void AddStream(MetadataStream stream)
            {
                var buffer = stream.CanCreateBuffer
                    ? ((IBufferProvider)stream).CreateBuffer()
                    : stream;
                _streams.Add(stream, buffer);
                Segments.Add(buffer);
            }

            public FileSegment GetBuffer(MetadataStream stream)
            {
                return _streams[stream];
            }

            public TBuffer GetBuffer<TBuffer>()
                where TBuffer : FileSegment
            {
                return (TBuffer)_streams.Values.Single(x => x is TBuffer);
            }
        }

        private readonly MetadataHeader _header;
        private readonly StreamTableBuilder _streamBuilder;

        public MetadataBuilder(MetadataHeader header)
        {
            _header = header;

            Segments.Add(header);
            Segments.Add(_streamBuilder = new StreamTableBuilder());
        }

        public override void Build(BuildingContext context)
        {
            var buildingContext = (NetBuildingContext)context;

            foreach (var streamHeader in _header.StreamHeaders)
                _streamBuilder.AddStream(streamHeader.Stream);

            AppendResourceData(buildingContext.Builder.TextBuilder);
            UpdateTableStreamHeader();
            UpdateMetaDataRows(buildingContext);

            base.Build(context);
        }

        public FileSegment GetStreamBuffer(MetadataStream stream)
        {
            return _streamBuilder.GetBuffer(stream);
        }

        public TBuffer GetStreamBuffer<TBuffer>()
            where TBuffer : FileSegment
        {
            return _streamBuilder.GetBuffer<TBuffer>();
        }

        private void AppendResourceData(NetTextBuilder textBuilder)
        {
            foreach (var resource in _header.GetStream<TableStream>().GetTable<ManifestResource>())
            {
                if (resource.IsEmbedded)
                {
                    resource.Offset = textBuilder.AppendResourceData(resource.Data);
                }
            }
        }

        public override void UpdateReferences(BuildingContext context)
        {
            base.UpdateReferences(context);

            UpdateMethodBodyAddresses();
            UpdateStreamHeaders();
            UpdateTableStreamHeader();
        }

        private void UpdateTableStreamHeader()
        {
            var tableStream = _header.GetStream<TableStream>();
            tableStream.ValidBitVector = tableStream.ComputeValidBitVector();
        }

        private void UpdateMethodBodyAddresses()
        {
            foreach (var method in _header.GetStream<TableStream>().GetTable<MethodDefinition>())
            {
                if (method.MethodBody != null)
                {
                    method.Rva =
                        method.MetadataRow.Column1 =
                            (uint)_header.NetDirectory.Assembly.FileOffsetToRva(method.MethodBody.StartOffset);
                }
            }
        }

        private void UpdateStreamHeaders()
        {
            foreach (var stream in _streamBuilder.GetStreams())
            {
                var buffer = _streamBuilder.GetBuffer(stream);
                stream.StreamHeader.Offset = (uint)(buffer.StartOffset - _header.StartOffset);
                stream.StreamHeader.Size = buffer.GetPhysicalLength();
            }
        }

        private void UpdateMetaDataRows(NetBuildingContext context)
        {
            var tables = _header.GetStream<TableStream>().GetPresentTables().ToArray();
            foreach (var table in tables)
                table.UpdateTokens();
            foreach (var table in tables)
                table.UpdateRows(context);
        }
    }
}
