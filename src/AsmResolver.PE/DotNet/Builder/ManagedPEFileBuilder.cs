using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.Builder;
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
    /// method bodies and/or field data. ALl remaining metadata in the tables stream and in the other metadata streams
    /// is written as-is without any change or verification.
    /// </para>
    /// <para>
    /// This class ignores the contents of the imports directory (exposed by the <see cref="IPEImage.Imports"/>
    /// property), as well as the base relocations directory (exposed by the <see cref="IPEImage.Relocations"/> property).
    /// It completely rebuilds the contents of these directories.
    /// </para>
    /// <para>
    /// This class builds up at most three PE sections; .text, .rsrc and .reloc, similar to what a normal .NET language
    /// compiler would emit. Almost everything is put into the .text section, including the import and debug directories.
    /// The win32 resources are put into .rsrc section, and this section will only be added if there is at least one entry
    /// in the root resource directory of the <see cref="IPEImage.Resources"/> property. Similarly, the .reloc section
    /// is only added if a bootstrapper code segment initializing the CLR is added and needs base relocations. 
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
                ImportDirectory = new ImportDirectoryBuffer(image.PEKind == OptionalHeaderMagic.Pe32);
                ExportDirectory = new ExportDirectoryBuffer();
                DotNetSegment = new DotNetSegmentBuffer(image.DotNetDirectory);
                ResourceDirectory = new ResourceDirectoryBuffer();
                RelocationsDirectory = new RelocationsDirectoryBuffer();
                FieldRvaDataReader = new FieldRvaDataReader();
                DebugDirectory= new DebugDirectoryBuffer();
                Bootstrapper = CreateBootstrapper(image);
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
            /// Gets the code segment used as a native entrypoint of the resulting PE file.
            /// </summary>
            /// <remarks>
            /// This property might be <c>null</c> if no bootstrapper is to be emitted. For example, since the
            /// bootstrapper is a legacy feature from older versions of the CLR, we do not see this segment in
            /// managed PE files targeting 64-bit architectures. 
            /// </remarks>
            public BootstrapperSegment Bootstrapper
            {
                get;
            }

            /// <summary>
            /// Gets the object responsible for reading a field RVA data.
            /// </summary>
            public IFieldRvaDataReader FieldRvaDataReader
            {
                get;
            }

            private X86BootstrapperSegment CreateBootstrapper(IPEImage image)
            {
                return image.MachineType switch
                {
                    MachineType.I386 => new X86BootstrapperSegment(
                        (image.Characteristics & Characteristics.Dll) != 0,
                        (uint) image.ImageBase, ImportDirectory.ImportAddressDirectory),
                    
                    MachineType.Amd64 => null,
                    
                    _ => throw new NotSupportedException($"Machine type {image.MachineType} is not supported.")
                };
            }
        }

        /// <inheritdoc />
        protected override ManagedPEBuilderContext CreateContext(IPEImage image) => new ManagedPEBuilderContext(image);

        /// <inheritdoc />
        protected override IEnumerable<PESection> CreateSections(IPEImage image, ManagedPEBuilderContext context)
        {
            var sections = new List<PESection>
            {
                CreateTextSection(image, context)
            };

            if (image.Resources != null && image.Resources.Entries.Count > 0)
                sections.Add(CreateRsrcSection(image, context));

            if (context.Bootstrapper != null)
            {
                var relocations = context.Bootstrapper.GetRelocations().ToArray();
                if (relocations.Length > 0)
                    sections.Add(CreateRelocSection(context, relocations));
            }

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
            CreateExportDirectory(image, context);
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

            if (context.Bootstrapper != null)
                contents.Add(context.Bootstrapper);

            return new PESection(".text",
                SectionFlags.ContentCode | SectionFlags.MemoryExecute | SectionFlags.MemoryRead)
            {
                Contents = contents
            };
        }

        private static void CreateImportDirectory(IPEImage image, ManagedPEBuilderContext context)
        {
            if (image.PEKind == OptionalHeaderMagic.Pe32)
            {
                string entrypointName = (image.Characteristics & Characteristics.Dll) != 0
                    ? "_CorDllMain"
                    : "_CorExeMain";
                context.ImportDirectory.AddModule(new ImportedModule("mscoree.dll")
                {
                    Symbols = {new ImportedSymbol(0, entrypointName)}
                });
            }
        }

        private static void CreateExportDirectory(IPEImage image, ManagedPEBuilderContext context)
        {
            if (image.Exports is {} exports && exports.Entries.Count > 0)
                context.ExportDirectory.AddDirectory(exports);
        }

        private static void CreateDebugDirectory(IPEImage image, ManagedPEBuilderContext context)
        {
            foreach (var entry in image.DebugData)
                context.DebugDirectory.AddEntry(entry);
        }

        /// <summary>
        /// Creates the win32 resources section (.rsrc) of the new .NET PE file. 
        /// </summary>
        /// <param name="image">The image to build.</param>
        /// <param name="context">The working space of the builder.</param>
        /// <returns>The resources section.</returns>
        protected virtual PESection CreateRsrcSection(IPEImage image, ManagedPEBuilderContext context)
        {
            context.ResourceDirectory.AddDirectory(image.Resources);
            
            return new PESection(".rsrc", SectionFlags.MemoryRead | SectionFlags.ContentInitializedData)
            {
                Contents = context.ResourceDirectory
            };
        }

        /// <summary>
        /// Creates the base relocations section (.reloc) of the new .NET PE file.
        /// </summary>
        /// <param name="context">The working space of the builder.</param>
        /// <param name="relocations">The working space of the builder.</param>
        /// <returns>The base relocations section.</returns>
        protected virtual PESection CreateRelocSection(ManagedPEBuilderContext context, IEnumerable<BaseRelocation> relocations)
        {
            foreach (var relocation in relocations)
                context.RelocationsDirectory.Add(relocation);
            
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
                : new DataDirectory(0, 0);
            var debugDataDirectory = !context.DebugDirectory.IsEmpty
                ? new DataDirectory(context.DebugDirectory.Rva, context.DebugDirectory.GetPhysicalSize())
                : new DataDirectory(0, 0);
            
            return new[]
            {
                exportDataDirectory,
                new DataDirectory(importDirectory.Rva, importDirectory.GetPhysicalSize()),
                new DataDirectory(resourceDirectory.Rva, resourceDirectory.GetPhysicalSize()),
                new DataDirectory(0, 0),
                new DataDirectory(0, 0),
                new DataDirectory(relocDirectory.Rva, relocDirectory.GetPhysicalSize()),
                debugDataDirectory,
                new DataDirectory(0, 0),
                new DataDirectory(0, 0),
                new DataDirectory(0, 0),
                new DataDirectory(0, 0),
                new DataDirectory(0, 0),
                new DataDirectory(iatDirectory.Rva, iatDirectory.GetPhysicalSize()),
                new DataDirectory(0, 0),
                new DataDirectory(dotNetDirectory.Rva, dotNetDirectory.GetPhysicalSize()),
            };
        }

        /// <inheritdoc />
        protected override uint GetEntrypointAddress(PEFile peFile, IPEImage image, ManagedPEBuilderContext context)
            => context.Bootstrapper?.Rva ?? 0;

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
            var tablesStream = dotNetSegment.DotNetDirectory.Metadata.GetStream<TablesStream>();
            AddMethodBodiesToTable(dotNetSegment.MethodBodyTable, tablesStream);
            AddFieldRvasToTable(context);
        }

        private static void AddMethodBodiesToTable(MethodBodyTableBuffer table, TablesStream tablesStream)
        {
            var methodTable = tablesStream.GetTable<MethodDefinitionRow>();
            for (int i = 0; i < methodTable.Count; i++)
            {
                var methodRow = methodTable[i];

                if (methodRow.Body != null)
                {
                    var bodySegment = GetMethodBodySegment(methodRow, i);
                    if (bodySegment is CilRawMethodBody cilBody)
                        table.AddCilBody(cilBody);
                    else if (bodySegment != null)
                        table.AddNativeBody(bodySegment, 4); // TODO: maybe make customizable?
                    else
                        continue;

                    methodTable[i] = new MethodDefinitionRow(
                        new SegmentReference(bodySegment),
                        methodRow.ImplAttributes,
                        methodRow.Attributes,
                        methodRow.Name,
                        methodRow.Signature,
                        methodRow.ParameterList);
                }
            }
        }

        private static ISegment GetMethodBodySegment(MethodDefinitionRow methodRow, int i)
        {
            if (methodRow.Body.IsBounded)
                return methodRow.Body.GetSegment();
            
            if (methodRow.Body.CanRead)
            {
                if ((methodRow.ImplAttributes & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.IL)
                    return CilRawMethodBody.FromReader(methodRow.Body.CreateReader());
                throw new NotImplementedException("Native unbounded method bodies cannot be reassembled yet.");
            }

            throw new NotSupportedException(
                $"Invalid or unsupported method body reference for method {i + 1}.");
        }

        private static void AddFieldRvasToTable(ManagedPEBuilderContext context)
        {
            var metadata = context.DotNetSegment.DotNetDirectory.Metadata;
            var fieldRvaTable = metadata
                .GetStream<TablesStream>()
                .GetTable<FieldRvaRow>(TableIndex.FieldRva);
            
            if (fieldRvaTable.Count == 0)
                return;
            
            var table = context.DotNetSegment.FieldRvaTable;
            var reader = context.FieldRvaDataReader;
            
            foreach (var row in fieldRvaTable)
            {
                var data = reader.ResolveFieldData(metadata, row);
                table.Add(data);
            }
        }
    }
}