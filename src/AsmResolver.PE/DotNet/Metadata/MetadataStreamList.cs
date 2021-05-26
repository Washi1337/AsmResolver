using System;
using AsmResolver.Collections;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides an implementation of a lazy-initialized list of metadata streams present in a metadata directory.
    /// </summary>
    public class MetadataStreamList : LazyList<IMetadataStream>
    {
        private readonly PEReaderContext _context;
        private readonly int _numberOfStreams;
        private BinaryStreamReader _directoryReader;
        private BinaryStreamReader _entriesReader;

        /// <summary>
        /// Prepares a new lazy-initialized metadata stream list.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="directoryReader">The input stream containing the metadata directory.</param>
        /// <param name="entriesReader">The input stream containing the metadata stream entries.</param>
        /// <param name="numberOfStreams">The number of streams.</param>
        public MetadataStreamList(
            PEReaderContext context,
            in BinaryStreamReader directoryReader,
            in BinaryStreamReader entriesReader,
            int numberOfStreams)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _directoryReader = directoryReader;
            _entriesReader = entriesReader;
            _numberOfStreams = numberOfStreams;
        }

        /// <inheritdoc />
        public override int Count => IsInitialized ? Items.Count : _numberOfStreams;

        /// <inheritdoc />
        protected override void Initialize()
        {
            var headers = new MetadataStreamHeader[_numberOfStreams];
            for (int i = 0; i < _numberOfStreams; i++)
                headers[i] = MetadataStreamHeader.FromReader(ref _entriesReader);

            for (int i = 0; i < _numberOfStreams; i++)
            {
                var header = headers[i];
                var streamReader = _directoryReader.ForkAbsolute(_directoryReader.Offset + header.Offset, headers[i].Size);
                Items.Add(_context.Parameters.MetadataStreamReader.ReadStream(_context, header, ref streamReader));
            }
        }

    }
}
