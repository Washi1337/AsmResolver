using System;
using System.Collections.Generic;
using System.Text;
using AsmResolver.IO;
using AsmResolver.Shims;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides an implementation of a metadata directory that is stored in a PE file.
    /// </summary>
    public class SerializedMetadata : Metadata
    {
        private readonly MetadataReaderContext _context;
        private readonly BinaryStreamReader _streamContentsReader;
        private readonly MetadataStreamHeader[] _streamHeaders;
        private readonly bool _hasJtdStream;
        private readonly bool _isEncMetadata;

        /// <summary>
        /// Reads a metadata directory from an input stream.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="directoryReader">The input stream containing the metadata directory.</param>
        /// <exception cref="ArgumentNullException">Occurs when any of the arguments are <c>null</c>.</exception>
        /// <exception cref="NotSupportedException">Occurs when an unsupported metadata directory format was encountered.</exception>
        /// <exception cref="BadImageFormatException">Occurs when the metadata directory header is invalid.</exception>
        public SerializedMetadata(MetadataReaderContext context, ref BinaryStreamReader directoryReader)
        {
            if (!directoryReader.IsValid)
                throw new ArgumentNullException(nameof(directoryReader));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Offset = directoryReader.Offset;
            Rva = directoryReader.Rva;

            _streamContentsReader = directoryReader.Fork();
            _streamHeaders = ArrayShim.Empty<MetadataStreamHeader>();

            // Verify signature.
            var signature = (MetadataSignature) directoryReader.ReadUInt32();
            switch (signature)
            {
                case MetadataSignature.Bsjb:
                    // BSJB header is the default header.
                    break;
                case MetadataSignature.ComPlus:
                    _context.NotSupported("Old COM+ metadata header format is not supported.");
                    return;

                default:
                    _context.BadImage($"Invalid metadata header ({(uint) signature:X8}).");
                    return;
            }

            // Header fields.
            MajorVersion = directoryReader.ReadUInt16();
            MinorVersion = directoryReader.ReadUInt16();
            Reserved = directoryReader.ReadUInt32();

            // Version string.
            uint versionLength = directoryReader.ReadUInt32();
            if (!directoryReader.CanRead(versionLength))
            {
                _context.BadImage($"Invalid version length in metadata header ({versionLength.ToString()} characters).");
                return;
            }

            byte[] versionBytes = new byte[versionLength];
            directoryReader.ReadBytes(versionBytes, 0, versionBytes.Length);
            VersionString = Encoding.ASCII.GetString(versionBytes);

            // Remainder of all header fields.
            Flags = directoryReader.ReadUInt16();
            int numberOfStreams = directoryReader.ReadInt16();

            // Eagerly read stream headers to determine if we are EnC metadata.
            _streamHeaders = new MetadataStreamHeader[numberOfStreams];

            bool? isEncMetadata = null;
            for (int i = 0; i < numberOfStreams; i++)
            {
                _streamHeaders[i] = MetadataStreamHeader.FromReader(ref directoryReader);
                string name = _streamHeaders[i].Name;
                if (isEncMetadata is null)
                {
                    if (name == TablesStream.CompressedStreamName)
                        isEncMetadata = false;
                    else if (name == TablesStream.EncStreamName)
                        isEncMetadata = true;
                }
                if (name == TablesStream.UncompressedStreamName)
                    isEncMetadata = true;
                else if (name == TablesStream.MinimalStreamName)
                    _hasJtdStream = true;
            }

            IsEncMetadata = _isEncMetadata = isEncMetadata ?? false;
        }

        /// <inheritdoc />
        protected override IList<IMetadataStream> GetStreams()
        {
            if (_streamHeaders.Length == 0)
                return base.GetStreams();

            var flags = MetadataStreamReaderFlags.None;
            if (_isEncMetadata)
                flags |= MetadataStreamReaderFlags.IsEnc;
            if (_hasJtdStream)
                flags |= MetadataStreamReaderFlags.HasJtdStream;

            return new MetadataStreamList(this,
                _context,
                flags,
                _streamHeaders,
                _streamContentsReader);
        }

    }
}
