using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.PE.Relocations.Builder
{
    /// <summary>
    /// Represents one block of relocations to be applied when the PE is loaded into memory.
    /// </summary>
    public class RelocationBlock : SegmentBase
    {
        private IList<RelocationEntry> _entries;

        /// <summary>
        /// Initializes an empty relocation block.
        /// </summary>
        protected RelocationBlock()
        {
        }

        /// <summary>
        /// Creates a new base relocation block for the provided page.
        /// </summary>
        /// <param name="pageRva">The virtual address of the page to apply base relocations on.</param>
        public RelocationBlock(uint pageRva)
        {
            PageRva = pageRva;
        }
        
        /// <inheritdoc />
        public uint PageRva
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IList<RelocationEntry> Entries
        {
            get
            {
                if (_entries is null)
                    Interlocked.CompareExchange(ref _entries, GetEntries(), null);
                return _entries;
            }
        }

        /// <summary>
        /// Obtains the relocations that need to be applied.
        /// </summary>
        /// <returns>The relocations.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Entries"/> property.</remarks>
        protected virtual IList<RelocationEntry> GetEntries()
        {
            return new List<RelocationEntry>();
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => (uint) (Entries.Count + 1) * sizeof(ushort) + 2 * sizeof(uint);

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteUInt32(PageRva);
            writer.WriteUInt32(GetPhysicalSize());
            foreach (var entry in _entries)
                entry.Write(writer);
            new RelocationEntry(0).Write(writer);
        }
        
    }
}