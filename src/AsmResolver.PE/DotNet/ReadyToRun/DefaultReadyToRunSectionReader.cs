using AsmResolver.IO;
using AsmResolver.PE.File.Headers;

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
            return type switch
            {
                ReadyToRunSectionType.CompilerIdentifier => new CompilerIdentifierSection(reader.ReadAsciiString()),
                ReadyToRunSectionType.ImportSections => new SerializedImportSectionsSection(context, ref reader),
                ReadyToRunSectionType.RuntimeFunctions when context.File.FileHeader.Machine == MachineType.Amd64 => new SerializedX64RuntimeFunctionsSection(context, ref reader),
                _ => new CustomReadyToRunSection(type, reader.ReadSegment(reader.Length))
            };
        }
    }
}
