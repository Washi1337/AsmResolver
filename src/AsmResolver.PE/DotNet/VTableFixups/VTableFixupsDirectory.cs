using System.Collections.ObjectModel;
using System.Linq;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.VTableFixups
{
    /// <summary>
    /// Represents the VTable fixups directory in the Cor20 header.
    /// </summary>
    public class VTableFixupsDirectory : Collection<VTableFixup>, ISegment
    {
        /// <inheritdoc />
        public ulong Offset
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public uint Rva
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <inheritdoc />
        public void UpdateOffsets(ulong newOffset, uint newRva)
        {
            Offset = newOffset;
            Rva = newRva;
        }

        /// <inheritdoc />
        public uint GetPhysicalSize() => (uint) this.Sum(v => v.GetPhysicalSize());

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            foreach (var vtable in this)
            {
                vtable.Write(writer);
            }
        }

        /// <inheritdoc />
        public uint GetVirtualSize() => (uint) this.Sum(v => v.GetVirtualSize());
    }
}
