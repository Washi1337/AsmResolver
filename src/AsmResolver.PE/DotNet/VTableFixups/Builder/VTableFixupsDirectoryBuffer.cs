using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.VTableFixups.Builder
{
    /// <summary>
    /// Provides a mechanism for building the VTable fixups directory in a PE file.
    /// </summary>
    public class VTableFixupsDirectoryBuffer : ISegment
    {
        private readonly SegmentBuilder _vtableDirectory = new();
        private readonly SegmentBuilder _vtableTokens = new();

        /// <inheritdoc />
        public ulong Offset => _vtableDirectory.Offset;

        /// <inheritdoc />
        public uint Rva => _vtableDirectory.Rva;

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <summary>
        /// Creates a new VTable fixups directory buffer.
        /// </summary>
        /// <param name="vtableFixupsDirectory"></param>
        public VTableFixupsDirectoryBuffer(VTableFixupsDirectory vtableFixupsDirectory)
        {
            _vtableDirectory.Add(vtableFixupsDirectory);
            foreach (var vtable in vtableFixupsDirectory)
            {
                _vtableTokens.Add(vtable.Tokens);
            }
        }

        /// <inheritdoc />
        public void UpdateOffsets(ulong newOffset, uint newRva)
        {
            _vtableDirectory.UpdateOffsets(newOffset, newRva);
            uint size = _vtableDirectory.GetPhysicalSize();
            _vtableTokens.UpdateOffsets(newOffset + size, newRva + size);
        }

        /// <inheritdoc />
        public uint GetPhysicalSize() => _vtableDirectory.GetPhysicalSize() + _vtableTokens.GetPhysicalSize();

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            _vtableDirectory.Write(writer);
            _vtableTokens.Write(writer);
        }

        /// <inheritdoc />
        public uint GetVirtualSize() => _vtableDirectory.GetVirtualSize() + _vtableTokens.GetVirtualSize();
    }
}
