using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Relocations;

namespace AsmResolver.PE.Builder;

public class UnmanagedPEFileBuilder : PEFileBuilder
{
    public UnmanagedPEFileBuilder()
        : this(ThrowErrorListener.Instance)
    {
    }

    public UnmanagedPEFileBuilder(IErrorListener errorListener)
    {
        ErrorListener = errorListener;
    }

    public IErrorListener ErrorListener
    {
        get;
        set;
    }

    /// <inheritdoc />
    protected override IEnumerable<PESection> CreateSections(PEFileBuilderContext context)
    {
        var image = context.Image;
        var sections = new List<PESection>();

        // Import all existing sections.
        if (context.Image.PEFile is { } originalFile)
        {
            foreach (var section in originalFile.Sections)
                sections.Add(new PESection(section));
        }

        // Always create .auxtext section.
        sections.Add(CreateAuxTextSection(context));

        // Add .sdata section when necessary.
        if (!context.ExportDirectory.IsEmpty || image.DotNetDirectory?.VTableFixups is not null)
            sections.Add(CreateSDataSection(context));

        // Add .rsrc section when necessary.
        if (!context.ResourceDirectory.IsEmpty)
            sections.Add(CreateRsrcSection(context));

        // Add .reloc section when necessary.
        if (!context.RelocationsDirectory.IsEmpty)
            sections.Add(CreateRelocSection(context));

        return sections;
    }

    /// <inheritdoc />
    protected override uint GetEntryPointAddress(PEFileBuilderContext context, PEFile outputFile)
    {
        return context.Image.PEFile?.OptionalHeader.AddressOfEntryPoint
            ?? context.ClrBootstrapper?.Segment.Rva
            ?? 0;
    }

    /// <inheritdoc />
    protected override void AssignDataDirectories(PEFileBuilderContext context, PEFile outputFile)
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

