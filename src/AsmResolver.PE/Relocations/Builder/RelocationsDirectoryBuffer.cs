using System.Collections.Generic;
using System.Linq;
using AsmResolver.IO;

namespace AsmResolver.PE.Relocations.Builder
{
    /// <summary>
    /// Provides a mechanism for building a base relocations directory.
    /// </summary>
    public class RelocationsDirectoryBuffer : SegmentBase
    {
        private readonly List<BaseRelocation> _relocations = new();
        private List<RelocationBlock>? _blocks = new();

        /// <summary>
        /// Gets a value indicating whether there is any data added to the buffer.
        /// </summary>
        public bool IsEmpty => _relocations.Count == 0;

        /// <summary>
        /// Adds a single base relocation to the buffer.
        /// </summary>
        /// <param name="relocation">The base relocation to add.</param>
        public void Add(BaseRelocation relocation)
        {
            _relocations.Add(relocation);
            _blocks = null;
        }

        private void EnsureBlocksCreated() => _blocks ??= CreateBlocks();

        private List<RelocationBlock> CreateBlocks()
        {
            var blocks = new Dictionary<uint, RelocationBlock>();
            for (int i = 0; i < _relocations.Count; i++)
            {
                var relocation = _relocations[i];
                uint pageRva = GetPageRva(relocation);
                var block = GetOrCreateBlock(blocks, pageRva);
                block.Entries.Add(CreateEntry(relocation));
            }

            return blocks
                .OrderBy(x => x.Key)
                .Select(x => x.Value)
                .ToList();
        }

        private static uint GetPageRva(BaseRelocation relocation) => (uint) (relocation.Location.Rva & ~0xFFF);

        private static RelocationEntry CreateEntry(BaseRelocation relocation) =>
            new(relocation.Type, (int) (relocation.Location.Rva & 0xFFF));

        private static RelocationBlock GetOrCreateBlock(IDictionary<uint, RelocationBlock> blocks, uint pageRva)
        {
            if (!blocks.TryGetValue(pageRva, out var block))
            {
                block = new RelocationBlock(pageRva);
                blocks.Add(pageRva, block);
            }

            return block;
        }

        /// <inheritdoc />
        public override void UpdateOffsets(in RelocationParameters parameters)
        {
            base.UpdateOffsets(parameters);
            _blocks = null;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            EnsureBlocksCreated();
            return (uint) _blocks!.Sum(b => b.GetPhysicalSize());
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            EnsureBlocksCreated();
            for (int i = 0; i < _blocks!.Count; i++)
                _blocks![i].Write(writer);
        }

    }
}
