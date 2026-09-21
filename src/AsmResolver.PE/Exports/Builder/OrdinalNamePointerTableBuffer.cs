using System;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.Exports.Builder
{
    /// <summary>
    /// Provides a mechanism for building up an ordinal and name-pointer table in an export data directory
    /// of a portable executable file.
    /// </summary>
    public class OrdinalNamePointerTableBuffer : SegmentBase
    {
        private readonly NameTableBuffer _nameTableBuffer;
        private readonly List<ushort> _ordinals = new();
        private readonly List<ExportedSymbol> _namedEntries = new();
        private bool _isSorted = true;

        /// <summary>
        /// Creates a new empty ordinal and name-pointer table buffer.
        /// </summary>
        /// <param name="nameTableBuffer">The table containing the names of all exports.</param>
        public OrdinalNamePointerTableBuffer(NameTableBuffer nameTableBuffer)
        {
            _nameTableBuffer = nameTableBuffer ?? throw new ArgumentNullException(nameof(nameTableBuffer));
        }

        /// <summary>
        /// Gets the relative virtual address (RVA) to the ordinal table.
        /// </summary>
        public uint OrdinalTableRva => Rva;

        /// <summary>
        /// Gets the raw size in bytes of the ordinal table.
        /// </summary>
        public uint OrdinalTableSize => (uint) (_ordinals.Count * sizeof(ushort));

        /// <summary>
        /// Gets the relative virtual address (RVA) to the name pointer table.
        /// </summary>
        public uint NamePointerTableRva => Rva + OrdinalTableSize;

        /// <summary>
        /// Gets the raw size in bytes of the name pointer.
        /// </summary>
        public uint NamePointerTableSize => (uint) (_namedEntries.Count * sizeof(uint));

        /// <summary>
        /// When the symbol is exported by name, adds the ordinal and name pointer pair to the buffer.
        /// </summary>
        /// <param name="symbol">The symbol to register.</param>
        public void AddSymbol(ExportedSymbol symbol)
        {
            if (symbol.ParentDirectory is null)
                throw new ArgumentException("Symbol was not added to an export directory.");

            if (symbol.IsByName)
            {
                _namedEntries.Add(symbol);
                _ordinals.Add((ushort) (symbol.Ordinal - symbol.ParentDirectory.BaseOrdinal));
                _isSorted = false;
            }
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => OrdinalTableSize + NamePointerTableSize;

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            SortNamedEntries();

            WriteOrdinalTable(writer);
            WriteNamePointerTable(writer);
        }

        /// <summary>
        /// Sorts the name pointer table and the parallel ordinal table by the names of the exported symbols.
        /// </summary>
        /// <remarks>
        /// The PE file format requires the name pointer table to be sorted in ascending lexicographical order,
        /// so that the operating system can binary search it when resolving an export by name. Symbols may be
        /// registered in any order (typically the order in which they appear in the export directory), so the
        /// table is sorted lazily right before it is written.
        /// </remarks>
        private void SortNamedEntries()
        {
            if (_isSorted)
                return;

            _isSorted = true;

            int count = _namedEntries.Count;
            if (count <= 1)
                return;

            var indices = new int[count];
            for (int i = 0; i < count; i++)
                indices[i] = i;

            var entries = _namedEntries;
            Array.Sort(indices, (a, b) => string.CompareOrdinal(entries[a].Name, entries[b].Name));

            var sortedEntries = new List<ExportedSymbol>(count);
            var sortedOrdinals = new List<ushort>(count);
            for (int i = 0; i < count; i++)
            {
                sortedEntries.Add(_namedEntries[indices[i]]);
                sortedOrdinals.Add(_ordinals[indices[i]]);
            }

            _namedEntries.Clear();
            _namedEntries.AddRange(sortedEntries);
            _ordinals.Clear();
            _ordinals.AddRange(sortedOrdinals);
        }

        private void WriteNamePointerTable(BinaryStreamWriter writer)
        {
            foreach (var entry in _namedEntries)
                writer.WriteUInt32(_nameTableBuffer.GetNameRva(entry.Name));
        }

        private void WriteOrdinalTable(BinaryStreamWriter writer)
        {
            foreach (ushort ordinal in _ordinals)
                writer.WriteUInt16(ordinal);
        }
    }
}
