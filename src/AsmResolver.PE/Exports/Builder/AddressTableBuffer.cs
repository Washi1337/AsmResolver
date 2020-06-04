using System.Collections.Generic;

namespace AsmResolver.PE.Exports.Builder
{
    public class AddressTableBuffer : SegmentBase
    {
        private readonly List<ExportedSymbol> _entries = new List<ExportedSymbol>();

        /// <summary>
        /// Adds a single symbol to the address table buffer.
        /// </summary>
        /// <param name="symbol">The symbol to add.</param>
        public void AddSymbol(ExportedSymbol symbol) => _entries.Add(symbol);
        
        /// <inheritdoc />
        public override uint GetPhysicalSize() => (uint) (_entries.Count * sizeof(uint));

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            foreach (var entry in _entries)
                writer.WriteUInt32(entry.Address.Rva);
        }
    }
}