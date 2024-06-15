using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder.Metadata
{
    /// <summary>
    /// Provides a default implementation for <see cref="IMetadataBuffer" /> that produces compressed metadata (#~).
    /// </summary>
    public class MetadataBuffer : IMetadataBuffer
    {
        private readonly string _versionString;

        /// <summary>
        /// Creates a new metadata directory buffer that targets runtime version v4.0.30319.
        /// </summary>
        public MetadataBuffer()
            : this(KnownRuntimeVersions.Clr40)
        {
        }

        /// <summary>
        /// Creates a new metadata directory buffer.
        /// </summary>
        /// <param name="versionString">The runtime version string to use.</param>
        public MetadataBuffer(string versionString)
        {
            _versionString = versionString ?? throw new ArgumentNullException(nameof(versionString));
        }

        /// <inheritdoc />
        public BlobStreamBuffer BlobStream
        {
            get;
        } = new();

        /// <inheritdoc />
        public StringsStreamBuffer StringsStream
        {
            get;
        } = new();

        /// <inheritdoc />
        public UserStringsStreamBuffer UserStringsStream
        {
            get;
        } = new();

        /// <inheritdoc />
        public GuidStreamBuffer GuidStream
        {
            get;
        } = new();

        /// <inheritdoc />
        public TablesStreamBuffer TablesStream
        {
            get;
        } = new();

        /// <summary>
        /// Gets or sets a value indicating whether the strings stream should be optimized for size upon constructing
        /// the metadata directory. This means that strings that are a suffix of another will not be added twice to the
        /// buffer, but rather will be referenced using an offset within the longer string. This avoids lots of
        /// duplication in the final stream.
        /// </summary>
        public bool OptimizeStringIndices
        {
            get;
            set;
        } = true;

        /// <inheritdoc />
        public MetadataDirectory CreateMetadata()
        {
            // Create metadata directory.
            var result = new MetadataDirectory
            {
                VersionString = _versionString,
                IsEncMetadata = TablesStream.IsEncMetadata
            };

            // Create and add streams.
            var tablesStream = Add<TablesStream>(result, TablesStream);

            // Optimize strings.
            if (OptimizeStringIndices)
                OptimizeIndices(tablesStream);

            var stringsStream =  AddIfNotEmpty<StringsStream>(result, StringsStream);
            AddIfNotEmpty<UserStringsStream>(result, UserStringsStream);
            var guidStream = AddIfNotEmpty<GuidStream>(result, GuidStream);
            var blobStream = AddIfNotEmpty<BlobStream>(result, BlobStream);

            // Update index sizes.
            tablesStream.StringIndexSize = stringsStream?.IndexSize ?? IndexSize.Short;
            tablesStream.GuidIndexSize = guidStream?.IndexSize ?? IndexSize.Short;
            tablesStream.BlobIndexSize = blobStream?.IndexSize ?? IndexSize.Short;

            return result;
        }

        private static TStream? AddIfNotEmpty<TStream>(MetadataDirectory metadata, IMetadataStreamBuffer streamBuffer)
            where TStream : class, IMetadataStream
        {
            return !streamBuffer.IsEmpty
                ? Add<TStream>(metadata, streamBuffer)
                : null;
        }

        private static TStream Add<TStream>(MetadataDirectory metadata, IMetadataStreamBuffer streamBuffer)
            where TStream : class, IMetadataStream
        {
            var stream = streamBuffer.CreateStream();
            metadata.Streams.Add(stream);
            return (TStream) stream;
        }

        private void OptimizeIndices(TablesStream tablesStream)
        {
            var translationTable = StringsStream.Optimize();

            OptimizeAssemblyTable(translationTable, tablesStream);
            OptimizeAssemblyReferenceTable(translationTable, tablesStream);
            OptimizeEventDefinitionTable(translationTable, tablesStream);
            OptimizeExportedTypeTable(translationTable, tablesStream);
            OptimizeFieldDefinitionTable(translationTable, tablesStream);
            OptimizeFileReferenceTable(translationTable, tablesStream);
            OptimizeGenericParameterTable(translationTable, tablesStream);
            OptimizeImplementationMapTable(translationTable, tablesStream);
            OptimizeManifestResourceTable(translationTable, tablesStream);
            OptimizeMemberReferenceTable(translationTable, tablesStream);
            OptimizeMethodDefinitionTable(translationTable, tablesStream);
            OptimizeModuleDefinitionTable(translationTable, tablesStream);
            OptimizeModuleReferenceTable(translationTable, tablesStream);
            OptimizeParameterDefinitionTable(translationTable, tablesStream);
            OptimizePropertyDefinitionTable(translationTable, tablesStream);
            OptimizeTypeDefinitionTable(translationTable, tablesStream);
            OptimizeTypeReferenceTable(translationTable, tablesStream);
        }

        private static void OptimizeAssemblyTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<AssemblyDefinitionRow>(TableIndex.Assembly);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
                row.Culture = translationTable[row.Culture];
            }
        }

        private static void OptimizeAssemblyReferenceTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<AssemblyReferenceRow>(TableIndex.AssemblyRef);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
                row.Culture = translationTable[row.Culture];
            }
        }

        private static void OptimizeEventDefinitionTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<EventDefinitionRow>(TableIndex.Event);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
            }
        }

        private static void OptimizeExportedTypeTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<ExportedTypeRow>(TableIndex.ExportedType);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
                row.Namespace = translationTable[row.Namespace];
            }
        }

        private static void OptimizeFieldDefinitionTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<FieldDefinitionRow>(TableIndex.Field);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
            }
        }

        private static void OptimizeFileReferenceTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<FileReferenceRow>(TableIndex.File);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
            }
        }

        private static void OptimizeGenericParameterTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<GenericParameterRow>(TableIndex.GenericParam);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
            }
        }

        private static void OptimizeImplementationMapTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<ImplementationMapRow>(TableIndex.ImplMap);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.ImportName = translationTable[row.ImportName];
            }
        }

        private static void OptimizeManifestResourceTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<ManifestResourceRow>(TableIndex.ManifestResource);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
            }
        }

        private static void OptimizeMemberReferenceTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<MemberReferenceRow>(TableIndex.MemberRef);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
            }
        }

        private static void OptimizeMethodDefinitionTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<MethodDefinitionRow>(TableIndex.Method);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
            }
        }

        private static void OptimizeModuleDefinitionTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<ModuleDefinitionRow>(TableIndex.Module);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
            }
        }

        private static void OptimizeModuleReferenceTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<ModuleReferenceRow>(TableIndex.ModuleRef);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
            }
        }

        private static void OptimizeParameterDefinitionTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<ParameterDefinitionRow>(TableIndex.Param);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
            }
        }

        private static void OptimizePropertyDefinitionTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<PropertyDefinitionRow>(TableIndex.Property);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
            }
        }

        private static void OptimizeTypeDefinitionTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<TypeDefinitionRow>(TableIndex.TypeDef);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
                row.Namespace = translationTable[row.Namespace];
            }
        }

        private static void OptimizeTypeReferenceTable(IDictionary<uint, uint> translationTable, TablesStream tablesStream)
        {
            var table = tablesStream.GetTable<TypeReferenceRow>(TableIndex.TypeRef);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                ref var row = ref table.GetRowRef(rid);
                row.Name = translationTable[row.Name];
                row.Namespace = translationTable[row.Namespace];
            }
        }
    }
}
