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
    public class MetaDataStream 
    {
        internal MetaDataStream()
        { }

        internal MetaDataStream(NETHeader netheader, NETHeaderReader reader, int headeroffset, int offset, int size,  string name)
        {
            this.headeroffset = headeroffset;
            this.netheader = netheader;
            this.offset = offset;
            this.size = size;
            this.reader = reader;
            this.name = name;
        }
        internal int offset, size,streamoffset , headeroffset;
        internal string name;
        internal NETHeaderReader reader;
        internal NETHeader netheader;
        
        /// <summary>
        /// Gets the offset of the header of the metadata.
        /// </summary>
        public int HeaderOffset
        {
            get { return offset; }
        }
        /// <summary>
        /// Gets the actual file offset of the stream.
        /// </summary>
        public int StreamOffset
        {
            get { return streamoffset; }
        }
        /// <summary>
        /// Gets the length of bytes of the stream
        /// </summary>
        public int StreamSize
        {
            get { return size; }
        }
        /// <summary>
        /// Gets the contents in byte array format.
        /// </summary>
        public byte[] Contents
        {
            get { return reader.header.assembly.peImage.ReadBytes(streamoffset, size); }
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
                reader.header.assembly.peImage.Write((int)headeroffset + 8, name, Encoding.ASCII);
            }
        }
        public Heap ToHeap()
        {
            switch (name)
            {
                case "#~":
                case "#-":
                    return netheader.TablesHeap;
                case "#Strings":
                    return netheader.StringsHeap;
                case "#US":
                    return netheader.UserStringsHeap;
                case "#GUID":
                    return netheader.GuidHeap;
                case "#Blob":
                    return netheader.BlobHeap ;
                default:
                    throw new ArgumentException("Metadatastream is not recognized as a valid heap.");
            }

        }
    }
}
