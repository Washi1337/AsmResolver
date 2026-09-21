using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.Relocations.Builder
{
    /// <summary>
    /// Represents one block of relocations to be applied when the PE is loaded into memory.
    /// </summary>
    public sealed class RelocationBlock : SegmentBase
    {
        private readonly HashSet<RelocationEntry> _entrySet = [];
        private readonly List<RelocationEntry> _entries = [];

        /// <summary>
        /// Creates a new base relocation block for the provided page.
        /// </summary>
        /// <param name="pageRva">The virtual address of the page to apply base relocations on.</param>
        public RelocationBlock(uint pageRva)
        {
            PageRva = pageRva;
        }

        /// <summary>
        /// Gets or sets the base RVA for this page.
        /// </summary>
        public uint PageRva
        {
            get;
        }

        /// <summary>
        /// Gets the list of entries added to this page.
        /// </summary>
        public IList<RelocationEntry> Entries => _entries;

        /// <summary>
        /// Adds a relocation entry to the block.
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        /// <param name="allowDuplicates"><c>true</c> if the entry should be ignored if it already exists, <c>false</c> otherwise.</param>
        /// <returns><c>true</c> if the entry was added, <c>false</c> otherwise.</returns>
        public bool Add(RelocationEntry entry, bool allowDuplicates = false)
        {
            if (_entrySet.Add(entry) || allowDuplicates)
            {
                Entries.Add(entry);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            // Align item count to even number (blocks are 32-bit aligned).
            uint totalCount = ((uint) Entries.Count).Align(2);
            return totalCount * sizeof(ushort) + 2 * sizeof(uint);
        }

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            _entries.Sort();

            // Block header.
            writer.WriteUInt32(PageRva);
            writer.WriteUInt32(GetPhysicalSize());

            // Write all entries in block.
            for (int i = 0; i < Entries.Count; i++)
                Entries[i].Write(writer);

            // Ensure block ends on aligned 32-bit address.
            if (Entries.Count % 2 == 1)
                default(RelocationEntry).Write(writer);
        }

    }
}
