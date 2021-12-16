using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.Builder;
using AsmResolver.PE.Code;
using AsmResolver.PE.Debug.Builder;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.Exports.Builder;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Imports.Builder;
using AsmResolver.PE.Platforms;
using AsmResolver.PE.Relocations;
using AsmResolver.PE.Relocations.Builder;
using AsmResolver.PE.Win32Resources.Builder;

namespace AsmResolver.PE.DotNet.Builder
{
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
    /// This class might modify the final imports directory (exposed by the <see cref="IPEImage.Imports"/> property),
    /// as well as the base relocations directory (exposed by the <see cref="IPEImage.Relocations"/> property). In
    /// particular, it might add or remove the entry to <c>mscoree.dll!_CorExeMain</c> or <c>mscoree.dll!_CorDllMain</c>,
    /// depending on the machine type specified by the <see cref="IPEImage.MachineType"/> property.
    /// </para>
    /// <para>
    /// This class builds up at most four PE sections; <c>.text</c>, <c>.sdata</c>, <c>.rsrc</c> and <c>.reloc</c>,
    /// similar to what a normal .NET language compiler would emit. Almost everything is put into the .text section,
    /// including the import and debug directories. The win32 resources are put into <c>.rsrc</c> section, and this
    /// section will only be added if there is at least one entry in the root resource directory of the
    /// <see cref="IPEImage.Resources"/> property. Similarly, the <c>.sdata</c> section is only added if at least
    /// one unmanaged export is added to the PE image. Finally, the <c>.reloc</c> section is only added if at least
    /// one base relocation was put into the directory, or when the CLR bootstrapper requires one.
    /// </para>
    /// </remarks>
    public class ManagedPEFileBuilder : PEFileBuilderBase<ManagedPEFileBuilder.ManagedPEBuilderContext>
    {
        /// <summary>
        /// Provides a working space for constructing a managed portable executable file.
        /// </summary>
        public class ManagedPEBuilderContext
        {
            /// <summary>
            /// Creates a new managed PE file builder context.
            /// </summary>
            /// <param name="image">The image to build.</param>
            public ManagedPEBuilderContext(IPEImage image)
            {
                if (image.DotNetDirectory is null)
                    throw new ArgumentException("Image does not contain a .NET directory.");

                ImportDirectory = new ImportDirectoryBuffer(image.PEKind == OptionalHeaderMagic.Pe32);
                ExportDirectory = new ExportDirectoryBuffer();
                DotNetSegment = new DotNetSegmentBuffer(image.DotNetDirectory);
                ResourceDirectory = new ResourceDirectoryBuffer();
                RelocationsDirectory = new RelocationsDirectoryBuffer();
                FieldRvaDataReader = new FieldRvaDataReader();
                DebugDirectory= new DebugDirectoryBuffer();
                Platform = Platform.Get(image.MachineType);
            }

            /// <summary>
            /// Gets the buffer that builds up a new import lookup and address directory.
            /// </summary>
            public ImportDirectoryBuffer ImportDirectory
            {
                get;
            }

            /// <summary>
            /// Gets the buffer that builds up a new export directory.
            /// </summary>
            public ExportDirectoryBuffer ExportDirectory
            {
                get;
            }

            /// <summary>
            /// Gets the buffer that builds up a new debug directory.
            /// </summary>
            public DebugDirectoryBuffer DebugDirectory
            {
                get;
            }

            /// <summary>
            /// Gets the buffer that builds up all .NET metadata related segments, including metadata, method bodies
            /// and field data.
            /// </summary>
            public DotNetSegmentBuffer DotNetSegment
            {
                get;
            }

            /// <summary>
            /// Gets the buffer that builds up the win32 resources directory.
            /// </summary>
            public ResourceDirectoryBuffer ResourceDirectory
            {
                get;
            }

            /// <summary>
            /// Gets the buffer that builds up the base relocations directory.
            /// </summary>
            public RelocationsDirectoryBuffer RelocationsDirectory
            {
                get;
            }

            /// <summary>
            /// Gets the target platform of the image.
            /// </summary>
            public Platform Platform
            {
                get;
            }

