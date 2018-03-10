using System.Collections.Generic;
using System.IO;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Emit
{
    public class ResourcesBuffer : FileSegment
    {
        private readonly IDictionary<ManifestResource, uint> _resources = new Dictionary<ManifestResource, uint>();
        private uint _length = 0;
        
        public uint GetResourceOffset(ManifestResource resource)
        {
            uint offset;
            if (!_resources.TryGetValue(resource, out offset))
            {
                _resources.Add(resource, _length);
                _length += (uint) resource.Data.Length + sizeof(uint);
                return offset;
            }
            return offset;
        }


        public override uint GetPhysicalLength()
        {
            return _length;
        }

        public override void Write(WritingContext context)
        {
            Write(context.Writer);
        }

        private void Write(IBinaryStreamWriter writer)
        {
            foreach (var resource in _resources.Keys)
            {
                writer.WriteUInt32((uint) resource.Data.Length);
                writer.WriteBytes(resource.Data);
            }
        }

        public ResourcesManifest CreateDirectory()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryStreamWriter(stream);
                Write(writer);
                return new ResourcesManifest(new MemoryStreamReader(stream.ToArray()));
            }
        }
    }
}