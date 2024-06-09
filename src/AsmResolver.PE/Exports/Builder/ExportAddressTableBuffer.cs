using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.Exports.Builder
{
    /// <summary>
    /// Provides a mechanism for building up a table of exported addresses in the export data directory of
    /// a portable executable (PE) file.
    /// </summary>
    public class ExportAddressTableBuffer : SegmentBase
    {
        private readonly List<ExportedSymbol> _entries = new();
        private readonly NameTableBuffer _nameTableBuffer;

        /// <summary>
        /// Constructs a new export address table buffer.
        /// </summary>
        /// <param name="nameTableBuffer">The name table buffer to use for obtaining the RVAs of forwarder symbols.</param>
        public ExportAddressTableBuffer(NameTableBuffer nameTableBuffer)
        {
            _nameTableBuffer = nameTableBuffer;
        }

        /// <summary>
        /// Adds a single symbol to the address table buffer.
        /// </summary>
        /// <param name="symbol">The symbol to add.</param>
        public void AddSymbol(ExportedSymbol symbol) => _entries.Add(symbol);

        /// <inheritdoc />
        public override uint GetPhysicalSize() => (uint) (_entries.Count * sizeof(uint));

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            foreach (var entry in _entries)
            {
                writer.WriteUInt32(entry.IsForwarder
                    ? _nameTableBuffer.GetNameRva(entry.ForwarderName)
                    : entry.Address.Rva);
            }
        }
    }
}
