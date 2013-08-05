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
            this._streamHeader = new Structures.METADATA_STREAM_HEADER()
            {
                Offset = 0,
                Size = (uint)contents.Length,
            };

            _mainStream = new MemoryStream();
            _binReader = new BinaryReader(_mainStream);
            _binWriter = new BinaryWriter(_mainStream);
            _mainStream.Write(contents, 0, contents.Length);
            _mainStream.Seek(0, SeekOrigin.Begin);
        }

        internal MetaDataStream(NETHeader netheader, int headeroffset, Structures.METADATA_STREAM_HEADER rawHeader, string name)
        {
            this._headeroffset = headeroffset;
            this._netheader = netheader;
            this._streamHeader = rawHeader;
            this._name = name;
            this._indexsize = 2;

            byte[] contents = netheader._assembly._peImage.ReadBytes(StreamOffset, (int)StreamSize);
            _mainStream = new MemoryStream();
            _binReader = new BinaryReader(_mainStream);
            _binWriter = new BinaryWriter(_mainStream);
            _mainStream.Write(contents, 0, contents.Length);
            _mainStream.Seek(0, SeekOrigin.Begin);
        }

        internal int _headeroffset;
        internal string _name;
        internal NETHeader _netheader;
        internal Structures.METADATA_STREAM_HEADER _streamHeader;
        internal byte _indexsize;
        internal MemoryStream _mainStream;
        internal BinaryReader _binReader;
        internal BinaryWriter _binWriter;

        /// <summary>
        /// Gets the offset of the header of the metadata.
        /// </summary>
        public int HeaderOffset
        {
            get { return _headeroffset; }
        }
        /// <summary>
        /// Gets the actual file offset of the stream.
        /// </summary>
        public uint StreamOffset
        {
            get { return _streamHeader.Offset + (uint)_netheader.MetaDataHeader.RawOffset; }
        }
        /// <summary>
        /// Gets the length of bytes of the stream
        /// </summary>
        public uint StreamSize
        {
            get { return _streamHeader.Size ; }
        }
        /// <summary>
        /// Gets the contents in byte array format.
        /// </summary>
        public virtual byte[] Contents
        {
            get
            {
                return _mainStream.ToArray();
            }
        }
        /// <summary>
        /// Gets the name of the metadata stream.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                if (HasImage)
                    _netheader._assembly._peImage.Write((int)_headeroffset + 8, _name, Encoding.ASCII);
            }
        }
    
        public byte IndexSize
        {
            get { return _indexsize; }
        }

        public NETHeader NETHeader
        {
            get { return _netheader; }
        }

        public bool HasImage
        {
            get { return _netheader != null; }
        }

        internal virtual void Initialize()
        {
        }

        public virtual void Dispose()
        {
            _binReader.Dispose();
            _binWriter.Dispose();
            _mainStream.Dispose();
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
            _streamHeader.Size = 0;
            _mainStream = new MemoryStream();
            _binReader = new BinaryReader(_mainStream);
            _binWriter = new BinaryWriter(_mainStream);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }
}
