using System.Collections.Generic;

namespace AsmResolver
{
    public class BaseRelocationBlock : FileSegment
    {
        internal static BaseRelocationBlock FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;
            var block = new BaseRelocationBlock(reader.ReadUInt32())
            {
                _blockSize = reader.ReadUInt32(),
            };

            block._entriesReadingContext = context.CreateSubContext(reader.Position, (int)(block.BlockSize - (2 * sizeof (uint))));
            return block;
        }

        private IList<BaseRelocationEntry> _entries;
        private ReadingContext _entriesReadingContext;
        private uint _blockSize;
        
        public BaseRelocationBlock(uint pageRva)
        {
            PageRva = pageRva;
        }

        public uint PageRva
        {
            get;
            set;
        }

        public uint BlockSize
        {
            get { return _entries == null ? _blockSize : GetPhysicalLength(); }
        }

        public IList<BaseRelocationEntry> Entries
        {
            get
            {
                if (_entries != null)
                    return _entries;
                _entries = new List<BaseRelocationEntry>();

                if (_entriesReadingContext != null)
                {
                    for (int i = 0; i < (_entriesReadingContext.Reader.Length / sizeof(ushort)); i++)
                    {
                        _entries.Add(BaseRelocationEntry.FromReadingContext(_entriesReadingContext));
                    }
                }

                return _entries;
            }
        }

        public override uint GetPhysicalLength()
        {
            return (uint)(2 * sizeof (uint) +
                          Entries.Count * sizeof (ushort));
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteUInt32(PageRva);
            writer.WriteUInt32(BlockSize);
            foreach (var entry in Entries)
                entry.Write(context);
        }
    }
}