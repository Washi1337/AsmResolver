using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.File;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Tls;

namespace AsmResolver.PE.Builder;

/// <summary>
/// Provides a mechanism for constructing PE files containing unmanaged code or metadata, based on a template PE file.
/// </summary>
/// <remarks>
/// <para>
/// This PE file builder attempts to preserve as much data as possible in the original executable file, and is well
/// suited for input binaries that contain unmanaged code and/or depend on raw offsets, RVAs or similar. However, it
/// may therefore not necessarily produce the most space-efficient output binaries, and may leave in original data
/// directories or sometimes entire PE sections that are no longer in use.
/// </para>
/// <para>
/// This class might modify the final imports directory (exposed by the <see cref="IPEImage.Imports"/> property),
/// as well as the base relocations directory (exposed by the <see cref="IPEImage.Relocations"/> property). In
/// particular, it might add or remove the entry to <c>mscoree.dll!_CorExeMain</c> or <c>mscoree.dll!_CorDllMain</c>,
/// and it may also add a reference to <c>kernel32.dll!VirtualProtect</c> in case dynamic initializers need to be
/// injected to initialize reconstructed some parts of the original import address tables (IATs).
/// </para>
/// <para>
/// By default, the Import Address Table (IATs) and .NET's VTable Fixup Table are not reconstructed and are preserved.
/// When changing any imports or VTable fixups in the PE image, set the appropriate <see cref="TrampolineImports"/> or
/// <see cref="TrampolineVTableFixups"/> properties to <c>true</c>. This will instruct the builder to patch the original
/// address tables and trampoline them to their new layout. This may also result in the builder injecting additional
/// initializer code to be inserted in <c>.auxtext</c>, depending on the type of imports that are present. This
/// initialization code is added as a TLS callback to the final PE file.
/// </para>
/// <para>
/// This class will add at most 5 new auxiliary sections to the final output PE file, next to the sections that were
/// already present in the input PE file, to fit in reconstructed data directories that did not fit in the original
/// location.
/// </para>
/// </remarks>
public class UnmanagedPEFileBuilder : PEFileBuilder<UnmanagedPEFileBuilder.BuilderContext>
{
    private static readonly IImportedSymbolClassifier DefaultSymbolClassifier =
        new DelegatedSymbolClassifier(_ => ImportedSymbolType.Function);

    /// <summary>
    /// Creates a new unmanaged PE file builder.
    /// </summary>
    public UnmanagedPEFileBuilder()
        : this(ThrowErrorListener.Instance, null)
    {
    }

    /// <summary>
    /// Creates a new unmanaged PE file builder using the provided error listener.
    /// </summary>
    /// <param name="errorListener">The error listener to use.</param>
    public UnmanagedPEFileBuilder(IErrorListener errorListener)
        : this(errorListener, null)
    {
    }

    /// <summary>
    /// Creates a new unmanaged PE file builder using the provided error listener and base PE file.
    /// </summary>
    /// <param name="errorListener">The error listener to use.</param>
    /// <param name="baseFile">The template file to base the resulting file on.</param>
    public UnmanagedPEFileBuilder(IErrorListener errorListener, PEFile? baseFile)
    {
        ErrorListener = errorListener;
        BaseFile = baseFile;
    }

    /// <summary>
    /// Gets or sets the service used for registering exceptions that occur during the construction of the PE.
    /// </summary>
    public IErrorListener ErrorListener
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the template file to base the resulting file on.
    /// </summary>
    public PEFile? BaseFile
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the imports directory of the new file is supposed to be reconstructed
    /// and that the old, existing imports directory should be patched and rewired via trampolines.
    /// </summary>
    public bool TrampolineImports
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the VTable fixups directory of the new file is supposed to be
    /// reconstructed and that the old, existing VTable fixups directory should be patched and rewired via trampolines.
    /// </summary>
    public bool TrampolineVTableFixups
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the service that is used for determining whether an imported symbol is a function or a data symbol.
    /// This is required when <see cref="TrampolineImports"/> is set to <c>true</c> and the IAT is reconstructed.
    /// </summary>
    public IImportedSymbolClassifier ImportedSymbolClassifier
    {
        get;
        set;
    } = DefaultSymbolClassifier;

