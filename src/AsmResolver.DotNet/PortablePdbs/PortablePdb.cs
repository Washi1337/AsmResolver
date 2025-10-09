using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs
{
    public class PortablePdb
    {
        private readonly LazyVariable<PortablePdb, IList<Document>> _documents;

        public PortablePdb(ModuleDefinition? ownerModule)
        {
            Owner = ownerModule;

            _documents = new LazyVariable<PortablePdb, IList<Document>>(pdb => pdb.GetDocuments());
        }

        public static bool TryFromFile(string path, ModuleDefinition? ownerModule, [NotNullWhen(true)] out PortablePdb? pdb)
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

        public static bool TryFromBytes(byte[] bytes, ModuleDefinition? ownerModule, [NotNullWhen(true)] out PortablePdb? pdb)
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

        public static bool TryFromMetadata(MetadataDirectory metadata, ModuleDefinition? ownerModule, [NotNullWhen(true)] out PortablePdb? pdb)
        {
            if (!metadata.TryGetStream<PdbStream>(out var stream))
            {
                pdb = null;
                return false;
            }

            pdb = new SerializedPortablePdb(metadata, ownerModule);
            return true;
        }

        public ModuleDefinition? Owner { get; }

        public byte[] PdbId { get; } = new byte[20];

        public MetadataToken EntryPointToken { get; set; }

        public MethodDefinition? EntryPoint { get; set; }

        public string? FilePath { get; set; }

        public IList<Document> Documents => _documents.GetValue(this);

        public virtual bool TryLookupMember<T>(MetadataToken token, [NotNullWhen(true)] out T? member) where T : class, IMetadataMember
        {
            member = null;
            return false;
        }

        protected virtual IList<Document> GetDocuments() => [];
    }
}
