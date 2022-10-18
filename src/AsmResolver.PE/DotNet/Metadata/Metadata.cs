using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides a basic implementation of a metadata directory in a managed PE.
    /// </summary>
    public class Metadata : SegmentBase, IMetadata
    {
        private IList<IMetadataStream>? _streams;

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
        public bool IsEncMetadata
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

        /// <summary>
        /// Reads a .NET metadata directory from a file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The read metadata.</returns>
        public static Metadata FromFile(string path) => FromBytes(System.IO.File.ReadAllBytes(path));

        /// <summary>
        /// Interprets the provided binary data as a .NET metadata directory.
        /// </summary>
        /// <param name="data">The raw data.</param>
        /// <returns>The read metadata.</returns>
        public static Metadata FromBytes(byte[] data) => FromReader(new BinaryStreamReader(data));

        /// <summary>
        /// Reads a .NET metadata directory from a file.
        /// </summary>
        /// <param name="file">The file to read.</param>
        /// <returns>The read metadata.</returns>
        public static Metadata FromFile(IInputFile file) => FromReader(file.CreateReader());

        /// <summary>
        /// Interprets the provided binary stream as a .NET metadata directory.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The read metadata.</returns>
        public static Metadata FromReader(BinaryStreamReader reader)
        {
            return FromReader(reader, new MetadataReaderContext(VirtualAddressFactory.Instance));
        }

        /// <summary>
        /// Interprets the provided binary stream as a .NET metadata directory.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="context">The context in which the reader is situated in.</param>
        /// <returns>The read metadata.</returns>
        public static Metadata FromReader(BinaryStreamReader reader, MetadataReaderContext context)
        {
            return new SerializedMetadata(context, ref reader);
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
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
        public override void Write(IBinaryStreamWriter writer)
        {
            ulong start = writer.Offset;

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

            ulong end = writer.Offset;
            WriteStreamHeaders(writer, GetStreamHeaders((uint) (end - start)));
            WriteStreams(writer);
        }

        /// <summary>
        /// Constructs new metadata stream headers for all streams in the metadata directory.
        /// </summary>
        /// <param name="offset">The offset of the first stream header.</param>
        /// <returns>A list of stream headers.</returns>
        protected virtual MetadataStreamHeader[] GetStreamHeaders(uint offset)
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
        protected virtual void WriteStreamHeaders(IBinaryStreamWriter writer, MetadataStreamHeader[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];
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
            for (int i = 0; i < Streams.Count; i++)
                Streams[i].Write(writer);
        }

        /// <inheritdoc />
        public virtual IMetadataStream GetStream(string name)
        {
            return TryGetStream(name, out var stream)
                ? stream
                : throw new KeyNotFoundException($"Metadata directory does not contain a stream called {name}.");
        }

        /// <inheritdoc />
        public TStream GetStream<TStream>()
            where TStream : class, IMetadataStream
        {
            return TryGetStream(out TStream? stream)
                ? stream
                : throw new KeyNotFoundException(
                    $"Metadata directory does not contain a stream of type {typeof(TStream).FullName}.");
        }

        /// <inheritdoc />
        public bool TryGetStream(string name, [NotNullWhen(true)] out IMetadataStream? stream)
        {
            bool heapRequested = name is not (TablesStream.CompressedStreamName
                or TablesStream.EncStreamName
                or TablesStream.UncompressedStreamName);

            return TryFindStream((c, s) => c.Name == s as string, name, heapRequested, out stream);
        }

        /// <inheritdoc />
        public bool TryGetStream<TStream>([NotNullWhen(true)] out TStream? stream)
            where TStream : class, IMetadataStream
        {
            bool heapRequested = !typeof(TablesStream).IsAssignableFrom(typeof(TStream));

            if (TryFindStream((c, _) => c is TStream, null, heapRequested, out var candidate))
            {
                stream = (TStream) candidate;
                return true;
            }

            stream = null;
            return false;
        }

        private bool TryFindStream(
            Func<IMetadataStream, object?, bool> condition,
            object? state,
            bool heapRequested,
            [NotNullWhen(true)] out IMetadataStream? stream)
        {
            var streams = Streams;
            bool reverseOrder = heapRequested && !IsEncMetadata;
            if (reverseOrder)
            {
                for (int i = streams.Count - 1; i >= 0; i--)
                {
                    if (condition(streams[i], state))
                    {
                        stream = streams[i];
                        return true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < streams.Count; i++)
                {
                    if (condition(streams[i], state))
                    {
                        stream = streams[i];
                        return true;
                    }
                }
            }

            stream = null;
            return false;
        }

        /// <summary>
        /// Obtains the list of streams defined in the data directory.
        /// </summary>
        /// <returns>The streams.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Streams"/> property.
        /// </remarks>
        protected virtual IList<IMetadataStream> GetStreams() => new List<IMetadataStream>();
    }
}