        if (!context.RelocationsDirectory.IsEmpty)
            header.SetDataDirectory(DataDirectoryIndex.BaseRelocationDirectory, context.RelocationsDirectory);
    }

    /// <summary>
    /// Builds up the main text section (.text) of the new .NET PE file.
    /// </summary>
    /// <param name="context">The working space of the builder.</param>
    /// <returns>The .text section.</returns>
    protected virtual PESection CreateAuxTextSection(PEFileBuilderContext context)
    {
        var contents = new SegmentBuilder();

        if (!context.ImportDirectory.IsEmpty)
            contents.Add(context.ImportDirectory.ImportAddressDirectory);

        contents.Add(CreateAuxDotNetSegment(context));

        if (!context.ImportDirectory.IsEmpty)
            contents.Add(context.ImportDirectory);

        if (!context.ExportDirectory.IsEmpty)
            contents.Add(context.ExportDirectory);

        if (!context.DebugDirectory.IsEmpty)
        {
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

        return new PESection(
            ".auxtext",
            SectionFlags.ContentCode | SectionFlags.MemoryExecute | SectionFlags.MemoryRead,
            contents
        );
    }

    private static ISegment CreateAuxDotNetSegment(PEFileBuilderContext context)
    {
        var dotNetDirectory = context.Image.DotNetDirectory!;

        var result = new SegmentBuilder
        {
            dotNetDirectory,
            context.FieldRvaTable,
            context.MethodBodyTable,
        };

        AddOrPatchIfPresent(dotNetDirectory.Metadata);
        AddOrPatchIfPresent(dotNetDirectory.DotNetResources);
        AddOrPatchIfPresent(dotNetDirectory.StrongName);

        if (dotNetDirectory.VTableFixups?.Count > 0)
            result.Add(dotNetDirectory.VTableFixups);

        AddOrPatchIfPresent(dotNetDirectory.ExportAddressTable);
        AddOrPatchIfPresent(dotNetDirectory.ManagedNativeHeader);

        return result;

        void AddOrPatchIfPresent(ISegment? segment)
        {
            if (segment is null)
                return;

            // TODO: Try reuse the segment in the original PE.

            result.Add(segment, 4);
        }
    }

    /// <summary>
    /// Creates the .sdata section containing the exports and vtables directory of the new .NET PE file.
    /// </summary>
    /// <param name="context">The working space of the builder.</param>
    /// <returns>The section.</returns>
    protected virtual PESection CreateSDataSection(PEFileBuilderContext context)
    {
        var image = context.Image;
        var contents = new SegmentBuilder();

        if (image.DotNetDirectory?.VTableFixups is { } fixups)
        {
            for (int i = 0; i < fixups.Count; i++)
                contents.Add(fixups[i].Tokens, (uint) context.Platform.PointerSize);
        }

        if (image.Exports is { Entries.Count: > 0 })
            contents.Add(context.ExportDirectory, (uint) context.Platform.PointerSize);

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
    protected virtual PESection CreateRsrcSection(PEFileBuilderContext context)
    {
        return new PESection(
            ".rsrc",
            SectionFlags.MemoryRead | SectionFlags.ContentInitializedData,
            context.ResourceDirectory
        );
    }

    /// <summary>
    /// Creates the base relocations section (.reloc) of the new .NET PE file.
    /// </summary>
    /// <param name="context">The working space of the builder.</param>
    /// <returns>The base relocations section.</returns>
    protected virtual PESection CreateRelocSection(PEFileBuilderContext context)
    {
        return new PESection(
            ".reloc",
            SectionFlags.MemoryRead | SectionFlags.ContentInitializedData,
            context.RelocationsDirectory
        );
    }

    /// <inheritdoc />
    protected override void CreateDataDirectoryBuffers(PEFileBuilderContext context)
    {
        CreateDotNetDirectories(context);
        base.CreateDataDirectoryBuffers(context);
    }

    /// <inheritdoc />
    protected override void CreateRelocationsDirectory(PEFileBuilderContext context)
    {
        // Since the PE is rebuild in its entirety, all relocations that were originally in the PE are invalidated.
        // Therefore, we filter out all relocations that were added by the reader.
        AddRange(context.Image.Relocations.Where(x => x.Location is not PESegmentReference));

        // Add relocations of the bootstrapper stub if necessary.
        if (context.ClrBootstrapper is { } bootstrapper)
            AddRange(bootstrapper.Relocations);

        return;

        void AddRange(IEnumerable<BaseRelocation> relocations)
        {
            foreach (var relocation in relocations)
                context.RelocationsDirectory.Add(relocation);
        }
    }

    private void CreateDotNetDirectories(PEFileBuilderContext context) => ProcessRvasInMetadataTables(context);

    private void ProcessRvasInMetadataTables(PEFileBuilderContext context)
    {
        var tablesStream = context.Image.DotNetDirectory?.Metadata?.GetStream<TablesStream>();
        if (tablesStream is null)
        {
            ErrorListener.RegisterException(new ArgumentException("Image does not have a .NET metadata tables stream."));
            return;
        }

        AddMethodBodiesToTable(context, tablesStream);
        AddFieldRvasToTable(context, tablesStream);
    }

    private static void AddMethodBodiesToTable(PEFileBuilderContext context, TablesStream tablesStream)
    {
        var methodTable = tablesStream.GetTable<MethodDefinitionRow>();
        for (uint rid = 1; rid <= methodTable.Count; rid++)
        {
            ref var methodRow = ref methodTable.GetRowRef(rid);

            var bodySegment = GetMethodBodySegment(methodRow);
            if (bodySegment is CilRawMethodBody cilBody)
                context.MethodBodyTable.AddCilBody(cilBody);
            else if (bodySegment is not null)
                context.MethodBodyTable.AddNativeBody(bodySegment, (uint) context.Platform.PointerSize);
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

        // Otherwise, assume it is an entry point of existing native code.
        return null;
    }

    private void AddFieldRvasToTable(PEFileBuilderContext context, TablesStream tablesStream)
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
}
