using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides a lazy-initialized implementation of the <see cref="DebugInfoSection"/> that is read from a file.
    /// </summary>
    public class SerializedDebugInfoSection : DebugInfoSection
    {
        private readonly PEReaderContext _context;
        private readonly BinaryStreamReader _reader;

        /// <summary>
        /// Reads a debug information section from the provided input stream.
        /// </summary>
        /// <param name="context">The context the reader is situated in.</param>
        /// <param name="reader">The input stream.</param>
        public SerializedDebugInfoSection(PEReaderContext context, BinaryStreamReader reader)
        {
            Offset = reader.Offset;
            Rva = reader.Rva;

            _context = context;
            _reader = reader;
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override BinaryStreamReader CreateReader() => _reader.Fork();

        /// <inheritdoc />
        protected override NativeArray<DebugInfo> GetEntries()
        {
            return NativeArray<DebugInfo>.FromReader(_reader, r => new SerializedDebugInfo(_context, r));
        }
    }
}
