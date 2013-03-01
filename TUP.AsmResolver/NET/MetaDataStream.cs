using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE;
using TUP.AsmResolver.PE.Readers;
namespace TUP.AsmResolver.NET
{
    /// <summary>
    /// Represents a metadata stream of a .NET application.
    /// </summary>
    public abstract class MetaDataStream : IDisposable
    {

        internal MetaDataStream(NETHeader netheader, int headeroffset, Structures.METADATA_STREAM_HEADER rawHeader, string name)
        {
            this.headeroffset = headeroffset;
            this.netheader = netheader;
            this.streamHeader = rawHeader;
            this.name = name;
            this.indexsize = 2;
        }

        internal int headeroffset;
        internal string name;
        internal NETHeader netheader;
        internal Structures.METADATA_STREAM_HEADER streamHeader;
        internal byte[] contents; 

        /// <summary>
        /// Gets the offset of the header of the metadata.
        /// </summary>
        public int HeaderOffset
        {
            get { return headeroffset; }
        }
        /// <summary>
        /// Gets the actual file offset of the stream.
        /// </summary>
        public uint StreamOffset
        {
            get { return streamHeader.Offset + (uint)netheader.MetaDataHeader.RawOffset; }
        }
        /// <summary>
        /// Gets the length of bytes of the stream
        /// </summary>
        public uint StreamSize
        {
            get { return streamHeader.Size ; }
        }
        /// <summary>
        /// Gets the contents in byte array format.
        /// </summary>
        public byte[] Contents
        {
            get
            {
                if (contents == null)
                    contents = netheader.assembly.peImage.ReadBytes(StreamOffset, (int)StreamSize);
                return contents;
            }
        }
        /// <summary>
        /// Gets the name of the metadata stream.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                netheader.assembly.peImage.Write((int)headeroffset + 8, name, Encoding.ASCII);
            }
        }
        //public MetaDataStream  ToHeap()
        //{
        //    switch (name)
        //    {
        //        case "#~":
        //        case "#-":
        //            return netheader.TablesHeap;
        //        case "#Strings":
        //            return netheader.StringsHeap;
        //        case "#US":
        //            return netheader.UserStringsHeap;
        //        case "#GUID":
        //            return netheader.GuidHeap;
        //        case "#Blob":
        //            return netheader.BlobHeap ;
        //        default:
        //            throw new ArgumentException("Metadatastream is not recognized as a valid heap.");
        //    }
        //
        //}       

        internal byte indexsize;
        public byte IndexSize
        {
            get { return indexsize; }
        }

        internal abstract void Reconstruct();

        internal abstract void Initialize();

        public abstract void Dispose();

    }
}
