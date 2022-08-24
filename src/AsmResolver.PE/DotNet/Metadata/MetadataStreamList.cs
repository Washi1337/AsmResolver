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
        private readonly MetadataReaderContext _context;
        private readonly IMetadata _owner;
        private readonly int _numberOfStreams;
        private readonly BinaryStreamReader _directoryReader;
        private BinaryStreamReader _entriesReader;

        /// <summary>
        /// Prepares a new lazy-initialized metadata stream list.
        /// </summary>
        /// <param name="owner">The owner of the metadata stream list.</param>
        /// <param name="context">The reader context.</param>
        /// <param name="directoryReader">The input stream containing the metadata directory.</param>
        /// <param name="entriesReader">The input stream containing the metadata stream entries.</param>
        /// <param name="numberOfStreams">The number of streams.</param>
        public MetadataStreamList(
            IMetadata owner,
            MetadataReaderContext context,
            in BinaryStreamReader directoryReader,
            in BinaryStreamReader entriesReader,
            int numberOfStreams)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _owner = owner;
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
                var stream = _context.MetadataStreamReader.ReadStream(_context, header, ref streamReader);

                Items.Add(stream);
            }
        }

        /// <inheritdoc />
        protected override void PostInitialize()
        {
            for (int i = 0; i < _numberOfStreams; i++)
            {
                if (Items[i] is ILazyMetadataStream lazyStream)
                    lazyStream.Initialize(_owner);
            }
        }
    }
}
