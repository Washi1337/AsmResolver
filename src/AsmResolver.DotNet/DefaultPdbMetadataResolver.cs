using System.IO;
using System.IO.Compression;
using System.Linq;
using AsmResolver.DotNet.PortablePdbs;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.DotNet.Serialized;
using AsmResolver.PE.Debug;

namespace AsmResolver.DotNet;

public class DefaultPdbMetadataResolver : IPdbMetadataResolver
{
    public static DefaultPdbMetadataResolver Instance { get; } = new();

    public PdbReaderContext? ResolvePortablePdb(SerializedModuleDefinition module)
    {
        if (module.DotNetDirectory?.Metadata is { } metadata && PortablePdb.TryFromMetadata(metadata, module, out var pdb))
        {
            return pdb.PdbReaderContext;
        }

        // System.Reflection.Metadata tries reading from a file first, so we will as well
        if (module.FilePath is { } path && PortablePdb.TryFromFile(Path.ChangeExtension(path, ".pdb"), module, out pdb))
        {
            return pdb.PdbReaderContext;
        }

        var pdbSection = module.DebugData.Select(dd => dd.Contents).OfType<PortablePdbDataSegment>().FirstOrDefault();
        if (pdbSection is not null)
        {
            var memoryStream = new MemoryStream(pdbSection.CompressedContents.ToArray());
            var decompressStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
            var pdbData = new byte[pdbSection.UncompressedSize];
            var resultStream = new MemoryStream(pdbData);
            decompressStream.CopyTo(resultStream);
            if (PortablePdb.TryFromBytes(pdbData, module, out pdb))
            {
                return pdb.PdbReaderContext;
            }
        }

        return null;
    }
}
