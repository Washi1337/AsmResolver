using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides a basic implementation of a metadata directory in a managed PE.
    /// </summary>
    public class Metadata : IMetadata
    {
        private IList<IMetadataStream> _streams;

        /// <inheritdoc />
        public uint FileOffset
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public uint Rva
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <inheritdoc />
        public ushort MajorVersion
        {
            get;
            set;
        } = 1;

        /// <inheritdoc />
        public ushort MinorVersion
        {
            get;
            set;
        } = 1;

        /// <inheritdoc />
        public uint Reserved
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string VersionString
        {
            get;
            set;
        } = "v4.0.30319";

        /// <inheritdoc />
        public ushort Flags
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IList<IMetadataStream> Streams
        {
            get
            {
                if (_streams is null)
                    Interlocked.CompareExchange(ref _streams, GetStreams(), null);
                return _streams;
            }
        }

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            FileOffset = newFileOffset;
            Rva = newRva;
            
            // TODO: update stream offsets.
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            return (uint) (sizeof(uint)                              // Signature
                           + 2 * sizeof(ushort)                      // Version
                           + sizeof(uint)                            // Reserved
                           + sizeof(uint)                            // Version length
                           + ((uint) VersionString.Length).Align(4)  // Version
                           + sizeof(ushort)                          // Flags
                           + sizeof(ushort)                          // Stream count
                           + GetSizeOfStreamHeaders()                // Stream headers
                           + Streams.Sum(s => s.GetPhysicalSize())); // Streams 
        }

        /// <inheritdoc />
        public uint GetVirtualSize() => GetPhysicalSize();

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            uint start = writer.FileOffset;
            
            writer.WriteUInt32((uint) MetadataSignature.Bsjb);
            writer.WriteUInt16(MajorVersion);
            writer.WriteUInt16(MinorVersion);
            writer.WriteUInt32(Reserved);
            
            var versionBytes = new byte[((uint) VersionString.Length).Align(4)];
            Encoding.UTF8.GetBytes(VersionString, 0, VersionString.Length, versionBytes, 0);
            writer.WriteInt32(versionBytes.Length);
            writer.WriteBytes(versionBytes);
            
            writer.WriteUInt16(Flags);
            writer.WriteUInt16((ushort) Streams.Count);

            uint end = writer.FileOffset;
            WriteStreamHeaders(writer, GetStreamHeaders(end - start));
            WriteStreams(writer);
        }

        /// <summary>
        /// Constructs new metadata stream headers for all streams in the metadata directory.
        /// </summary>
        /// <param name="offset">The offset of the first stream header.</param>
        /// <returns>A list of stream headers.</returns>
        protected virtual IList<MetadataStreamHeader> GetStreamHeaders(uint offset)
        {
            uint sizeOfHeaders = GetSizeOfStreamHeaders();
            offset += sizeOfHeaders;
            
            var result = new MetadataStreamHeader[Streams.Count];
            for (int i = 0; i < result.Length; i++)
            {
                uint size = Streams[i].GetPhysicalSize();
                result[i] = new MetadataStreamHeader(offset, size, Streams[i].Name);
                offset += size;
            }

            return result;
        }

        private uint GetSizeOfStreamHeaders()
        {
            uint sizeOfHeaders = (uint) (Streams.Count * 2 * sizeof(uint)
                                         + Streams.Sum(s => ((uint) s.Name.Length + 1).Align(4)));
            return sizeOfHeaders;
        }

        /// <summary>
        /// Writes a collection of stream headers to an output stream.
        /// </summary>
        /// <param name="writer">The output stream to write to.</param>
        /// <param name="headers">The headers to write.</param>
        protected virtual void WriteStreamHeaders(IBinaryStreamWriter writer, IEnumerable<MetadataStreamHeader> headers)
        {
            foreach (var header in headers)
            {
                writer.WriteUInt32(header.Offset);
                writer.WriteUInt32(header.Size);
                writer.WriteAsciiString(header.Name);
                writer.WriteByte(0);
                writer.Align(4);
            }
        }

        /// <summary>
        /// Writes the contents of all streams to an output stream.
        /// </summary>
        /// <param name="writer">The output stream to write to.</param>
        protected virtual void WriteStreams(IBinaryStreamWriter writer)
        {
            foreach (var stream in Streams)
                stream.Write(writer);
        }
        
        /// <inheritdoc />
        public virtual IMetadataStream GetStream(string name)
        {
            var streams = Streams;
            
            // Reversed order search. CLR counts the last stream.  
            for (int i = streams.Count - 1; i >= 0; i--)
            {
                if (streams[i].Name == name)
                    return streams[i];
            }

            return null;
        }

        /// <inheritdoc />
        public TStream GetStream<TStream>()
            where TStream : IMetadataStream
        {
            var streams = Streams;
            
            // Reversed order search. CLR counts the last stream.  
            for (int i = streams.Count - 1; i >= 0; i--)
            {
                if (streams[i] is TStream stream)
                    return stream;
            }

            return default;
        }

        /// <summary>
        /// Obtains the list of streams defined in the data directory.
        /// </summary>
        /// <returns>The streams.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Streams"/> property.
        /// </remarks>
        protected virtual IList<IMetadataStream> GetStreams()
        {
            return new List<IMetadataStream>();
        }
    }
}