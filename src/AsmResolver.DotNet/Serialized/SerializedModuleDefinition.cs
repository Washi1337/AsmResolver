using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.PortablePdbs;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE;
using AsmResolver.PE.Debug;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.Platforms;
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
        public SerializedModuleDefinition(PEImage peImage, ModuleReaderParameters readerParameters)
            : base(new MetadataToken(TableIndex.Module, 1))
        {
            if (peImage is null)
                throw new ArgumentNullException(nameof(peImage));
            if (readerParameters is null)
                throw new ArgumentNullException(nameof(readerParameters));

            var metadata = peImage.DotNetDirectory?.Metadata;
            if (metadata is null)
                throw new BadImageFormatException("Input PE image does not contain a .NET metadata directory.");

            var streams = metadata.GetImpliedStreamSelection();

            var tablesStream = streams.TablesStream;
            if (tablesStream is null)
                throw new BadImageFormatException(".NET metadata directory does not define a tables stream.");

            var moduleTable = tablesStream.GetTable<ModuleDefinitionRow>(TableIndex.Module);
            if (!moduleTable.TryGetByRid(1, out _row))
                throw new BadImageFormatException("Module definition table does not contain any rows.");

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

            // If we cannot determine the target runtime the image was compiled against, assume .netfx 4.0 since it's
            // the most common case for standalone binaries.
            OriginalTargetRuntime = TargetRuntimeProber.TryGetLikelyTargetRuntime(peImage, out var originalTargetRuntime)
                ? originalTargetRuntime
                : DotNetRuntimeInfo.NetFramework(4, 0);

            // Initialize member factory.
            ReaderContext = new ModuleReaderContext(peImage, this, readerParameters);
            _memberFactory = new CachedSerializedMemberFactory(ReaderContext);

            // Find assembly definition and corlib assembly.
            Assembly = FindParentAssembly();
            CorLibTypeFactory = CreateCorLibTypeFactory();

            // Prepare lazy RID lists.
            _fieldLists = new LazyRidListRelation<TypeDefinitionRow>(metadata, TableIndex.Field, TableIndex.TypeDef,
                (rid, _) => rid, tablesStream.GetFieldRange);
            _methodLists = new LazyRidListRelation<TypeDefinitionRow>(metadata, TableIndex.Method, TableIndex.TypeDef,
                (rid, _) => rid, tablesStream.GetMethodRange);
            _paramLists = new LazyRidListRelation<MethodDefinitionRow>(metadata, TableIndex.Param, TableIndex.Method,
                (rid, _) => rid, tablesStream.GetParameterRange);
            _propertyLists = new LazyRidListRelation<PropertyMapRow>(metadata, TableIndex.Property, TableIndex.PropertyMap,
                (_, map) => map.Parent, tablesStream.GetPropertyRange);
            _eventLists = new LazyRidListRelation<EventMapRow>(metadata, TableIndex.Event, TableIndex.EventMap,
                (_, map) => map.Parent, tablesStream.GetEventRange);
        }

        /// <inheritdoc />
        public override DotNetDirectory DotNetDirectory => ReaderContext.Image.DotNetDirectory!;

        /// <summary>
        /// Gets the reading context that is used for reading the contents of the module.
        /// </summary>
        public ModuleReaderContext ReaderContext
        {
            get;
        }

        [LazyProperty]
        public partial PdbReaderContext? PdbReaderContext
        {
            get;
        }

        private PdbReaderContext? GetPdbReaderContext() => ReaderContext.Parameters.PdbMetadataResolver.ResolvePortablePdb(this);

        /// <inheritdoc />
        public override bool HasCustomAttributes => CustomAttributesInternal is null
            ? HasNonEmptyCustomAttributes(this)
            : CustomAttributes.Count > 0;

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
            if (ReaderContext.Streams.UserStringsStream is not { } userStringsStream)
            {
                value = null;
                return false;
            }

            value = userStringsStream.GetStringByIndex(token.Rid);
            return value is not null;
        }

        /// <inheritdoc />
        public override IndexEncoder GetIndexEncoder(CodedIndex codedIndex) =>
            ReaderContext.Streams.TablesStream!.GetIndexEncoder(codedIndex);

        /// <inheritdoc />
        public override IEnumerable<IMetadataMember> EnumerateTableMembers(TableIndex tableIndex)
        {
            var table = ReaderContext.Streams.TablesStream!.GetTable(tableIndex);

            for (uint rid = 1; rid <= table.Count; rid++)
            {
                if (TryLookupMember(new MetadataToken(tableIndex, rid), out var member))
                    yield return member;
            }
        }

        /// <inheritdoc />
        protected override Utf8String? GetName() => ReaderContext.Streams.StringsStream?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override Guid GetMvid() => ReaderContext.Streams.GuidStream?.GetGuidByIndex(_row.Mvid) ?? Guid.Empty;

        /// <inheritdoc />
        protected override Guid GetEncId() => ReaderContext.Streams.GuidStream?.GetGuidByIndex(_row.EncId) ?? Guid.Empty;

        /// <inheritdoc />
        protected override Guid GetEncBaseId() => ReaderContext.Streams.GuidStream?.GetGuidByIndex(_row.EncBaseId) ?? Guid.Empty;

        /// <inheritdoc />
        protected override IList<TypeDefinition> GetTopLevelTypes()
        {
            EnsureTypeDefinitionTreeInitialized();

            var typeDefTable = ReaderContext.Streams.TablesStream!.GetTable<TypeDefinitionRow>(TableIndex.TypeDef);
            int nestedTypeCount = ReaderContext.Streams.TablesStream.GetTable(TableIndex.NestedClass).Count;

            var types = new MemberCollection<ITypeOwner, TypeDefinition>(this,
                typeDefTable.Count - nestedTypeCount);

            for (int i = 0; i < typeDefTable.Count; i++)
            {
                uint rid = (uint) i + 1;
                if (_typeDefTree.GetKey(rid) == 0)
                {
                    var token = new MetadataToken(TableIndex.TypeDef, rid);
                    types.AddNoOwnerCheck(_memberFactory.LookupTypeDefinition(token)!);
                }
            }

            return types;
        }

        /// <inheritdoc />
        protected override IList<AssemblyReference> GetAssemblyReferences()
        {
            var table = ReaderContext.Streams.TablesStream!.GetTable<AssemblyReferenceRow>(TableIndex.AssemblyRef);

            var result = new MemberCollection<ModuleDefinition, AssemblyReference>(this, table.Count);

            // Don't use the member factory here, this method may be called before the member factory is initialized.
            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.AssemblyRef, (uint) i + 1);
                result.AddNoOwnerCheck(new SerializedAssemblyReference(ReaderContext, token, table[i]));
            }

            return result;
        }

        /// <inheritdoc />
        protected override IList<ModuleReference> GetModuleReferences()
        {
            var table = ReaderContext.Streams.TablesStream!.GetTable(TableIndex.ModuleRef);

            var result = new MemberCollection<ModuleDefinition, ModuleReference>(this, table.Count);

            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.ModuleRef, (uint) i + 1);
                if (_memberFactory.TryLookupMember(token, out var member) && member is ModuleReference module)
                    result.AddNoOwnerCheck(module);
            }

            return result;
        }

        /// <inheritdoc />
        protected override IList<FileReference> GetFileReferences()
        {
            var table = ReaderContext.Streams.TablesStream!.GetTable(TableIndex.File);

            var result = new MemberCollection<ModuleDefinition, FileReference>(this, table.Count);

            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.File, (uint) i + 1);
                if (_memberFactory.TryLookupMember(token, out var member) && member is FileReference file)
                    result.AddNoOwnerCheck(file);
            }

            return result;
        }

        /// <inheritdoc />
        protected override IList<ManifestResource> GetResources()
        {
            var table = ReaderContext.Streams.TablesStream!.GetTable(TableIndex.ManifestResource);

            var result = new MemberCollection<ModuleDefinition, ManifestResource>(this, table.Count);

            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.ManifestResource, (uint) i + 1);
                if (_memberFactory.TryLookupMember(token, out var member) && member is ManifestResource resource)
                    result.AddNoOwnerCheck(resource);
            }

            return result;
        }

        /// <inheritdoc />
        protected override IList<ExportedType> GetExportedTypes()
        {
            var table = ReaderContext.Streams.TablesStream!.GetTable(TableIndex.ExportedType);

            var result = new MemberCollection<ModuleDefinition, ExportedType>(this, table.Count);

            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.ExportedType, (uint) i + 1);
                if (_memberFactory.TryLookupMember(token, out var member) && member is ExportedType exportedType)
                    result.AddNoOwnerCheck(exportedType);
            }

            return result;
        }

        /// <inheritdoc />
        protected override string GetRuntimeVersion() => ReaderContext.Metadata.VersionString;

        /// <inheritdoc />
        protected override IManagedEntryPoint? GetManagedEntryPoint()
        {
            if ((DotNetDirectory.Flags & DotNetDirectoryFlags.NativeEntryPoint) != 0)
            {
                // TODO: native entry points.
                return null;
            }

            if (DotNetDirectory.EntryPoint.IsManaged)
            {
                var token = DotNetDirectory.EntryPoint.MetadataToken;
                return !TryLookupMember<IManagedEntryPoint>(token, out var member)
                    ? ReaderContext.BadImageAndReturn<IManagedEntryPoint>("Module contained an invalid managed entry point metadata token.")
                    : member;
            }

            return null;
        }

        /// <inheritdoc />
        protected override ResourceDirectory? GetNativeResourceDirectory() => ReaderContext.Image.Resources;

        /// <inheritdoc />
        protected override IList<DebugDataEntry> GetDebugData() => new List<DebugDataEntry>(ReaderContext.Image.DebugData);

        protected override PortablePdb? GetPortablePdb() => PdbReaderContext?.Pdb;

        protected override IList<Document> GetDocuments()
        {
            var documentTable = PdbReaderContext?.TablesStream.GetTable<DocumentRow>();
            var count = documentTable?.Count ?? 0;
            var documents = new MemberCollection<ModuleDefinition, Document>(this, count);

            for (uint rid = 1; rid <= count; rid++)
            {
                if (PdbReaderContext!.Pdb.TryLookupMember<Document>(new MetadataToken(TableIndex.Document, rid), out var member))
                    documents.AddNoOwnerCheck(member);
            }

            return documents;
        }

        private AssemblyDefinition? FindParentAssembly()
        {
            var assemblyTable = ReaderContext.Streams.TablesStream!.GetTable<AssemblyDefinitionRow>(TableIndex.Assembly);

            if (assemblyTable.Count > 0)
            {
                return new SerializedAssemblyDefinition(
                    ReaderContext,
                    new MetadataToken(TableIndex.Assembly, 1),
                    assemblyTable[0],
                    this
                );
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
            foreach (var reference in AssemblyReferences)
            {
                if (reference.Name is not null && KnownCorLibs.KnownCorLibNames.Contains(reference.Name))
                {
                    if (mostRecentCorLib is null || reference.Version > mostRecentCorLib.GetAssembly()!.Version)
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
