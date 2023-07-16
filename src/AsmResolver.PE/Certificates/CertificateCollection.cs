using System.Collections.ObjectModel;
using AsmResolver.IO;

namespace AsmResolver.PE.Certificates
{
    /// <summary>
    /// Represents the collection of attribute certificates that are stored in a signed portable executable file.
    /// </summary>
    public class CertificateCollection : Collection<AttributeCertificate>, ISegment
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
        public void UpdateOffsets(in RelocationParameters parameters)
        {
            Offset = parameters.Offset;
            Rva = parameters.Rva;

            var current = parameters;
            for (int i = 0; i < Items.Count; i++)
            {
                var certificate = Items[i];
                certificate.UpdateOffsets(current);
                current.Advance(certificate.GetPhysicalSize().Align(8));
            }
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            uint size = 0;
            for (int i = 0; i < Items.Count; i++)
                size += Items[i].GetPhysicalSize().Align(8);
            return size;
        }

        /// <inheritdoc />
        public uint GetVirtualSize() => GetPhysicalSize();

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Write(writer);
                writer.Align(8);
            }
        }
    }
}
