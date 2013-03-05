using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TUP.AsmResolver.PE;
namespace TUP.AsmResolver.NET
{
    public class GuidHeap : MetaDataStream
    {
        uint newEntryOffset = 0;
        bool hasReadAllGuids = false;

        SortedDictionary<uint, Guid> readGuids = new SortedDictionary<uint, Guid>();

        internal GuidHeap(NETHeader netheader, int headeroffset, Structures.METADATA_STREAM_HEADER rawHeader, string name)
            : base(netheader, headeroffset, rawHeader, name)
        {
        }

        public Guid GetGuidByOffset(uint offset)
        {
            Guid guid;
            if (readGuids.TryGetValue(offset, out guid))
                return guid;
            mainStream.Seek(offset - 1, SeekOrigin.Begin);
            guid = new Guid(binReader.ReadBytes(16));
            readGuids.Add(offset, guid);
            return guid;
        }

        public uint GetGuidOffset(Guid guid)
        {
            if (guid == null || guid == Guid.Empty)
                return 0;

            if (!hasReadAllGuids)
                ReadAllGuids();

            if (readGuids.ContainsValue(guid))
                return readGuids.First(rg => rg.Value == guid).Key;

            uint offset = newEntryOffset;
            readGuids.Add(offset, guid);
            newEntryOffset += 16;
            return offset;
        }

        internal override void Initialize()
        {
        }

        internal void ReadAllGuids()
        {
            mainStream.Seek(1, SeekOrigin.Begin);
            while (mainStream.Position < mainStream.Length)
            {
                bool alreadyExists = readGuids.ContainsKey((uint)mainStream.Position);
                GetGuidByOffset((uint)mainStream.Position);
                if (alreadyExists)
                    mainStream.Seek(16, SeekOrigin.Current);
            }

            newEntryOffset = (uint)mainStream.Length;
            hasReadAllGuids = true;
        }

        public override void Dispose()
        {
            ClearCache();
            base.Dispose();
        }

        public override void ClearCache()
        {
            readGuids.Clear();
        }
    }
}