            /// <summary>
            /// Gets the code segment used as a native entrypoint of the resulting PE file.
            /// </summary>
            /// <remarks>
            /// This property might be <c>null</c> if no bootstrapper is to be emitted. For example, since the
            /// bootstrapper is a legacy feature from older versions of the CLR, we do not see this segment in
            /// managed PE files targeting 64-bit architectures.
            /// </remarks>
            public RelocatableSegment? Bootstrapper
            {
                get;
                set;
            }

            /// <summary>
            /// Gets the object responsible for reading a field RVA data.
            /// </summary>
            public IFieldRvaDataReader FieldRvaDataReader
            {
                get;
            }
        }

        /// <inheritdoc />
        protected override ManagedPEBuilderContext CreateContext(IPEImage image) => new(image);

        /// <inheritdoc />
        protected override IEnumerable<PESection> CreateSections(IPEImage image, ManagedPEBuilderContext context)
        {
            // Always create .text section.
            var sections = new List<PESection>
            {
                CreateTextSection(image, context)
            };

            // Add .sdata section when necessary.
            if (image.Exports is not null || image.DotNetDirectory?.VTableFixups is not null)
                sections.Add(CreateSDataSection(image, context));

            // Add .rsrc section when necessary.
            if (image.Resources is not null && image.Resources.Entries.Count > 0)
                sections.Add(CreateRsrcSection(image, context));

            // Collect all base relocations.
            // Since the PE is rebuild in its entirety, all relocations that were originally in the PE are invalidated.
            // Therefore, we filter out all relocations that were added by the reader.
            var relocations = image.Relocations
                .Where(r => r.Location is not PESegmentReference)
                .ToList();

            // Add relocations of the bootstrapper stub if necessary.
            if (context.Bootstrapper.HasValue)
                relocations.AddRange(context.Bootstrapper.Value.Relocations);

            // Add .reloc section when necessary.
            if (relocations.Count > 0)
                sections.Add(CreateRelocSection(context, relocations));

            return sections;
        }

        /// <summary>
        /// Builds up the main text section (.text) of the new .NET PE file.
        /// </summary>
        /// <param name="image">The image to build.</param>
        /// <param name="context">The working space of the builder.</param>
        /// <returns>The .text section.</returns>
        protected virtual PESection CreateTextSection(IPEImage image, ManagedPEBuilderContext context)
        {
            CreateImportDirectory(image, context);
            CreateDebugDirectory(image, context);
            ProcessRvasInMetadataTables(context);

            var contents = new SegmentBuilder();
            if (context.ImportDirectory.Count > 0)
                contents.Add(context.ImportDirectory.ImportAddressDirectory);

            contents.Add(context.DotNetSegment);

            if (context.ImportDirectory.Count > 0)
                contents.Add(context.ImportDirectory);

            if (!context.ExportDirectory.IsEmpty)
                contents.Add(context.ExportDirectory);

            if (!context.DebugDirectory.IsEmpty)
            {
                contents.Add(context.DebugDirectory);
                contents.Add(context.DebugDirectory.ContentsTable);
            }

            if (context.Bootstrapper.HasValue)
                contents.Add(context.Bootstrapper.Value.Segment);

            if (image.Exports is { Entries: { Count: > 0 } entries })
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    var export = entries[i];
                    if (export.Address.IsBounded && export.Address.GetSegment() is { } segment)
                        contents.Add(segment, 4);
                }
            }

