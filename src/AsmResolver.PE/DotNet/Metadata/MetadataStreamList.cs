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
        private readonly MetadataStreamHeader[] _streamHeaders;
        private readonly IMetadata _owner;
        private readonly BinaryStreamReader _directoryReader;
        private readonly MetadataStreamReaderFlags _streamReaderFlags;

        /// <summary>
        /// Prepares a new lazy-initialized metadata stream list.
        /// </summary>
        /// <param name="owner">The owner of the metadata stream list.</param>
        /// <param name="context">The reader context.</param>
        /// <param name="streamReaderFlags">Flags describing the currently read metadata.</param>
        /// <param name="streamHeaders">The stream headers.</param>
        /// <param name="directoryReader">The input stream containing the metadata directory.</param>
        public MetadataStreamList(
            IMetadata owner,
            MetadataReaderContext context,
            MetadataStreamReaderFlags streamReaderFlags,
            MetadataStreamHeader[] streamHeaders,
            in BinaryStreamReader directoryReader)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _streamHeaders = streamHeaders;
            _owner = owner;
            _directoryReader = directoryReader;
            _streamReaderFlags = streamReaderFlags;
        }

        /// <inheritdoc />
        public override int Count => IsInitialized ? Items.Count : _streamHeaders.Length;

        /// <inheritdoc />
        protected override void Initialize()
        {
            foreach (var header in _streamHeaders)
            {
                var streamReader = _directoryReader.ForkAbsolute(_directoryReader.Offset + header.Offset, header.Size);
                var stream = _context.MetadataStreamReader.ReadStream(_context, _streamReaderFlags, header, ref streamReader);
                Items.Add(stream);
            }
        }

        /// <inheritdoc />
        protected override void PostInitialize()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] is ILazyMetadataStream lazyStream)
                    lazyStream.Initialize(_owner);
            }
        }
    }
}
