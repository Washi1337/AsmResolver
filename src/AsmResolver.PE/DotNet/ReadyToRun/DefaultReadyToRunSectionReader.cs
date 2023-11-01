using AsmResolver.IO;
using AsmResolver.PE.File.Headers;
using static AsmResolver.PE.DotNet.ReadyToRun.ReadyToRunSectionType;

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
                CompilerIdentifier => new CompilerIdentifierSection(reader.ReadAsciiString()),
                ImportSections => new SerializedImportSectionsSection(context, ref reader),
                RuntimeFunctions when context.File.FileHeader.Machine == MachineType.Amd64 => new SerializedX64RuntimeFunctionsSection(context, ref reader),
                MethodDefEntryPoints => new SerializedMethodEntryPointsSection(ref reader),
                _ => new CustomReadyToRunSection(type, reader.ReadSegment(reader.Length))
            };
        }
    }
}