    /// <inheritdoc />
    protected override BuilderContext CreateContext(IPEImage image)
    {
        var baseFile = BaseFile ?? image.PEFile;
        var baseImage = baseFile is not null
            ? PEImage.FromFile(baseFile, new PEReaderParameters(EmptyErrorListener.Instance))
            : null;

        return new BuilderContext(image, baseImage);
    }

    /// <inheritdoc />
    protected override IEnumerable<PESection> CreateSections(BuilderContext context)
    {
        var sections = new List<PESection>();

        // Import all existing sections.
        sections.AddRange(context.ClonedSections);

        // Add .auxtext section when necessary.
        if (CreateAuxTextSection(context) is { } auxTextSection)
            sections.Add(auxTextSection);

        // Add .rdata section when necessary.
        if (CreateAuxRDataSection(context) is { } auxRDataSection)
            sections.Add(auxRDataSection);

        // Add .auxdata section when necessary.
        if (CreateAuxDataSection(context) is { } auxDataSection)
            sections.Add(auxDataSection);

        // Add .auxrsrc section when necessary.
        if (CreateAuxRsrcSection(context) is { } auxRsrcSection)
            sections.Add(auxRsrcSection);

        // Add .reloc section when necessary.
        if (CreateAuxRelocSection(context) is { } auxRelocSection)
            sections.Add(auxRelocSection);

        return sections;
    }

    /// <inheritdoc />
    protected override uint GetEntryPointAddress(BuilderContext context, PEFile outputFile)
    {
        return context.Image.PEFile?.OptionalHeader.AddressOfEntryPoint
            ?? context.ClrBootstrapper?.Segment.Rva
            ?? 0;
    }

    /// <inheritdoc />
    protected override void AssignDataDirectories(BuilderContext context, PEFile outputFile)
    {
        var header = outputFile.OptionalHeader;

        // Import all existing data directories.
        if (context.Image.PEFile is { } originalFile)
        {
            header.DataDirectories.Clear();
            foreach (var directory in originalFile.OptionalHeader.DataDirectories)
                header.DataDirectories.Add(directory);
        }

        header.EnsureDataDirectoryCount(OptionalHeader.DefaultNumberOfRvasAndSizes);

        if (!context.ImportDirectory.IsEmpty)
        {
            header.SetDataDirectory(DataDirectoryIndex.ImportDirectory, context.ImportDirectory);
            header.SetDataDirectory(DataDirectoryIndex.IatDirectory, context.ImportDirectory.ImportAddressDirectory);
        }

        if (!context.ExportDirectory.IsEmpty)
            header.SetDataDirectory(DataDirectoryIndex.ExportDirectory, context.ExportDirectory);

        if (!context.DebugDirectory.IsEmpty)
            header.SetDataDirectory(DataDirectoryIndex.DebugDirectory, context.DebugDirectory);

        if (!context.ResourceDirectory.IsEmpty)
            header.SetDataDirectory(DataDirectoryIndex.ResourceDirectory, context.ResourceDirectory);

        if (context.Image.DotNetDirectory is not null)
            header.SetDataDirectory(DataDirectoryIndex.ClrDirectory, context.Image.DotNetDirectory);

        if (context.Image.TlsDirectory is not null)
            header.SetDataDirectory(DataDirectoryIndex.TlsDirectory, context.Image.TlsDirectory);

        if (!context.RelocationsDirectory.IsEmpty)
            header.SetDataDirectory(DataDirectoryIndex.BaseRelocationDirectory, context.RelocationsDirectory);
    }

