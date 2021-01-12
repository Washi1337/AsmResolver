using System;
using System.Collections.Generic;
using System.Text;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides an implementation of a metadata directory that is stored in a PE file.
    /// </summary>
    public class SerializedMetadata : Metadata
    {
        private readonly PEReadContext _context;
        private readonly IBinaryStreamReader _streamEntriesReader;
        private readonly IBinaryStreamReader _streamContentsReader;
        private readonly int _numberOfStreams;

        /// <summary>
        /// Reads a metadata directory from an input stream.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="directoryReader">The input stream containing the metadata directory.</param>
        /// <exception cref="ArgumentNullException">Occurs when any of the arguments are <c>null</c>.</exception>
        /// <exception cref="NotSupportedException">Occurs when an unsupported metadata directory format was encountered.</exception>
        /// <exception cref="BadImageFormatException">Occurs when the metadata directory header is invalid.</exception>
        public SerializedMetadata(PEReadContext context, IBinaryStreamReader directoryReader)
        {
            if (directoryReader == null) 
                throw new ArgumentNullException(nameof(directoryReader));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            _streamContentsReader = directoryReader.Fork();
            
            var signature = (MetadataSignature) directoryReader.ReadUInt32();
            switch (signature)
            {
                case MetadataSignature.Bsjb:
                    // BSJB header is the default header.
                    break;
                case MetadataSignature.Moc:
                    _context.NotSupported("Old +MOC metadata header format is not supported.");
                    return;
                    
                default:
                    _context.BadImage($"Invalid metadata header ({(uint) signature:X8}).");
                    return;
            }

            MajorVersion = directoryReader.ReadUInt16();
            MinorVersion = directoryReader.ReadUInt16();
            Reserved = directoryReader.ReadUInt32();

            int versionLength = directoryReader.ReadInt32();
            if (!directoryReader.CanRead(versionLength))
            {
                _context.BadImage($"Invalid version length in metadata header ({versionLength.ToString()} characters).");
                return;
            }

            var versionBytes = new byte[versionLength];
            directoryReader.ReadBytes(versionBytes, 0, versionBytes.Length);
            VersionString = Encoding.ASCII.GetString(versionBytes);

            Flags = directoryReader.ReadUInt16();
            _numberOfStreams = directoryReader.ReadInt16();
            _streamEntriesReader = directoryReader.Fork();
        }

        /// <inheritdoc />
        protected override IList<IMetadataStream> GetStreams()
        {
            if (_numberOfStreams == 0)
                return base.GetStreams();
            
            return new MetadataStreamList(
                _context, 
                _streamContentsReader.Fork(),
                _streamEntriesReader.Fork(),
                _numberOfStreams);
        }

    }
}