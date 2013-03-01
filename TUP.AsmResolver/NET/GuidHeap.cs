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
        MemoryStream stream;
        BinaryReader binaryreader;

        SortedDictionary<uint, Guid> readGuids = new SortedDictionary<uint, Guid>();

        internal GuidHeap(NETHeader netheader, int headeroffset, Structures.METADATA_STREAM_HEADER rawHeader, string name)
            : base(netheader, headeroffset, rawHeader, name)
        {
            stream = new MemoryStream(this.Contents);
            binaryreader = new BinaryReader(stream);
        }

        public Guid GetGuidByOffset(uint offset)
        {
            Guid guid;
            if (readGuids.TryGetValue(offset, out guid))
                return guid;
            offset--;
            stream.Seek(offset, SeekOrigin.Begin);
            guid = new Guid(binaryreader.ReadBytes(16));
            readGuids.Add(offset, guid);
            return guid;
        }

        public uint GetGuidOffset(Guid guid)
        {
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

        internal override void Reconstruct()
        {
            
        }

        internal void ReadAllGuids()
        {
            newEntryOffset = (uint)stream.Length;
            hasReadAllGuids = true;
        }

        public override void Dispose()
        {
            binaryreader.BaseStream.Close();
            binaryreader.BaseStream.Dispose();
            binaryreader.Close();
            binaryreader.Dispose();
        }

    }
}
