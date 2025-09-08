using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.File;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Relocations;

namespace AsmResolver.PE.Builder;

/// <summary>
/// Provides a mechanism for constructing PE files from images containing .NET metadata.
/// </summary>
/// <remarks>
/// <para>
/// This PE builder is focused on .NET images only, and assumes that every input PE image is either a fully .NET
/// image with only architecture independent code (CIL), or contains native methods that are fully well-defined,
/// i.e. they are represented by a single segment. Any method definition in the metadata table that references a
/// native method body of which the size is not explicitly defined will cause an exception. This class also might
/// replace rows in the method and/or field RVA metadata tables with new ones containing the updated references to
/// method bodies and/or field data. All remaining metadata in the tables stream and in the other metadata streams
/// is written as-is without any change or verification.
/// </para>
/// <para>
/// This class might modify the final imports directory (exposed by the <see cref="PEImage.Imports"/> property),
/// as well as the base relocations directory (exposed by the <see cref="PEImage.Relocations"/> property). In
/// particular, it might add or remove the entry to <c>mscoree.dll!_CorExeMain</c> or <c>mscoree.dll!_CorDllMain</c>,
/// depending on the machine type specified by the <see cref="PEImage.MachineType"/> property.
/// </para>
/// <para>
/// This class builds up at most four PE sections; <c>.text</c>, <c>.sdata</c>, <c>.rsrc</c> and <c>.reloc</c>,
/// similar to what a normal .NET language compiler would emit. Almost everything is put into the .text section,
/// including the import and debug directories. The win32 resources are put into <c>.rsrc</c> section, and this
/// section will only be added if there is at least one entry in the root resource directory of the
/// <see cref="PEImage.Resources"/> property. Similarly, the <c>.sdata</c> section is only added if at least
/// one unmanaged export is added to the PE image. Finally, the <c>.reloc</c> section is only added if at least
/// one base relocation was put into the directory, or when the CLR bootstrapper requires one.
/// </para>
/// </remarks>
public class ManagedPEFileBuilder : PEFileBuilder
{
    /// <summary>
    /// Creates a new managed PE file builder with default settings.
    /// </summary>
    public ManagedPEFileBuilder()
        : this(ThrowErrorListener.Instance)
    {
    }

    /// <summary>
    /// Creates a new managed PE file builder with the provided error listener.
    /// </summary>
    public ManagedPEFileBuilder(IErrorListener errorListener)
    {
        ErrorListener = errorListener;
    }

    /// <summary>
    /// Gets or sets the object responsible for recording diagnostic information during the building process.
    /// </summary>
    public IErrorListener ErrorListener
    {
        get;
        set;
    }

    /// <inheritdoc />
    protected override IEnumerable<PESection> CreateSections(PEFileBuilderContext context)
    {
        var image = context.Image;

        if (image.DotNetDirectory is null)
            ErrorListener.RegisterException(new ArgumentException("PE image does not contain a valid .NET data directory."));

        var sections = new List<PESection>();

        // Always create .text section.
        sections.Add(CreateTextSection(context));

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

    /// <summary>
    /// Builds up the main text section (.text) of the new .NET PE file.
    /// </summary>
    /// <param name="context">The working space of the builder.</param>
    /// <returns>The .text section.</returns>
    protected virtual PESection CreateTextSection(PEFileBuilderContext context)
    {
        var contents = new SegmentBuilder();

        if (!context.ImportDirectory.IsEmpty)
            contents.Add(context.ImportDirectory.ImportAddressDirectory);

        contents.Add(CreateDotNetSegment(context));

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
            contents.Add(context.ClrBootstrapper.Value.Segment, context.Platform.ThunkStubAlignment);

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
            ".text",
            SectionFlags.ContentCode | SectionFlags.MemoryExecute | SectionFlags.MemoryRead,
            contents
        );
    }

