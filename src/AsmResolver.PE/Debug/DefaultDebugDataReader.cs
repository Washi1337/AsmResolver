using System;
using AsmResolver.PE.File;

namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Provides a default implementation of the <see cref="IDebugDataReader"/> interface.
    /// </summary>
    public class DefaultDebugDataReader : IDebugDataReader
    {
        private readonly PEFile _peFile;

        /// <summary>
        /// Creates a new instance of the <see cref="DefaultDebugDataReader"/> class.
        /// </summary>
        /// <param name="peFile">The PE file to read from.</param>
        public DefaultDebugDataReader(PEFile peFile)
        {
            _peFile = peFile ?? throw new ArgumentNullException(nameof(peFile));
        }

        /// <inheritdoc />
        public IDebugDataSegment ReadDebugData(DebugDataType type, uint rva, uint size)
        {
            if (!_peFile.TryCreateReaderAtRva(rva, size, out var reader))
                return null;

            return new CustomDebugDataSegment(type, DataSegment.FromReader(reader));
        }
    }
}