using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.DotNet.Serialized;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs;

public partial class PortablePdb
{
    public PortablePdb(ModuleDefinition ownerModule)
    {
        Owner = ownerModule;
    }

    public static bool TryFromFile(string path, SerializedModuleDefinition ownerModule, [NotNullWhen(true)] out SerializedPortablePdb? pdb)
    {
        MetadataDirectory metadata;
        try
        {
            metadata = MetadataDirectory.FromFile(path);
        }
        catch
        {
            pdb = null;
            return false;
        }

        if (!TryFromMetadata(metadata, ownerModule, out pdb))
            return false;

        pdb.FilePath = path;
        return true;
    }

    public static bool TryFromBytes(byte[] bytes, SerializedModuleDefinition ownerModule, [NotNullWhen(true)] out SerializedPortablePdb? pdb)
    {
        MetadataDirectory metadata;
        try
        {
            metadata = MetadataDirectory.FromBytes(bytes);
        }
        catch
        {
            pdb = null;
            return false;
        }

        return TryFromMetadata(metadata, ownerModule, out pdb);
    }

    public static bool TryFromMetadata(MetadataDirectory metadata, SerializedModuleDefinition ownerModule, [NotNullWhen(true)] out SerializedPortablePdb? pdb)
    {
        if (!metadata.TryGetStream<PdbStream>(out var stream))
        {
            pdb = null;
            return false;
        }

        pdb = new SerializedPortablePdb(metadata, ownerModule);
        return true;
    }

    public ModuleDefinition Owner { get; }

    public byte[] PdbId { get; } = new byte[20];

    public string? FilePath { get; set; }

    public virtual bool TryLookupMember<T>(MetadataToken token, [NotNullWhen(true)] out T? member) where T : class, IMetadataMember
    {
        member = null;
        return false;
    }
}