    /// <summary>
    /// Builds up an auxiliary main text section (.auxtext) of the new .NET PE file when necessary.
    /// </summary>
    /// <param name="context">The working space of the builder.</param>
    /// <returns>The new .text section, or <c>null</c> if no auxiliary text section was required.</returns>
    protected virtual PESection? CreateAuxTextSection(BuilderContext context)
    {
        var contents = new SegmentBuilder();

        // Reconstructed IAT.
        if (TrampolineImports && !context.ImportDirectory.IsEmpty)
            contents.Add(context.ImportDirectory.ImportAddressDirectory);

        // .NET metadata etc.
        if (CreateAuxDotNetSegment(context) is { } auxSegment)
            contents.Add(auxSegment);

        // Reconstructed ILT.
        if (TrampolineImports && !context.ImportDirectory.IsEmpty)
            contents.Add(context.ImportDirectory);

        // Reconstructed exports.
        if (!context.ExportDirectory.IsEmpty)
            contents.Add(context.ExportDirectory);

        // Reconstructed debug directory.
        if (!context.DebugDirectory.IsEmpty)
        {
            if (!TryPatchDataDirectory(context, context.DebugDirectory, DataDirectoryIndex.DebugDirectory))
                contents.Add(context.DebugDirectory);

            contents.Add(context.DebugDirectory.ContentsTable);
        }

        // New CLR bootstrapper
        if (context.ClrBootstrapper.HasValue)
            contents.Add(context.ClrBootstrapper.Value.Segment);

        // Import trampolines.
        if (TrampolineImports)
        {
            contents.Add(context.ImportTrampolines);
            context.ImportTrampolines.ApplyPatches(context.ClonedSections);
        }

        // Code for newly added exports.
        if (context.Image.Exports is { Entries: { Count: > 0 } entries })
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var export = entries[i];
                if (export.Address.IsBounded && export.Address.GetSegment() is { } segment)
                    contents.Add(segment, (uint) context.Platform.PointerSize);
            }
        }

        if (contents.Count == 0)
            return null;

        return new PESection(
            ".auxtext",
            SectionFlags.ContentCode | SectionFlags.MemoryExecute | SectionFlags.MemoryRead,
            contents
        );
    }

    private ISegment? CreateAuxDotNetSegment(BuilderContext context)
    {
        var dotNetDirectory = context.Image.DotNetDirectory;
        if (dotNetDirectory is null)
            return null;

        var result = new SegmentBuilder();

        // IMAGE_COR20_HEADER
        AddOrPatch(dotNetDirectory, context.BaseImage?.DotNetDirectory);

        // Field RVA data.
        if (!context.FieldRvaTable.IsEmpty)
            result.Add(context.FieldRvaTable);

        // Managed and new unmanaged method bodies.
        if (!context.MethodBodyTable.IsEmpty)
            result.Add(context.MethodBodyTable);

        // All .NET metadata.
        AddOrPatch(dotNetDirectory.Metadata, context.BaseImage?.DotNetDirectory?.Metadata);
        AddOrPatch(dotNetDirectory.DotNetResources, context.BaseImage?.DotNetDirectory?.DotNetResources);
        AddOrPatch(dotNetDirectory.StrongName, context.BaseImage?.DotNetDirectory?.StrongName);

        // VTable fixup tables.
        if (TrampolineVTableFixups && dotNetDirectory.VTableFixups?.Count > 0)
        {
            // NOTE: We cannot safely patch the existing tables here as we need to trampoline the slots.
            result.Add(dotNetDirectory.VTableFixups);
            result.Add(context.VTableTrampolines);
            context.VTableTrampolines.ApplyPatches(context.ClonedSections);
        }

        // Other managed native headers.
        AddOrPatch(dotNetDirectory.ExportAddressTable, context.BaseImage?.DotNetDirectory?.ExportAddressTable);
        AddOrPatch(dotNetDirectory.ManagedNativeHeader, context.BaseImage?.DotNetDirectory?.ManagedNativeHeader);

        return result.Count > 0
            ? result
            : null;

        void AddOrPatch(ISegment? segment, ISegment? originalSegment)
        {
            if (segment is not null && !TryPatchSegment(context, segment, originalSegment))
                result.Add(segment, (uint) context.Platform.PointerSize);
        }
    }

    /// <summary>
    /// Creates an auxiliary .rdata section containing a TLS data directory, its template data and callback function
    /// table (when present in the image).
    /// </summary>
    /// <param name="context">The working space of the builder.</param>
    /// <returns>The new .rdata section.</returns>
    protected virtual PESection? CreateAuxRDataSection(BuilderContext context)
    {
        var image = context.Image;
        var contents = new SegmentBuilder();

        // Add TLS data directory and contents.
        if (image.TlsDirectory is { } directory)
        {
            var originalDirectory = context.BaseImage?.TlsDirectory;
            AddOrPatch(directory, originalDirectory);

            if (directory.TemplateData is not null)
                AddOrPatch(directory.TemplateData, originalDirectory?.TemplateData);
            if (directory.CallbackFunctions.Count > 0)
                AddOrPatch(directory.CallbackFunctions, originalDirectory?.CallbackFunctions);
        }

        if (contents.Count == 0)
            return null;

        return new PESection(
            ".rdata",
            SectionFlags.MemoryRead | SectionFlags.ContentInitializedData,
            contents
        );

        void AddOrPatch(ISegment? newSegment, ISegment? originalSegment)
        {
            if (newSegment is null)
                return;

            if (originalSegment is null || !TryPatchSegment(context, originalSegment, newSegment))
                contents.Add(newSegment, (uint) context.Platform.PointerSize);
        }
    }

    /// <summary>
    /// Creates the .sdata section containing the exports, vtable fixup tokens, and the TLS index.
    /// </summary>
    /// <param name="context">The working space of the builder.</param>
    /// <returns>The section.</returns>
    protected virtual PESection? CreateAuxDataSection(BuilderContext context)
    {
        var image = context.Image;
        var contents = new SegmentBuilder();

        // Add VTable fixups
        if (TrampolineVTableFixups && image.DotNetDirectory?.VTableFixups is { } fixups)
        {
            for (int i = 0; i < fixups.Count; i++)
                contents.Add(fixups[i].Tokens, (uint) context.Platform.PointerSize);
        }

        // Add export directory.
        if (image.Exports is { Entries.Count: > 0 })
            contents.Add(context.ExportDirectory, (uint) context.Platform.PointerSize);

        if (image.TlsDirectory is { } directory)
        {
            // Add TLS index segment.
            if (directory.Index is not PESegmentReference
                && directory.Index.IsBounded
                && directory.Index.GetSegment() is { } indexSegment)
            {
                if (context.BaseImage?.TlsDirectory?.Index is not { } index
                    || !TryPatchSegment(context, indexSegment, index.Rva, sizeof(uint)))
                {
                    contents.Add(indexSegment);
                }
            }
        }

        if (contents.Count == 0)
            return null;

        return new PESection(
            ".auxdata",
            SectionFlags.MemoryRead | SectionFlags.MemoryWrite | SectionFlags.ContentInitializedData,
            contents
        );
    }

    /// <summary>
    /// Creates the win32 resources section (.rsrc) of the new .NET PE file.
    /// </summary>
    /// <param name="context">The working space of the builder.</param>
    /// <returns>The resources section.</returns>
    protected virtual PESection? CreateAuxRsrcSection(BuilderContext context)
    {
        // Do we have any resources to add?
        if (context.ResourceDirectory.IsEmpty)
            return null;

        // Try fitting the data in the original data directory.
        if (TryPatchDataDirectory(context, context.ResourceDirectory, DataDirectoryIndex.ResourceDirectory))
            return null;

        // Otherwise, create a new section.
        return new PESection(
            ".auxrsrc",
            SectionFlags.MemoryRead | SectionFlags.ContentInitializedData,
            context.ResourceDirectory
        );
    }

    /// <summary>
    /// Creates the base relocations section (.reloc) of the new .NET PE file.
    /// </summary>
    /// <param name="context">The working space of the builder.</param>
    /// <returns>The base relocations section.</returns>
    protected virtual PESection? CreateAuxRelocSection(BuilderContext context)
    {
        // Do we have any base relocations to add?
        if (context.RelocationsDirectory.IsEmpty)
            return null;

        // Try fitting the data in the original data directory.
        if (TryPatchDataDirectory(context, context.RelocationsDirectory, DataDirectoryIndex.BaseRelocationDirectory))
            return null;

        return new PESection(
            ".reloc",
            SectionFlags.MemoryRead | SectionFlags.ContentInitializedData | SectionFlags.MemoryDiscardable,
            context.RelocationsDirectory
        );
    }

    /// <inheritdoc />
    protected override void CreateDataDirectoryBuffers(BuilderContext context)
    {
        CreateDotNetDirectories(context);
        base.CreateDataDirectoryBuffers(context);
    }

    /// <inheritdoc />
    protected override void CreateImportDirectory(BuilderContext context)
    {
        if (!TrampolineImports
            || context.BaseImage?.Imports is not { } baseDirectory
            || context.Image.Imports is not { } newDirectory)
        {
            return;
        }

        // Try to map all symbols stored in the original directory to symbols in the new directory.
        foreach (var module in baseDirectory)
        {
            foreach (var original in module.Symbols)
            {
                if (FindNewImportedSymbol(original) is not { } newSymbol)
                    continue;

                switch (ImportedSymbolClassifier.Classify(newSymbol))
                {
                    case ImportedSymbolType.Function:
                        context.ImportTrampolines.AddFunctionTableSlotTrampoline(original, newSymbol);
                        break;

                    case ImportedSymbolType.Data:
                        context.ImportTrampolines.VirtualProtect ??= GetOrCreateVirtualProtect();
                        context.ImportTrampolines.AddDataSlotInitializer(original, newSymbol);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        // If we have a data slot initializer, then inject it as a TLS callback to ensure the original IAT data slots
        // are initialized before any other user-code is called.
        if (context.ImportTrampolines.DataInitializerSymbol is { } initializerSymbol)
        {
            var tls = context.Image.TlsDirectory ??= new TlsDirectory();
            tls.UpdateOffsets(new RelocationParameters(0, 0, 0, context.Platform.Is32Bit));

            if (tls.Index == SegmentReference.Null)
                tls.Index = new ZeroesSegment(sizeof(ulong)).ToReference();

            tls.CallbackFunctions.Insert(0, initializerSymbol.GetReference()!);
        }

        // Rebuild import directory as normal.
        base.CreateImportDirectory(context);
        return;

        ImportedSymbol? FindNewImportedSymbol(ImportedSymbol symbol)
        {
            // We consider a slot to be equal if the tokens match.
            foreach (var newModule in newDirectory)
            {
                if (symbol.DeclaringModule!.Name != newModule.Name)
                    continue;

                foreach (var newSymbol in newModule.Symbols)
                {
                    if (symbol.IsImportByName && newSymbol.IsImportByName && newSymbol.Name == symbol.Name)
                        return newSymbol;
                    if (symbol.IsImportByOrdinal && newSymbol.IsImportByOrdinal && newSymbol.Ordinal == symbol.Ordinal)
                        return newSymbol;
                }
            }

            return null;
        }

        ImportedSymbol GetOrCreateVirtualProtect()
        {
            bool addModule = false;

            // Find or create a new kernel32.dll module import.
            var kernel32 = context.Image.Imports.FirstOrDefault(
                x => x.Name?.Equals("kernel32.dll", StringComparison.OrdinalIgnoreCase) ?? false);
            if (kernel32 is null)
            {
                kernel32 = new ImportedModule("KERNEL32.dll");
                addModule = true;
            }

            // Find or create a new VirtualProtect module import.
            var virtualProtect = kernel32.Symbols.FirstOrDefault(x => x.Name == "VirtualProtect");
            if (virtualProtect is null)
            {
                virtualProtect = new ImportedSymbol(0, "VirtualProtect");
                kernel32.Symbols.Add(virtualProtect);
            }

            // Add the new module if kernel32.dll wasn't added yet.
            if (addModule)
                context.ImportDirectory.AddModule(kernel32);

            return virtualProtect;
        }
    }

    /// <inheritdoc />
    protected override void CreateRelocationsDirectory(BuilderContext context)
    {
        base.CreateRelocationsDirectory(context);

        // We may have some extra relocations for the newly generated code of our trampolines and TLS-backed initializers.
        foreach (var reloc in context.ImportTrampolines.GetRequiredBaseRelocations())
            context.RelocationsDirectory.Add(reloc);

        foreach (var reloc in context.VTableTrampolines.GetRequiredBaseRelocations())
            context.RelocationsDirectory.Add(reloc);

        if (context.Image.TlsDirectory is { } tls)
        {
            foreach (var reloc in tls.GetRequiredBaseRelocations())
                context.RelocationsDirectory.Add(reloc);
        }
    }

    private void CreateDotNetDirectories(BuilderContext context)
    {
        ProcessRvasInMetadataTables(context);
        AddVTableFixupTrampolines(context);
    }

    private void ProcessRvasInMetadataTables(BuilderContext context)
    {
        var tablesStream = context.Image.DotNetDirectory?.Metadata?.GetStream<TablesStream>();
        if (tablesStream is null)
            return;

        AddMethodBodiesToTable(context, tablesStream);
        AddFieldRvasToTable(context, tablesStream);
    }

    private void AddMethodBodiesToTable(BuilderContext context, TablesStream tablesStream)
    {
        var methodTable = tablesStream.GetTable<MethodDefinitionRow>();
        for (uint rid = 1; rid <= methodTable.Count; rid++)
        {
            ref var methodRow = ref methodTable.GetRowRef(rid);

            var bodySegment = GetMethodBodySegment(methodRow);
            if (bodySegment is CilRawMethodBody cilBody)
                context.MethodBodyTable.AddCilBody(cilBody); // TODO: try reuse CIL bodies when possible.
            else if (bodySegment is not null)
                context.MethodBodyTable.AddNativeBody(bodySegment, (uint)context.Platform.PointerSize);
            else
                continue;

            methodRow.Body = bodySegment.ToReference();
        }
    }

    private static ISegment? GetMethodBodySegment(MethodDefinitionRow methodRow)
    {
        var body = methodRow.Body;

        // If method body is well-defined, use the existing segment.
        if (body.IsBounded)
            return body.GetSegment();

        // IL method bodies are parseable, reconstruct the segment on-the-fly.
        if (body.CanRead && (methodRow.ImplAttributes & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.IL)
        {
            var reader = body.CreateReader();
            return CilRawMethodBody.FromReader(ThrowErrorListener.Instance, ref reader);
        }

        // Otherwise, assume it is an entry point of existing native code that we need to preserve.
        return null;
    }

    private void AddFieldRvasToTable(BuilderContext context, TablesStream tablesStream)
    {
        var directory = context.Image.DotNetDirectory!;

        var fieldRvaTable = tablesStream.GetTable<FieldRvaRow>(TableIndex.FieldRva);
        if (fieldRvaTable.Count == 0)
            return;

        var table = context.FieldRvaTable;
        var reader = context.FieldRvaDataReader;

        for (uint rid = 1; rid <= fieldRvaTable.Count; rid++)
        {
            ref var row = ref fieldRvaTable.GetRowRef(rid);

            // Preserve existing RVAs.
            if (row.Data is PESegmentReference)
                continue;

            var data = reader.ResolveFieldData(
                ErrorListener,
                context.Platform,
                directory,
                row
            );

            if (data is null)
                continue;

            table.Add(data);
            row.Data = data.ToReference();
        }
    }

    private void AddVTableFixupTrampolines(BuilderContext context)
    {
        if (!TrampolineVTableFixups
            || context.BaseImage?.DotNetDirectory?.VTableFixups is not { } baseDirectory
            || context.Image.DotNetDirectory?.VTableFixups is not { } newDirectory)
        {
            return;
        }

        // Try to map all tokens stored in the original directory to slots in the new directory.
        foreach (var originalFixup in baseDirectory)
        {
            for (int i = 0; i < originalFixup.Tokens.Count; i++)
            {
                if (FindNewTokenSlot(originalFixup.Tokens[i]) is not { } newSlot)
                    continue;

                var originalSlot = new Symbol(originalFixup.Tokens.GetReferenceToIndex(i));
                context.VTableTrampolines.AddFunctionTableSlotTrampoline(originalSlot, newSlot);
            }
        }

        return;

        ISymbol? FindNewTokenSlot(MetadataToken token)
        {
            // We consider a slot to be equal if the tokens match.
            foreach (var fixup in newDirectory)
            {
                for (int j = 0; j < fixup.Tokens.Count; j++)
                {
                    if (fixup.Tokens[j] == token)
                        return new Symbol(fixup.Tokens.GetReferenceToIndex(j));
                }
            }

            return null;
        }
    }

    private static bool TryPatchDataDirectory(BuilderContext context, ISegment? segment, DataDirectoryIndex directoryIndex)
    {
        if (segment is null || context.BaseImage?.PEFile is not { } peFile)
            return false;

        var directory = peFile.OptionalHeader.GetDataDirectory(directoryIndex);
        return TryPatchSegment(context, segment, directory.VirtualAddress, directory.Size);
    }

    private static bool TryPatchSegment(BuilderContext context, ISegment? segment, ISegment? originalSegment)
    {
        if (segment is null || originalSegment is null)
            return false;

        return TryPatchSegment(context, segment, originalSegment.Rva, originalSegment.GetPhysicalSize());
    }

    private static bool TryPatchSegment(BuilderContext context, ISegment? segment, uint rva, uint size)
    {
        if (segment is null || context.BaseImage?.PEFile is not { } peFile)
            return false;

        // Before we can measure size, we need to update offsets.
        segment.UpdateOffsets(new RelocationParameters(
            context.Image.ImageBase,
            peFile.RvaToFileOffset(rva),
            rva,
            context.Platform.Is32Bit
        ));

        // Do we fit in the existing segment?
        if (segment.GetPhysicalSize() <= size
            && context.TryGetSectionContainingRva(rva, out var section)
            && section.Contents is not null)
        {
            uint relativeOffset = rva - section.Rva;
            section.Contents = section.Contents.AsPatchedSegment().Patch(relativeOffset, segment);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Provides a workspace for <see cref="UnmanagedPEFileBuilder"/>.
    /// </summary>
    public class BuilderContext : PEFileBuilderContext
    {
        /// <summary>
        /// Creates a new builder context.
        /// </summary>
        /// <param name="image">The image to build a PE file for.</param>
        /// <param name="baseImage">The template image to base the file on.</param>
        public BuilderContext(IPEImage image, IPEImage? baseImage)
            : base(image)
        {
            BaseImage = baseImage;

            if (baseImage?.PEFile is not null)
            {
                foreach (var section in baseImage.PEFile.Sections)
                    ClonedSections.Add(new PESection(section));
            }

            ImportTrampolines = new TrampolineTableBuffer(Platform);
            VTableTrampolines = new TrampolineTableBuffer(Platform);
        }

        /// <summary>
        /// Gets a collection of sections that were cloned from the template file.
        /// </summary>
        public List<PESection> ClonedSections { get; } = new();

        /// <summary>
        /// Gets the template image to base the file on.
        /// </summary>
        public IPEImage? BaseImage { get; }

        /// <summary>
        /// Gets the trampolines table for all imported symbols.
        /// </summary>
        public TrampolineTableBuffer ImportTrampolines { get; }

        /// <summary>
        /// Gets the trampolines table for all fixed up managed method addresses.
        /// </summary>
        public TrampolineTableBuffer VTableTrampolines { get; }

        /// <summary>
        /// Searches for a cloned section containing the provided RVA.
        /// </summary>
        /// <param name="rva">The RVA.</param>
        /// <param name="section">The section.</param>
        /// <returns><c>true</c> if the section was found, <c>false</c> otherwise.</returns>
        public bool TryGetSectionContainingRva(uint rva, [NotNullWhen(true)] out PESection? section)
        {
            foreach (var candidate in ClonedSections)
            {
                if (candidate.ContainsRva(rva))
                {
                    section = candidate;
                    return true;
                }
            }

            section = null;
            return false;
        }
    }
}
