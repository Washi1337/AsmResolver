using System.Collections;
using System.Collections.Generic;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Builder
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
                _length += (uint) resource.Data.Length;
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
            foreach (var resource in _resources.Keys)
                context.Writer.WriteBytes(resource.Data);
        }
    }
}