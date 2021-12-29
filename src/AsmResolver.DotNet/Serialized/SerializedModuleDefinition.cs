using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE;
using AsmResolver.PE.Debug;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata.Guid;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.DotNet.Metadata.UserStrings;
using AsmResolver.PE.Win32Resources;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="ModuleDefinition"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public partial class SerializedModuleDefinition : ModuleDefinition
    {
        private readonly ModuleDefinitionRow _row;

        /// <summary>
        /// Interprets a PE image as a .NET module.
        /// </summary>
        /// <param name="peImage">The image to interpret as a .NET module.</param>
        /// <param name="readerParameters">The parameters to use while reading the module.</param>
        public SerializedModuleDefinition(IPEImage peImage, ModuleReaderParameters readerParameters)
            : base(new MetadataToken(TableIndex.Module, 1))
        {
            if (peImage is null)
                throw new ArgumentNullException(nameof(peImage));
            if (readerParameters is null)
                throw new ArgumentNullException(nameof(readerParameters));

            var metadata = peImage.DotNetDirectory?.Metadata;
            if (metadata is null)
                throw new BadImageFormatException("Input PE image does not contain a .NET metadata directory.");

            var tablesStream = metadata.GetStream<TablesStream>();
            if (tablesStream is null)
                throw new BadImageFormatException(".NET metadata directory does not define a tables stream.");

            var moduleTable = tablesStream.GetTable<ModuleDefinitionRow>(TableIndex.Module);
            if (!moduleTable.TryGetByRid(1, out _row))
                throw new BadImageFormatException("Module definition table does not contain any rows.");

            // Store parameters in fields.
            ReaderContext = new ModuleReaderContext(peImage, this, readerParameters);

            // Copy over PE header fields.
            FilePath = peImage.FilePath;
            MachineType = peImage.MachineType;
            FileCharacteristics = peImage.Characteristics;
            PEKind = peImage.PEKind;
            SubSystem = peImage.SubSystem;
            DllCharacteristics = peImage.DllCharacteristics;
            TimeDateStamp = peImage.TimeDateStamp;

            // Copy over "simple" columns.
            Generation = _row.Generation;
            Attributes = peImage.DotNetDirectory!.Flags;

            // Initialize member factory.
            _memberFactory = new CachedSerializedMemberFactory(ReaderContext);

            // Find assembly definition and corlib assembly.
            Assembly = FindParentAssembly();
            CorLibTypeFactory = CreateCorLibTypeFactory();

            OriginalTargetRuntime = DetectTargetRuntime();
            MetadataResolver = new DefaultMetadataResolver(CreateAssemblyResolver(
                readerParameters.PEReaderParameters.FileService));

            // Prepare lazy RID lists.
            _fieldLists = new LazyRidListRelation<TypeDefinitionRow>(metadata, TableIndex.TypeDef,
                (rid, _) => rid, tablesStream.GetFieldRange);
            _methodLists = new LazyRidListRelation<TypeDefinitionRow>(metadata, TableIndex.TypeDef,
                (rid, _) => rid, tablesStream.GetMethodRange);
            _paramLists = new LazyRidListRelation<MethodDefinitionRow>(metadata, TableIndex.Method,
                (rid, _) => rid, tablesStream.GetParameterRange);
            _propertyLists = new LazyRidListRelation<PropertyMapRow>(metadata, TableIndex.PropertyMap,
                (_, map) => map.Parent, tablesStream.GetPropertyRange);
            _eventLists = new LazyRidListRelation<EventMapRow>(metadata, TableIndex.EventMap,
                (_, map) => map.Parent, tablesStream.GetEventRange);
        }

        /// <inheritdoc />
        public override IDotNetDirectory DotNetDirectory => ReaderContext.Image.DotNetDirectory!;

        /// <summary>
        /// Gets the reading context that is used for reading the contents of the module.
        /// </summary>
        public ModuleReaderContext ReaderContext
        {
            get;
        }

        /// <inheritdoc />
        public override IMetadataMember LookupMember(MetadataToken token) =>
            !TryLookupMember(token, out var member)
                ? throw new ArgumentException($"Cannot resolve metadata token {token}.")
                : member;

        /// <inheritdoc />
        public override bool TryLookupMember(MetadataToken token, [NotNullWhen(true)] out IMetadataMember? member) =>
            _memberFactory.TryLookupMember(token, out member);

        /// <inheritdoc />
        public override string LookupString(MetadataToken token) =>
            !TryLookupString(token, out var member)
                ? throw new ArgumentException($"Cannot resolve string token {token}.")
                : member;

        /// <inheritdoc />
        public override bool TryLookupString(MetadataToken token, [NotNullWhen(true)] out string? value)
        {
            if (!ReaderContext.Metadata.TryGetStream<UserStringsStream>(out var userStringsStream))
            {
                value = null;
                return false;
            }

            value = userStringsStream.GetStringByIndex(token.Rid);
            return value is not null;
        }

        /// <inheritdoc />
        public override IndexEncoder GetIndexEncoder(CodedIndex codedIndex) =>
            ReaderContext.Metadata.GetStream<TablesStream>().GetIndexEncoder(codedIndex);

        /// <inheritdoc />
        public override IEnumerable<TypeReference> GetImportedTypeReferences()
        {
            var table = ReaderContext.Metadata
                .GetStream<TablesStream>()
                .GetTable(TableIndex.TypeRef);

            for (uint rid = 1; rid <= table.Count; rid++)
            {
                if (TryLookupMember(new MetadataToken(TableIndex.TypeRef, rid), out var member)
                    && member is TypeReference reference)
                {
                    yield return reference;
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<MemberReference> GetImportedMemberReferences()
        {
            var table = ReaderContext.Metadata
                .GetStream<TablesStream>()
                .GetTable(TableIndex.MemberRef);

            for (uint rid = 1; rid <= table.Count; rid++)
            {
                if (TryLookupMember(new MetadataToken(TableIndex.MemberRef, rid), out var member)
                    && member is MemberReference reference)
                {
                    yield return reference;
                }
            }
        }

        /// <inheritdoc />
        protected override Utf8String? GetName()
        {
            return ReaderContext.Metadata.TryGetStream<StringsStream>(out var stringsStream)
                ? stringsStream.GetStringByIndex(_row.Name)
                : null;
        }

        /// <inheritdoc />
        protected override Guid GetMvid()
        {
            return ReaderContext.Metadata.TryGetStream<GuidStream>(out var guidStream)
                ? guidStream.GetGuidByIndex(_row.Mvid)
                : Guid.Empty;
        }

        /// <inheritdoc />
        protected override Guid GetEncId()
        {
            return ReaderContext.Metadata.TryGetStream<GuidStream>(out var guidStream)
                ? guidStream.GetGuidByIndex(_row.EncId)
                : Guid.Empty;
        }

        /// <inheritdoc />
        protected override Guid GetEncBaseId()
        {
            return ReaderContext.Metadata.TryGetStream<GuidStream>(out var guidStream)
                ? guidStream.GetGuidByIndex(_row.EncBaseId)
                : Guid.Empty;
        }

        /// <inheritdoc />
        protected override IList<TypeDefinition> GetTopLevelTypes()
        {
            EnsureTypeDefinitionTreeInitialized();

            var types = new OwnedCollection<ModuleDefinition, TypeDefinition>(this);

            var typeDefTable = ReaderContext.Metadata
                .GetStream<TablesStream>()
                .GetTable<TypeDefinitionRow>(TableIndex.TypeDef);

            for (int i = 0; i < typeDefTable.Count; i++)
            {
                uint rid = (uint) i + 1;
                if (_typeDefTree.GetKey(rid) == 0)
                {
                    var token = new MetadataToken(TableIndex.TypeDef, rid);
                    types.Add(_memberFactory.LookupTypeDefinition(token)!);
                }
            }

            return types;
        }

        /// <inheritdoc />
        protected override IList<AssemblyReference> GetAssemblyReferences()
        {
            var result = new OwnedCollection<ModuleDefinition, AssemblyReference>(this);

            var table = ReaderContext.Metadata
                .GetStream<TablesStream>()
                .GetTable<AssemblyReferenceRow>(TableIndex.AssemblyRef);

            // Don't use the member factory here, this method may be called before the member factory is initialized.
            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.AssemblyRef, (uint) i + 1);
                result.Add(new SerializedAssemblyReference(ReaderContext, token, table[i]));
            }

            return result;
        }

        /// <inheritdoc />
        protected override IList<ModuleReference> GetModuleReferences()
        {
            var result = new OwnedCollection<ModuleDefinition, ModuleReference>(this);

            var table = ReaderContext.Metadata
                .GetStream<TablesStream>()
                .GetTable(TableIndex.ModuleRef);

            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.ModuleRef, (uint) i + 1);
                if (_memberFactory.TryLookupMember(token, out var member) && member is ModuleReference module)
                    result.Add(module);
            }

            return result;
        }

        /// <inheritdoc />
        protected override IList<FileReference> GetFileReferences()
        {
            var result = new OwnedCollection<ModuleDefinition, FileReference>(this);

            var table = ReaderContext.Metadata
                .GetStream<TablesStream>()
                .GetTable(TableIndex.File);

            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.File, (uint) i + 1);
                if (_memberFactory.TryLookupMember(token, out var member) && member is FileReference file)
                    result.Add(file);
            }

            return result;
        }

        /// <inheritdoc />
        protected override IList<ManifestResource> GetResources()
        {
            var result = new OwnedCollection<ModuleDefinition, ManifestResource>(this);

            var table = ReaderContext.Metadata
                .GetStream<TablesStream>()
                .GetTable(TableIndex.ManifestResource);

            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.ManifestResource, (uint) i + 1);
                if (_memberFactory.TryLookupMember(token, out var member) && member is ManifestResource resource)
                    result.Add(resource);
            }

            return result;
        }

        /// <inheritdoc />
        protected override IList<ExportedType> GetExportedTypes()
        {
            var result = new OwnedCollection<ModuleDefinition, ExportedType>(this);

            var table = ReaderContext.Metadata
                .GetStream<TablesStream>()
                .GetTable(TableIndex.ExportedType);

            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.ExportedType, (uint) i + 1);
                if (_memberFactory.TryLookupMember(token, out var member) && member is ExportedType exportedType)
                    result.Add(exportedType);
            }

            return result;
        }

        /// <inheritdoc />
        protected override string GetRuntimeVersion() => ReaderContext.Metadata!.VersionString;

        /// <inheritdoc />
        protected override IManagedEntrypoint? GetManagedEntrypoint()
        {
            if ((DotNetDirectory.Flags & DotNetDirectoryFlags.NativeEntrypoint) != 0)
            {
                // TODO: native entrypoints.
                return null;
            }

            if (DotNetDirectory.Entrypoint != 0)
                return LookupMember(DotNetDirectory.Entrypoint) as IManagedEntrypoint;

            return null;
        }

        /// <inheritdoc />
        protected override IResourceDirectory? GetNativeResources() => ReaderContext.Image.Resources;

        /// <inheritdoc />
        protected override IList<DebugDataEntry> GetDebugData() => new List<DebugDataEntry>(ReaderContext.Image.DebugData);

        private AssemblyDefinition? FindParentAssembly()
        {
            var assemblyTable = ReaderContext.Metadata
                .GetStream<TablesStream>()
                .GetTable<AssemblyDefinitionRow>();

            if (assemblyTable.Count > 0)
            {
                var assembly = new SerializedAssemblyDefinition(
                    ReaderContext,
                    new MetadataToken(TableIndex.Assembly, 1),
                    assemblyTable[0],
                    this);
                return assembly;
            }

            return null;
        }

        private CorLibTypeFactory CreateCorLibTypeFactory()
        {
            return FindMostRecentCorLib() is { } corLib
                ? new CorLibTypeFactory(corLib)
                : CorLibTypeFactory.CreateMscorlib40TypeFactory(this);
        }

        private IResolutionScope? FindMostRecentCorLib()
        {
            // TODO: perhaps check public key tokens.

            IResolutionScope? mostRecentCorLib = null;
            var mostRecentVersion = new Version();
            foreach (var reference in AssemblyReferences)
            {
                if (reference.Name is not null && KnownCorLibs.KnownCorLibNames.Contains(reference.Name))
                {
                    if (mostRecentVersion < reference.Version)
                        mostRecentCorLib = reference;
                }
            }

            if (mostRecentCorLib is null && Assembly is {Name: { } name })
            {
                if (KnownCorLibs.KnownCorLibNames.Contains(name))
                    mostRecentCorLib = this;
            }

            return mostRecentCorLib;
        }
    }
}