            return new PESection(".text",
                SectionFlags.ContentCode | SectionFlags.MemoryExecute | SectionFlags.MemoryRead)
            {
                Contents = contents
            };
        }

        private static void CreateImportDirectory(IPEImage image, ManagedPEBuilderContext context)
        {
            bool importEntrypointRequired = context.Platform.IsClrBootstrapperRequired
                                            || (image.DotNetDirectory!.Flags & DotNetDirectoryFlags.ILOnly) == 0;
            string entrypointName = (image.Characteristics & Characteristics.Dll) != 0
                ? "_CorDllMain"
                : "_CorExeMain";

            var modules = CollectImportedModules(image, importEntrypointRequired, entrypointName, out var entrypointSymbol);

            foreach (var module in modules)
                context.ImportDirectory.AddModule(module);

            if (importEntrypointRequired)
            {
                if (entrypointSymbol is null)
                    throw new InvalidOperationException("Entrypoint symbol was required but not imported.");

                context.Bootstrapper = context.Platform.CreateThunkStub(image.ImageBase, entrypointSymbol);
            }
        }

        private static List<IImportedModule> CollectImportedModules(
            IPEImage image,
            bool entryRequired,
            string mscoreeEntryName,
            out ImportedSymbol? entrypointSymbol)
        {
            var modules = new List<IImportedModule>();

            IImportedModule? mscoreeModule = null;
            entrypointSymbol = null;

            foreach (var module in image.Imports)
            {
                // Check if the CLR entrypoint is already imported.
                if (module.Name == "mscoree.dll")
                {
                    mscoreeModule = module;

                    // Find entrypoint in this imported module.
                    if (entryRequired)
                        entrypointSymbol = mscoreeModule.Symbols.FirstOrDefault(s => s.Name == mscoreeEntryName);

                    // Only include mscoree.dll if necessary.
                    if (entryRequired || module.Symbols.Count > 1)
                        modules.Add(module);
                }
                else
                {
                    // Imported module is some other module. Just add in its entirety.
                    modules.Add(module);
                }
            }

            if (entryRequired)
            {
                // Add mscoree.dll if it wasn't imported yet.
                if (mscoreeModule is null)
                {
                    mscoreeModule = new ImportedModule("mscoree.dll");
                    modules.Add(mscoreeModule);
                }

                // Add entrypoint sumbol if it wasn't imported yet.
                if (entrypointSymbol is null)
                {
                    entrypointSymbol = new ImportedSymbol(0, mscoreeEntryName);
                    mscoreeModule.Symbols.Add(entrypointSymbol);
                }
            }

            return modules;
        }

        private static void CreateDebugDirectory(IPEImage image, ManagedPEBuilderContext context)
        {
            foreach (var entry in image.DebugData)
                context.DebugDirectory.AddEntry(entry);
        }

        /// <summary>
        /// Creates the .sdata section containing the exports and vtables directory of the new .NET PE file.
        /// </summary>
        /// <param name="image">The image to build.</param>
        /// <param name="context">The working space of the builder.</param>
        /// <returns>The section.</returns>
        protected virtual PESection CreateSDataSection(IPEImage image, ManagedPEBuilderContext context)
        {
            var contents = new SegmentBuilder();

            if (image.DotNetDirectory?.VTableFixups is { } fixups)
            {
                for (int i = 0; i < fixups.Count; i++)
                    contents.Add(fixups[i].Tokens);
            }

            if (image.Exports is { Entries: { Count: > 0 } } exports)
            {
                context.ExportDirectory.AddDirectory(exports);
                contents.Add(context.ExportDirectory, 4);
            }

            return new PESection(
                ".sdata",
                SectionFlags.MemoryRead | SectionFlags.MemoryWrite | SectionFlags.ContentInitializedData,
                contents);
        }

        /// <summary>
        /// Creates the win32 resources section (.rsrc) of the new .NET PE file.
        /// </summary>
        /// <param name="image">The image to build.</param>
        /// <param name="context">The working space of the builder.</param>
        /// <returns>The resources section.</returns>
        protected virtual PESection CreateRsrcSection(IPEImage image, ManagedPEBuilderContext context)
        {
            context.ResourceDirectory.AddDirectory(image.Resources!);

            return new PESection(
                ".rsrc",
                SectionFlags.MemoryRead | SectionFlags.ContentInitializedData,
                context.ResourceDirectory);
        }

        /// <summary>
        /// Creates the base relocations section (.reloc) of the new .NET PE file.
        /// </summary>
        /// <param name="context">The working space of the builder.</param>
        /// <param name="relocations">The working space of the builder.</param>
        /// <returns>The base relocations section.</returns>
        protected virtual PESection CreateRelocSection(ManagedPEBuilderContext context, IReadOnlyList<BaseRelocation> relocations)
        {
            for (int i = 0; i < relocations.Count; i++)
                context.RelocationsDirectory.Add(relocations[i]);

            return new PESection(".reloc", SectionFlags.MemoryRead | SectionFlags.ContentInitializedData)
            {
                Contents =  context.RelocationsDirectory
            };
        }

        /// <inheritdoc />
        protected override IEnumerable<DataDirectory> CreateDataDirectories(PEFile peFile, IPEImage image, ManagedPEBuilderContext context)
        {
            var importDirectory = context.ImportDirectory;
            var resourceDirectory = context.ResourceDirectory;
            var relocDirectory = context.RelocationsDirectory;
            var iatDirectory = importDirectory.ImportAddressDirectory;
            var dotNetDirectory = context.DotNetSegment.DotNetDirectory;

            var exportDataDirectory = !context.ExportDirectory.IsEmpty
                ? new DataDirectory(context.ExportDirectory.Rva, context.ExportDirectory.GetPhysicalSize())
                : default;
            var debugDataDirectory = !context.DebugDirectory.IsEmpty
                ? new DataDirectory(context.DebugDirectory.Rva, context.DebugDirectory.GetPhysicalSize())
                : default;

            return new[]
            {
                exportDataDirectory,
                new DataDirectory(importDirectory.Rva, importDirectory.GetPhysicalSize()),
                new DataDirectory(resourceDirectory.Rva, resourceDirectory.GetPhysicalSize()),
                default,
                default,
                new DataDirectory(relocDirectory.Rva, relocDirectory.GetPhysicalSize()),
                debugDataDirectory,
                default,
                default,
                default,
                default,
                default,
                new DataDirectory(iatDirectory.Rva, iatDirectory.GetPhysicalSize()),
                default,
                new DataDirectory(dotNetDirectory.Rva, dotNetDirectory.GetPhysicalSize()),
            };
        }

        /// <inheritdoc />
        protected override uint GetEntrypointAddress(PEFile peFile, IPEImage image, ManagedPEBuilderContext context)
            => context.Bootstrapper?.Segment.Rva ?? 0;

        /// <inheritdoc />
        protected override uint GetFileAlignment(PEFile peFile, IPEImage image, ManagedPEBuilderContext context)
            => 0x200;

        /// <inheritdoc />
        protected override uint GetSectionAlignment(PEFile peFile, IPEImage image, ManagedPEBuilderContext context)
            => 0x2000;

        /// <inheritdoc />
        protected override uint GetImageBase(PEFile peFile, IPEImage image, ManagedPEBuilderContext context)
            => (uint) image.ImageBase;

        private static void ProcessRvasInMetadataTables(ManagedPEBuilderContext context)
        {
            var dotNetSegment = context.DotNetSegment;
            var tablesStream = dotNetSegment.DotNetDirectory.Metadata?.GetStream<TablesStream>();
            if (tablesStream is null)
                throw new ArgumentException("Image does not have a .NET metadata tables stream.");

            AddMethodBodiesToTable(dotNetSegment.MethodBodyTable, tablesStream);
            AddFieldRvasToTable(context);
        }

        private static void AddMethodBodiesToTable(MethodBodyTableBuffer table, TablesStream tablesStream)
        {
            var methodTable = tablesStream.GetTable<MethodDefinitionRow>();
            for (int i = 0; i < methodTable.Count; i++)
            {
                var methodRow = methodTable[i];

                var bodySegment = GetMethodBodySegment(methodRow);
                if (bodySegment is CilRawMethodBody cilBody)
                    table.AddCilBody(cilBody);
                else if (bodySegment is not null)
                    table.AddNativeBody(bodySegment, 4); // TODO: maybe make customizable?
                else
                    continue;

                methodTable[i] = new MethodDefinitionRow(
                    bodySegment.ToReference(),
                    methodRow.ImplAttributes,
                    methodRow.Attributes,
                    methodRow.Name,
                    methodRow.Signature,
                    methodRow.ParameterList);
            }
        }

        private static ISegment? GetMethodBodySegment(MethodDefinitionRow methodRow)
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

                throw new NotImplementedException("Native unbounded method bodies cannot be reassembled yet.");
            }

            return null;
        }

        private static void AddFieldRvasToTable(ManagedPEBuilderContext context)
        {
            var metadata = context.DotNetSegment.DotNetDirectory.Metadata;
            var fieldRvaTable = metadata
                !.GetStream<TablesStream>()
                !.GetTable<FieldRvaRow>(TableIndex.FieldRva);

            if (fieldRvaTable.Count == 0)
                return;

            var table = context.DotNetSegment.FieldRvaTable;
            var reader = context.FieldRvaDataReader;

            for (int i = 0; i < fieldRvaTable.Count; i++)
            {
                var data = reader.ResolveFieldData(ThrowErrorListener.Instance, metadata, fieldRvaTable[i]);
                if (data is null)
                    continue;

                table.Add(data);
            }
        }
    }
}
