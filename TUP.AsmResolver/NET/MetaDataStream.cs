using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE;
using TUP.AsmResolver.PE.Readers;
namespace TUP.AsmResolver.NET
{
    /// <summary>
    /// Represents a metadata stream of a .NET application.
    /// </summary>
    public class MetaDataStream : IDisposable, ICacheProvider, IImageProvider, ICloneable 
    {
        public MetaDataStream(string name, byte[] contents)
        {
            Name = name;
            this.streamHeader = new Structures.METADATA_STREAM_HEADER()
            {
                Offset = 0,
                Size = (uint)contents.Length,
            };

            mainStream = new MemoryStream();
            binReader = new BinaryReader(mainStream);
            binWriter = new BinaryWriter(mainStream);
            mainStream.Write(contents, 0, contents.Length);
            mainStream.Seek(0, SeekOrigin.Begin);
        }

        internal MetaDataStream(NETHeader netheader, int headeroffset, Structures.METADATA_STREAM_HEADER rawHeader, string name)
        {
            this.headeroffset = headeroffset;
            this.netheader = netheader;
            this.streamHeader = rawHeader;
            this.name = name;
            this.indexsize = 2;

            byte[] contents = netheader.assembly.peImage.ReadBytes(StreamOffset, (int)StreamSize);
            mainStream = new MemoryStream();
            binReader = new BinaryReader(mainStream);
            binWriter = new BinaryWriter(mainStream);
            mainStream.Write(contents, 0, contents.Length);
            mainStream.Seek(0, SeekOrigin.Begin);
        }

        internal int headeroffset;
        internal string name;
        internal NETHeader netheader;
        internal Structures.METADATA_STREAM_HEADER streamHeader;
        internal byte indexsize;
        internal MemoryStream mainStream;
        internal BinaryReader binReader;
        internal BinaryWriter binWriter;

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
                return mainStream.ToArray();
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
                if (HasImage)
                    netheader.assembly.peImage.Write((int)headeroffset + 8, name, Encoding.ASCII);
            }
        }
    
        public byte IndexSize
        {
            get { return indexsize; }
        }

        public NETHeader NETHeader
        {
            get { return netheader; }
        }

        public bool HasImage
        {
            get { return netheader != null; }
        }

        internal virtual void Initialize()
        {
        }

        public virtual void Dispose()
        {
            binReader.Dispose();
            binWriter.Dispose();
            mainStream.Dispose();
            ClearCache();
        }

        public virtual void ClearCache()
        {
        }

        public virtual void LoadCache()
        {
        }

        internal virtual void MakeEmpty()
        {
            // used for rebuilding to remove all unused data.

            Dispose();
            streamHeader.Size = 0;
            mainStream = new MemoryStream();
            binReader = new BinaryReader(mainStream);
            binWriter = new BinaryWriter(mainStream);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }
}
