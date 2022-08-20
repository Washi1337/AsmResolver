using System.Collections.ObjectModel;
using AsmResolver.IO;

namespace AsmResolver.PE.Tls
{
    /// <summary>
    /// Represents a collection of Thread-Local Storage (TLS) callback function addresses.
    /// </summary>
    public class TlsCallbackCollection : Collection<ISegmentReference>, ISegment
    {
        private readonly ITlsDirectory _owner;
        private ulong _imageBase = 0x00400000;
        private bool _is32Bit = true;

        internal TlsCallbackCollection(ITlsDirectory owner)
        {
            _owner = owner;
        }

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
        public void UpdateOffsets(in RelocationParameters parameters)
        {
            Offset = parameters.Offset;
            Rva = parameters.Rva;
            _imageBase = parameters.ImageBase;
            _is32Bit = parameters.Is32Bit;
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            uint pointerSize = (uint) (_is32Bit ? sizeof(uint) : sizeof(ulong));
            return (uint) (pointerSize * (Count + 1));
        }

        /// <inheritdoc />
        public uint GetVirtualSize() => GetPhysicalSize();

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            ulong imageBase = _imageBase;
            bool is32Bit = _is32Bit;

            for (int i = 0; i < Items.Count; i++)
                writer.WriteNativeInt(imageBase + Items[i].Rva, is32Bit);

            writer.WriteNativeInt(0, is32Bit);
        }
    }
}
