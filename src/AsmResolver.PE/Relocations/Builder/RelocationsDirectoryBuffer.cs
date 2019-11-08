using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.PE.Relocations
{
    public class RelocationsDirectoryBuffer : SegmentBase
    {
        private readonly IList<BaseRelocation> _relocations = new List<BaseRelocation>();
        private IList<RelocationBlock> _blocks = new List<RelocationBlock>();
        
        public void Add(BaseRelocation relocation)
        {
            _relocations.Add(relocation);
            _blocks = null;
        }

        public override uint GetPhysicalSize()
        {
            EnsureBlocksCreated();
            return (uint) _blocks.Sum(b => b.GetPhysicalSize());
        }

        public override void Write(IBinaryStreamWriter writer)
        {
            EnsureBlocksCreated();
            foreach (var block in CreateBlocks())
                block.Write(writer);
        }

        private void EnsureBlocksCreated()
        {
            if (_blocks == null)
                _blocks = CreateBlocks();
        }

        private IList<RelocationBlock> CreateBlocks()
        {
            var blocks = new Dictionary<uint, RelocationBlock>();
            foreach (var relocation in _relocations)
            {
                uint pageRva = GetPageRva(relocation);
                var block = GetOrCreateBlock(blocks, pageRva);
                block.Entries.Add(CreateEntry(relocation));
            }

            return blocks
                .OrderBy(x => x.Key)
                .Select(x => x.Value)
                .ToArray();
        }

        private static uint GetPageRva(BaseRelocation relocation) => (uint) (relocation.Location.Rva & ~0xFFF);

        private static RelocationEntry CreateEntry(BaseRelocation relocation) =>
            new RelocationEntry(relocation.Type, (int) (relocation.Location.Rva & 0xFFF));

        private static RelocationBlock GetOrCreateBlock(IDictionary<uint, RelocationBlock> blocks, uint pageRva)
        {
            if (!blocks.TryGetValue(pageRva, out var block))
            {
                block = new RelocationBlock(pageRva);
                blocks.Add(pageRva, block);
            }

            return block;
        }
        
    }
}