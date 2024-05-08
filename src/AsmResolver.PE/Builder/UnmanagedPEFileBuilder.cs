using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Builder;

public class UnmanagedPEFileBuilder : PEFileBuilder<UnmanagedPEFileBuilder.BuilderContext>
{
    public UnmanagedPEFileBuilder()
        : this(ThrowErrorListener.Instance, null)
    {
    }

    public UnmanagedPEFileBuilder(IErrorListener errorListener)
        : this(errorListener, null)
    {
    }

    public UnmanagedPEFileBuilder(IErrorListener errorListener, IPEFile? baseFile)
    {
        ErrorListener = errorListener;
        BaseFile = baseFile;
    }

    public IErrorListener ErrorListener
    {
        get;
        set;
    }

    public IPEFile? BaseFile
    {
        get;
        set;
    }

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
        foreach (var section in context.ClonedSections)
            sections.Add(section);

        // Add .auxtext section when necessary.
        if (CreateAuxTextSection(context) is { } auxTextSection)
            sections.Add(auxTextSection);

        // Add .sdata section when necessary.
        if (CreateAuxSDataSection(context) is { } auxSDataSection)
            sections.Add(auxSDataSection);

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
    protected override void CreateImportDirectory(BuilderContext context)
    {
        // TODO: rewire import table.
        // base.CreateImportDirectory(context);
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

        //if (!context.ImportDirectory.IsEmpty)
        //{
        //    header.SetDataDirectory(DataDirectoryIndex.ImportDirectory, context.ImportDirectory);
        //    header.SetDataDirectory(DataDirectoryIndex.IatDirectory, context.ImportDirectory.ImportAddressDirectory);
        //}

        if (!context.ExportDirectory.IsEmpty)
            header.SetDataDirectory(DataDirectoryIndex.ExportDirectory, context.ExportDirectory);

        if (!context.DebugDirectory.IsEmpty)
            header.SetDataDirectory(DataDirectoryIndex.DebugDirectory, context.DebugDirectory);

        if (!context.ResourceDirectory.IsEmpty)
            header.SetDataDirectory(DataDirectoryIndex.ResourceDirectory, context.ResourceDirectory);

        if (context.Image.DotNetDirectory is not null)
            header.SetDataDirectory(DataDirectoryIndex.ClrDirectory, context.Image.DotNetDirectory);

        if (!context.RelocationsDirectory.IsEmpty)
            header.SetDataDirectory(DataDirectoryIndex.BaseRelocationDirectory, context.RelocationsDirectory);
    }

    /// <summary>
    /// Builds up the main text section (.text) of the new .NET PE file.
    /// </summary>
    /// <param name="context">The working space of the builder.</param>
    /// <returns>The .text section.</returns>
    protected virtual PESection? CreateAuxTextSection(BuilderContext context)
    {
        var contents = new SegmentBuilder();

        //if (!context.ImportDirectory.IsEmpty)
        //    contents.Add(context.ImportDirectory.ImportAddressDirectory);

        if (CreateAuxDotNetSegment(context) is { } auxSegment)
            contents.Add(auxSegment);

        //if (!context.ImportDirectory.IsEmpty)
        //    contents.Add(context.ImportDirectory);

        if (!context.ExportDirectory.IsEmpty)
            contents.Add(context.ExportDirectory);

        if (!context.DebugDirectory.IsEmpty)
        {
            if (!TryPatchDataDirectory(context, context.DebugDirectory, DataDirectoryIndex.DebugDirectory))
                contents.Add(context.DebugDirectory);

            contents.Add(context.DebugDirectory.ContentsTable);
        }

        if (context.ClrBootstrapper.HasValue)
            contents.Add(context.ClrBootstrapper.Value.Segment);

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
        var dotNetDirectory = context.Image.DotNetDirectory!;

        var result = new SegmentBuilder();

        AddOrPatch(dotNetDirectory, context.BaseImage?.DotNetDirectory);

        if (!context.FieldRvaTable.IsEmpty)
            result.Add(context.FieldRvaTable);

        if (!context.MethodBodyTable.IsEmpty)
            result.Add(context.MethodBodyTable);
        
        AddOrPatch(dotNetDirectory.Metadata, context.BaseImage?.DotNetDirectory?.Metadata);
        AddOrPatch(dotNetDirectory.DotNetResources, context.BaseImage?.DotNetDirectory?.DotNetResources);
        AddOrPatch(dotNetDirectory.StrongName, context.BaseImage?.DotNetDirectory?.StrongName);

        //if (dotNetDirectory.VTableFixups?.Count > 0)
        //    result.Add(dotNetDirectory.VTableFixups);

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
    /// Creates the .sdata section containing the exports and vtables directory of the new .NET PE file.
    /// </summary>
    /// <param name="context">The working space of the builder.</param>
    /// <returns>The section.</returns>
    protected virtual PESection? CreateAuxSDataSection(BuilderContext context)
    {
        var image = context.Image;
        var contents = new SegmentBuilder();

        //if (image.DotNetDirectory?.VTableFixups is { } fixups)
        //{
        //    for (int i = 0; i < fixups.Count; i++)
        //        contents.Add(fixups[i].Tokens, (uint) context.Platform.PointerSize);
        //}

        if (image.Exports is { Entries.Count: > 0 })
            contents.Add(context.ExportDirectory, (uint) context.Platform.PointerSize);

        if (contents.Count == 0)
            return null;

        return new PESection(
            ".sdata",
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

    private void CreateDotNetDirectories(BuilderContext context) => ProcessRvasInMetadataTables(context);

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

    private bool TryPatchDataDirectory(BuilderContext context, ISegment? segment, DataDirectoryIndex directoryIndex)
    {
        if (segment is null || context.BaseImage?.PEFile is not { } peFile)
            return false;

        var directory = peFile.OptionalHeader.GetDataDirectory(directoryIndex);
        return TryPatchSegment(context, segment, directory.VirtualAddress, directory.Size);
    }

    private bool TryPatchSegment(BuilderContext context, ISegment? segment, ISegment? originalSegment)
    {
        if (segment is null || originalSegment is null)
            return false;

        return TryPatchSegment(context, segment, originalSegment.Rva, originalSegment.GetPhysicalSize());
    }

    private bool TryPatchSegment(BuilderContext context, ISegment? segment, uint rva, uint size)
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

    public class BuilderContext : PEFileBuilderContext
    {
        public BuilderContext(IPEImage image, IPEImage? baseImage)
            : base(image)
        {
            BaseImage = baseImage;

            if (baseImage?.PEFile is not null)
            {
                foreach (var section in baseImage.PEFile.Sections)
                    ClonedSections.Add(new PESection(section));
            }
        }

        public List<PESection> ClonedSections { get; } = new();

        public IPEImage? BaseImage { get; }

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
