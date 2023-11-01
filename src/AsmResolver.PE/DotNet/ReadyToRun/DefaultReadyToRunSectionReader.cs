using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides a default implementation for the <see cref="IReadyToRunSectionReader"/> interface.
    /// </summary>
    public class DefaultReadyToRunSectionReader : IReadyToRunSectionReader
    {
        /// <inheritdoc />
        public IReadyToRunSection ReadSection(PEReaderContext context, ReadyToRunSectionType type, ref BinaryStreamReader reader)
        {
            return new CustomReadyToRunSection(type, reader.ReadSegment(reader.Length));
        }
    }
}
