using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.PE.DotNet.Builder
{
    /// <summary>
    /// Provides a mechanism for building up a method body table that can be stored in a PE file.
    /// </summary>
    public class MethodBodyTableBuffer : ISegment
    {
        private readonly SegmentBuilder _tinyBodies = new SegmentBuilder();
        private readonly SegmentBuilder _fatBodies = new SegmentBuilder();
        private readonly SegmentBuilder _nativeBodies = new SegmentBuilder();
        
        private readonly SegmentBuilder _segments = new SegmentBuilder();

        /// <summary>
        /// Creates a new method body table buffer.
        /// </summary>
        public MethodBodyTableBuffer()
        {
            _segments.Add(_tinyBodies);
            _segments.Add(_fatBodies);
            _segments.Add(_nativeBodies);
        }

        /// <inheritdoc />
        public ulong Offset => _segments.Offset;

        /// <inheritdoc />
        public uint Rva => _segments.Rva;

        /// <inheritdoc />
        public bool CanUpdateOffsets => _segments.CanUpdateOffsets;

        /// <summary>
        /// Adds a CIL method body to the buffer.
        /// </summary>
        /// <param name="body">The method body to add.</param>
        public void AddCilBody(CilRawMethodBody body)
        {
            if (body.IsFat)
                _fatBodies.Add(body, 4);
            else
                _tinyBodies.Add(body);
        }

        /// <summary>
        /// Adds a native method body to the buffer.
        /// </summary>
        /// <param name="body">The method body to add.</param>
        /// <param name="alignment">The byte-boundary to align the body to.</param>
        public void AddNativeBody(ISegment body, uint alignment) => _nativeBodies.Add(body, alignment);
        
        /// <inheritdoc />
        public void UpdateOffsets(ulong newOffset, uint newRva) => _segments.UpdateOffsets(newOffset, newRva);

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            uint physicalSize = _segments.GetPhysicalSize();
            return physicalSize;
        }

        /// <inheritdoc />
        public uint GetVirtualSize() => _segments.GetVirtualSize();

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer) => _segments.Write(writer);
    }
}