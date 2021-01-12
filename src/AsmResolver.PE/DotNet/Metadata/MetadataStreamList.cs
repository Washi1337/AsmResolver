using System;
using AsmResolver.Collections;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides an implementation of a lazy-initialized list of metadata streams present in a metadata directory.
    /// </summary>
    public class MetadataStreamList : LazyList<IMetadataStream>
    {
        private readonly PEReadContext _context;
        private readonly IBinaryStreamReader _directoryReader;
        private readonly IBinaryStreamReader _entriesReader;
        private readonly int _numberOfStreams;

        /// <summary>
        /// Prepares a new lazy-initialized metadata stream list.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="directoryReader">The input stream containing the metadata directory.</param>
        /// <param name="entriesReader">The input stream containing the metadata stream entries.</param>
        /// <param name="numberOfStreams">The number of streams.</param>
        public MetadataStreamList(
            PEReadContext context,
            IBinaryStreamReader directoryReader, 
            IBinaryStreamReader entriesReader, 
            int numberOfStreams)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _directoryReader = directoryReader ?? throw new ArgumentNullException(nameof(directoryReader));
            _entriesReader = entriesReader ?? throw new ArgumentNullException(nameof(entriesReader));
            _numberOfStreams = numberOfStreams;
        }

        /// <inheritdoc />
        public override int Count => IsInitialized ? Items.Count : _numberOfStreams; 

        /// <inheritdoc />
        protected override void Initialize()
        {
            var headers = new MetadataStreamHeader[_numberOfStreams];
            for (int i = 0; i < _numberOfStreams; i++)
                headers[i] = MetadataStreamHeader.FromReader(_entriesReader);

            for (int i = 0; i < _numberOfStreams; i++)
            {
                var header = headers[i];
                var streamReader = _directoryReader.Fork(_directoryReader.Offset + header.Offset, headers[i].Size);
                Items.Add(_context.Parameters.MetadataStreamReader.ReadStream(_context, header, streamReader));
            }
        }
        
    }
}