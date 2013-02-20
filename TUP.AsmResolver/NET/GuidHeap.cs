using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace TUP.AsmResolver.NET
{
    public class GuidHeap : Heap
    {
        internal GuidHeap(MetaDataStream stream)
            : base(stream)
        {
        }

        BinaryReader binaryreader;
        public static GuidHeap FromStream(MetaDataStream stream)
        {
            GuidHeap heap = new GuidHeap(stream);
            heap.binaryreader = new BinaryReader(new MemoryStream(heap.Contents));
            return heap;
        }

        public Guid GetGuidByOffset(uint offset)
        {
            return new Guid(binaryreader.ReadBytes(16));
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
