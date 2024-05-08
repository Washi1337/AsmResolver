using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Imports;

namespace AsmResolver.PE.Builder
{
    /// <summary>
    /// Provides a base for a PE file builder.
    /// </summary>
    public abstract class PEFileBuilder : PEFileBuilder<PEFileBuilderContext>
    {
        /// <inheritdoc />
        protected override PEFileBuilderContext CreateContext(IPEImage image) => new(image);
    }

    /// <summary>
    /// Provides a base for a PE file builder.
    /// </summary>
    /// <typeparam name="TContext">The type that this builder uses to store intermediate values in.</typeparam>
    public abstract class PEFileBuilder<TContext> : IPEFileBuilder
        where TContext : PEFileBuilderContext
    {
        /// <inheritdoc />
        public virtual PEFile CreateFile(IPEImage image)
        {
            var peFile = new PEFile();
            var context = CreateContext(image);

            CreateDataDirectoryBuffers(context);

            foreach (var section in CreateSections(context))
                peFile.Sections.Add(section);

            ComputeHeaderFields(context, peFile);

            return peFile;
        }

        /// <summary>
        /// Creates a new context for temporary storage of intermediate values during the construction of a PE image.
        /// </summary>
        /// <param name="image">The image to build.</param>
        /// <returns>The context.</returns>
        protected abstract TContext CreateContext(IPEImage image);

        /// <summary>
        /// Creates the sections of the PE image.
        /// </summary>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        /// <returns>The sections.</returns>
        protected abstract IEnumerable<PESection> CreateSections(TContext context);

        /// <summary>
        /// Creates the data directory headers stored in the optional header of the PE file.
        /// </summary>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        /// <param name="outputFile">The (incomplete) PE file that contains the sections.</param>
        /// <returns>The data directories.</returns>
        protected abstract void AssignDataDirectories(TContext context, PEFile outputFile);

        /// <summary>
        /// Gets the relative virtual address (RVA) to the entry point of the PE file.
        /// </summary>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        /// <param name="outputFile">The (incomplete) PE file.</param>
        /// <returns>The relative virtual address to the entry point.</returns>
        protected abstract uint GetEntryPointAddress(TContext context, PEFile outputFile);

        /// <summary>
        /// Gets the file alignment for the new PE file.
        /// </summary>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        /// <param name="outputFile">The (incomplete) PE file.</param>
        /// <returns>The file alignment. This should be a power of 2 between 512 and 64,000.</returns>
        protected virtual uint GetFileAlignment(TContext context, PEFile outputFile)
            => context.Image.PEFile?.OptionalHeader.FileAlignment ?? 0x200;

        /// <summary>
        /// Gets the section alignment for the new PE file.
        /// </summary>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        /// <param name="outputFile">The (incomplete) PE file.</param>
        /// <returns>
        /// The section alignment. Must be greater or equal to the file alignment. Default is the page size for
        /// the architecture.
        /// </returns>
        protected virtual uint GetSectionAlignment(TContext context, PEFile outputFile)
            => context.Image.PEFile?.OptionalHeader.SectionAlignment ?? 0x2000;

        /// <summary>
        /// Gets the image base for the new PE file.
        /// </summary>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        /// <param name="outputFile">The (incomplete) PE file.</param>
        /// <returns>The image base.</returns>
        protected virtual ulong GetImageBase(TContext context, PEFile outputFile)
            => context.Image.ImageBase;

        /// <summary>
        /// Updates the fields in the file header and optional header of the PE file.
        /// </summary>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        /// <param name="outputFile">The (incomplete) PE file to update.</param>
        protected virtual void ComputeHeaderFields(TContext context, PEFile outputFile)
        {
            ComputeOptionalHeaderFields(context, outputFile);
            ComputeFileHeaderFields(context, outputFile);

            outputFile.DosHeader.NextHeaderOffset = outputFile.DosHeader.GetPhysicalSize();
        }

        /// <summary>
        /// Updates the fields in the optional header of the PE file.
        /// </summary>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        /// <param name="outputFile">The (incomplete) PE file to update.</param>
        protected virtual void ComputeOptionalHeaderFields(TContext context, PEFile outputFile)
        {
            var image = context.Image;
            var header = outputFile.OptionalHeader;

            header.ImageBase = GetImageBase(context, outputFile);
            header.SectionAlignment = GetSectionAlignment(context, outputFile);
            header.FileAlignment = GetFileAlignment(context, outputFile);

            if (header.SectionAlignment < header.FileAlignment)
                throw new ArgumentException("File alignment cannot be larger than the section alignment.");

            outputFile.UpdateHeaders();

            header.Magic = image.PEKind;
            header.MajorLinkerVersion = 0x30;
            header.MinorLinkerVersion = 0;

            header.SizeOfCode = 0;
            header.SizeOfInitializedData = 0;
            header.SizeOfUninitializedData = 0;
            header.SizeOfImage = 0;

            for (int i = 0; i < outputFile.Sections.Count; i++)
            {
                var section = outputFile.Sections[i];
                uint physicalSize = section.GetPhysicalSize();

                if (section.IsContentCode)
                    header.SizeOfCode += physicalSize;
                if (section.IsContentInitializedData)
                    header.SizeOfInitializedData += physicalSize;
                if (section.IsContentUninitializedData)
                    header.SizeOfUninitializedData += physicalSize;
                header.SizeOfImage += section.GetVirtualSize();
            }

            header.AddressOfEntryPoint = GetEntryPointAddress(context, outputFile);

            header.BaseOfCode = outputFile.Sections.FirstOrDefault(s => s.IsContentCode)?.Rva ?? 0;
            header.BaseOfData = outputFile.Sections.FirstOrDefault(s => s.IsContentInitializedData)?.Rva ?? 0;

            header.MajorOperatingSystemVersion = 4;
            header.MinorOperatingSystemVersion = 0;
            header.MajorImageVersion = 0;
            header.MinorImageVersion = 0;
            header.MajorSubsystemVersion = 4;
            header.MinorSubsystemVersion = 0;
            header.Win32VersionValue = 0;
            header.SizeOfImage = header.SizeOfImage.Align(header.SectionAlignment);
            header.SizeOfHeaders = (outputFile.DosHeader.GetPhysicalSize()
                                    + outputFile.FileHeader.GetPhysicalSize()
                                    + outputFile.OptionalHeader.GetPhysicalSize()
                                    + (uint) outputFile.Sections.Count * SectionHeader.SectionHeaderSize)
                .Align(header.FileAlignment);

            header.CheckSum = 0; // TODO: actually calculate a checksum.

            header.SubSystem = image.SubSystem;
            header.DllCharacteristics = image.DllCharacteristics;

            // TODO: make more configurable.
            header.SizeOfStackReserve = 0x00100000;
            header.SizeOfStackCommit = 0x00001000;
            header.SizeOfHeapReserve = 0x00100000;
            header.SizeOfHeapCommit = 0x00001000;
            header.LoaderFlags = 0;

            AssignDataDirectories(context, outputFile);

            header.NumberOfRvaAndSizes = OptionalHeader.DefaultNumberOfRvasAndSizes;
        }

        /// <summary>
        /// Updates the fields in the file header of the PE file.
        /// </summary>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        /// <param name="outputFile">The (incomplete) PE file to update.</param>
        protected virtual void ComputeFileHeaderFields(TContext context, PEFile outputFile)
        {
            var image = context.Image;
            var header = outputFile.FileHeader;

            header.Machine = context.Image.MachineType;
            header.NumberOfSections = (ushort) outputFile.Sections.Count;
            header.TimeDateStamp = (uint) (image.TimeDateStamp - new DateTime(1970, 1, 1)).TotalSeconds;
            header.NumberOfSymbols = 0;
            header.PointerToSymbolTable = 0;
            header.SizeOfOptionalHeader = (ushort) outputFile.OptionalHeader.GetPhysicalSize();
            header.Characteristics = image.Characteristics;
        }

        /// <summary>
        /// Populates all buffers with data from the input image.
        /// </summary>
        /// <param name="context">The builder context.</param>
        protected virtual void CreateDataDirectoryBuffers(TContext context)
        {
            CreateExportDirectory(context);
            CreateImportDirectory(context);
            CreateDebugDirectory(context);
            CreateResourcesDirectory(context);
            CreateRelocationsDirectory(context);
        }

        /// <summary>
        /// Populates the import directory buffer.
        /// </summary>
        /// <param name="context">The builder context.</param>
        protected virtual void CreateImportDirectory(TContext context)
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

        /// <summary>
        /// Collects all imported modules in an input image, and adds any CLR entry points when required.
        /// </summary>
        /// <param name="image">The image to pull the data from.</param>
        /// <param name="requireClrEntryPoint"><c>true</c> if the CLR entry point should be included.</param>
        /// <param name="clrEntryPointName">The name of the CLR entry point.</param>
        /// <param name="clrEntryPoint">The imported symbol of the CLR entry point.</param>
        /// <returns>The modules.</returns>
        protected static IList<IImportedModule> CollectImportedModules(
            IPEImage image,
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

            return modules;
        }

        /// <summary>
        /// Populates the debug directory buffer.
        /// </summary>
        /// <param name="context">The builder context.</param>
        protected virtual void CreateDebugDirectory(TContext context)
        {
            var entries = context.Image.DebugData;
            for (int i = 0; i < entries.Count; i++)
                context.DebugDirectory.AddEntry(entries[i]);
        }

        /// <summary>
        /// Populates the exports directory buffer.
        /// </summary>
        /// <param name="context">The builder context.</param>
        protected virtual void CreateExportDirectory(TContext context)
        {
            if (context.Image.Exports is { Entries.Count: > 0 } exports)
                context.ExportDirectory.AddDirectory(exports);
        }

        /// <summary>
        /// Populates the resources directory buffer.
        /// </summary>
        /// <param name="context">The builder context.</param>
        protected virtual void CreateResourcesDirectory(TContext context)
        {
            if (context.Image.Resources is not null)
                context.ResourceDirectory.AddDirectory(context.Image.Resources);
        }

        /// <summary>
        /// Populates the base relocations directory buffer.
        /// </summary>
        /// <param name="context">The builder context.</param>
        protected virtual void CreateRelocationsDirectory(TContext context)
        {
            var relocations = context.Image.Relocations;
            for (int i = 0; i < relocations.Count; i++)
                context.RelocationsDirectory.Add(relocations[i]);
        }
    }
}