    private static ISegment CreateDotNetSegment(PEFileBuilderContext context)
    {
        var dotNetDirectory = context.Image.DotNetDirectory!;

        var result = new SegmentBuilder
        {
            dotNetDirectory,
            context.FieldRvaTable,
            context.MethodBodyTable,
        };

        AddIfPresent(dotNetDirectory.Metadata);
        AddIfPresent(dotNetDirectory.DotNetResources);
        AddIfPresent(dotNetDirectory.StrongName);

        if (dotNetDirectory.VTableFixups?.Count > 0)
            result.Add(dotNetDirectory.VTableFixups);

        AddIfPresent(dotNetDirectory.ExportAddressTable);
        AddIfPresent(dotNetDirectory.ManagedNativeHeader);

        return result;

        void AddIfPresent(ISegment? segment)
        {
            if (segment is not null)
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
            SectionFlags.MemoryRead | SectionFlags.ContentInitializedData | SectionFlags.MemoryDiscardable,
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
    protected override void CreateImportDirectory(PEFileBuilderContext context)
    {
        var image = context.Image;

        bool includeClrBootstrapper = image.DotNetDirectory is not null
            && (
                context.Platform.IsClrBootstrapperRequired
                || (image.DotNetDirectory?.Flags & DotNetDirectoryFlags.ILOnly) == 0
            );

        string clrEntryPointName = (image.Characteristics & Characteristics.Dll) != 0
            ? "_CorDllMain"
            : "_CorExeMain";

        var modules = CollectImportedModules(
            image,
            includeClrBootstrapper,
            clrEntryPointName,
            out var entryPointSymbol
        );

        foreach (var module in modules)
            context.ImportDirectory.AddModule(module);

        if (includeClrBootstrapper)
        {
            if (entryPointSymbol is null)
                throw new InvalidOperationException("Entry point symbol was required but not imported.");

            context.ClrBootstrapper = context.Platform.CreateThunkStub(entryPointSymbol);
        }
    }

    private static List<ImportedModule> CollectImportedModules(
        PEImage image,
        bool requireClrEntryPoint,
        string clrEntryPointName,
        out ImportedSymbol? clrEntryPoint)
    {
        clrEntryPoint = null;

        var modules = image.Imports.ToList();

        if (requireClrEntryPoint)
        {
            // Add mscoree.dll if it wasn't imported yet.
            if (modules.FirstOrDefault(x => x.Name == "mscoree.dll") is not { } mscoreeModule)
            {
                mscoreeModule = new ImportedModule("mscoree.dll");
                modules.Add(mscoreeModule);
            }

            // Add entry point sumbol if it wasn't imported yet.
            clrEntryPoint = mscoreeModule.Symbols.FirstOrDefault(x => x.Name == clrEntryPointName);
            if (clrEntryPoint is null)
            {
                clrEntryPoint = new ImportedSymbol(0, clrEntryPointName);
                mscoreeModule.Symbols.Add(clrEntryPoint);
            }
        }
        else
        {
            // Remove mscoree.dll!_CorXXXMain and entry of mscoree.dll.
            if (modules.FirstOrDefault(x => x.Name == "mscoree.dll") is { } mscoreeModule)
            {
                if (mscoreeModule.Symbols.FirstOrDefault(x => x.Name == clrEntryPointName) is { } entry)
                    mscoreeModule.Symbols.Remove(entry);

                if (mscoreeModule.Symbols.Count == 0)
                    modules.Remove(mscoreeModule);
            }
        }

        return modules;
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

    /// <inheritdoc />
    protected override void AssignDataDirectories(PEFileBuilderContext context, PEFile outputFile)
    {
        var header = outputFile.OptionalHeader;

        header.DataDirectories.Clear();
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

    /// <inheritdoc />
    protected override uint GetEntryPointAddress(PEFileBuilderContext context, PEFile outputFile)
        => context.ClrBootstrapper?.Segment.Rva ?? 0;

    private void ProcessRvasInMetadataTables(PEFileBuilderContext context)
    {
        var tablesStream = context.Image.DotNetDirectory?.Metadata?.GetStream<TablesStream>();
        if (tablesStream is null)
        {
            ErrorListener.RegisterException(new ArgumentException("Image does not have a .NET metadata tables stream."));
            return;
        }

        AddMethodBodySegments(context, tablesStream);
        AddFieldRvaSegments(context, tablesStream);
    }

    /// <summary>
    /// Adds all method bodies referenced by the method metadata table to the PE file.
    /// </summary>
    /// <param name="context">The context for the new PE file.</param>
    /// <param name="tablesStream">The tables stream to get the method body RVAs from.</param>
    protected virtual void AddMethodBodySegments(PEFileBuilderContext context, TablesStream tablesStream)
    {
        var methodTable = tablesStream.GetTable<MethodDefinitionRow>();
        for (uint rid = 1; rid <= methodTable.Count; rid++)
        {
            ref var methodRow = ref  methodTable.GetRowRef(rid);

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

    private ISegment? GetMethodBodySegment(MethodDefinitionRow methodRow)
    {
        if (methodRow.Body.IsBounded)
            return methodRow.Body.GetSegment();

        if (methodRow.Body.CanRead)
        {
            if ((methodRow.ImplAttributes & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.IL)
            {
                var reader = methodRow.Body.CreateReader();
                return CilRawMethodBody.FromReader(ThrowErrorListener.Instance, ref reader);
            }

            ErrorListener.NotSupported("Native unbounded method bodies are not supported.");
        }

        return null;
    }

    /// <summary>
    /// Adds all segments referenced by the FieldRva metadata table to the PE file.
    /// </summary>
    /// <param name="context">The context for the new PE file.</param>
    /// <param name="tablesStream">The tables stream to get the Field RVAs from.</param>
    protected virtual void AddFieldRvaSegments(PEFileBuilderContext context, TablesStream tablesStream)
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

            uint requiredAlignment = TryGetRequiredFieldAlignment(context, row);

            if (requiredAlignment != 0)
            {
                table.Add(data, requiredAlignment);
            }
            else
            {
                table.Add(data);
            }

            row.Data = data.ToReference();
        }
    }
}
