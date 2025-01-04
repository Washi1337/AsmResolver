using AsmResolver.IO;
using AsmResolver.PE.File;
using static AsmResolver.PE.DotNet.ReadyToRun.ReadyToRunSectionType;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides a default implementation for the <see cref="IReadyToRunSectionReader"/> interface.
    /// </summary>
    public class DefaultReadyToRunSectionReader : IReadyToRunSectionReader
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="DefaultReadyToRunSectionReader"/> class.
        /// </summary>
        public static DefaultReadyToRunSectionReader Instance { get; } = new();

        /// <inheritdoc />
        public IReadyToRunSection ReadSection(PEReaderContext context, ReadyToRunSectionType type, ref BinaryStreamReader reader)
        {
            return type switch
            {
                CompilerIdentifier => ReadCompilerIdentifierSection(ref reader),
                ImportSections => new SerializedImportSectionsSection(context, ref reader),
                RuntimeFunctions when context.File.FileHeader.Machine == MachineType.Amd64 => new SerializedX64RuntimeFunctionsSection(context, ref reader),
                MethodDefEntryPoints => new SerializedMethodEntryPointsSection(ref reader),
                ReadyToRunSectionType.DebugInfo => new SerializedDebugInfoSection(context, reader),
                _ => ReadUnsupportedReadyToRunSection(ref reader)
            };

            CompilerIdentifierSection ReadCompilerIdentifierSection(ref BinaryStreamReader reader)
            {
                ulong offset = reader.Offset;
                uint rva = reader.Rva;

                var section = new CompilerIdentifierSection(reader.ReadAsciiString());
                section.UpdateOffsets(new RelocationParameters(offset, rva));

                return section;
            }

            CustomReadyToRunSection ReadUnsupportedReadyToRunSection(ref BinaryStreamReader reader)
            {
                ulong offset = reader.Offset;
                uint rva = reader.Rva;

                var section = new CustomReadyToRunSection(type, reader.ReadSegment(reader.Length));
                section.UpdateOffsets(new RelocationParameters(offset, rva));

                return section;
            }
        }
    }
}
