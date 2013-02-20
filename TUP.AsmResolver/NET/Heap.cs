using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE;
namespace TUP.AsmResolver.NET
{
    /// <summary>
    /// Represents a heap from a metadata stream.
    /// </summary>
    public abstract class Heap : MetaDataStream , IDisposable
    {
        internal Heap()
        {
            indexsize = 2;
        }
        internal Heap(MetaDataStream stream)
        {
            name = stream.name;
            netheader = stream.netheader;
            offset = stream.offset;
            size = stream.size;
            streamoffset = stream.streamoffset;
            reader = stream.reader;
            indexsize = 2;
        }
        

        internal byte indexsize;
        public byte IndexSize
        {
            get { return indexsize; }
        }

        public abstract void Dispose();
    }
}
