using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Builder
{
    /// <summary>
    /// Provides a base for a PE file builder.
    /// </summary>
    /// <typeparam name="TContext">The type that this builder uses to store intermediate values in.</typeparam>
    public abstract class PEFileBuilderBase<TContext> : IPEFileBuilder
    {
        /// <inheritdoc />
        public virtual PEFile CreateFile(IPEImage image)
        {
            var peFile = new PEFile();
            var context = CreateContext(image);
            
            foreach (var section in CreateSections(image, context))
                peFile.Sections.Add(section);
            
            ComputeHeaderFields(peFile, image, context);
            
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
        /// <param name="image">The image to create sections for.</param>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        /// <returns>The sections.</returns>
        protected abstract IEnumerable<PESection> CreateSections(IPEImage image, TContext context);

        /// <summary>
        /// Creates the data directory headers stored in the optional header of the PE file. 
        /// </summary>
        /// <param name="peFile">The (incomplete) PE file that contains the sections.</param>
        /// <param name="image">The image to create the data directories for.</param>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        /// <returns>The data directories.</returns>
        protected abstract IEnumerable<DataDirectory> CreateDataDirectories(PEFile peFile, IPEImage image, TContext context);

        /// <summary>
        /// Gets the relative virtual address (RVA) to the entrypoint of the PE file.
        /// </summary>
        /// <param name="peFile">The (incomplete) PE file containing the entrypoint.</param>
        /// <param name="image">The image that the PE file was based on.</param>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        /// <returns>The relative virtual address to the entrypoin.</returns>
        protected abstract uint GetEntrypointAddress(PEFile peFile, IPEImage image, TContext context);

        /// <summary>
        /// Gets the file alignment for the new PE file.
        /// </summary>
        /// <param name="peFile">The (incomplete) PE file to be aligned.</param>
        /// <param name="image">The image that the PE file was based on.</param>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        /// <returns>The file alignment. This should be a power of 2 between 512 and 64,000.</returns>
        protected abstract uint GetFileAlignment(PEFile peFile, IPEImage image, TContext context);
        
        /// <summary>
        /// Gets the section alignment for the new PE file.
        /// </summary>
        /// <param name="peFile">The (incomplete) PE file to be aligned.</param>
        /// <param name="image">The image that the PE file was based on.</param>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        /// <returns>
        /// The section alignment. Must be greater or equal to the file alignment. Default is the page size for
        /// the architecture.
        /// </returns>
        protected abstract uint GetSectionAlignment(PEFile peFile, IPEImage image, TContext context);
        
        /// <summary>
        /// Gets the image base for the new PE file.
        /// </summary>
        /// <param name="peFile">The (incomplete) PE file to determine the image base for.</param>
        /// <param name="image">The image that the PE file was based on.</param>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        /// <returns>The image base.</returns>
        protected abstract uint GetImageBase(PEFile peFile, IPEImage image, TContext context);
        
        /// <summary>
        /// Updates the fields in the file header and optional header of the PE file.
        /// </summary>
        /// <param name="peFile">The (incomplete) PE file to update.</param>
        /// <param name="image">The image that the PE file was based on.</param>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        protected virtual void ComputeHeaderFields(PEFile peFile, IPEImage image, TContext context)
        {
            ComputeOptionalHeaderFields(peFile, image, context);
            ComputeFileHeaderFields(peFile, image, context);
            peFile.DosHeader.NextHeaderOffset = peFile.DosHeader.GetPhysicalSize();
        }

        /// <summary>
        /// Updates the fields in the optional header of the PE file.
        /// </summary>
        /// <param name="peFile">The (incomplete) PE file to update.</param>
        /// <param name="image">The image that the PE file was based on.</param>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        protected virtual void ComputeOptionalHeaderFields(PEFile peFile, IPEImage image, TContext context)
        {
            var header = peFile.OptionalHeader;

            header.ImageBase = GetImageBase(peFile, image, context);
            header.SectionAlignment = GetSectionAlignment(peFile, image, context);
            header.FileAlignment = GetFileAlignment(peFile, image, context);
            
            if (header.SectionAlignment < header.FileAlignment)
                throw new ArgumentException("File alignment cannot be larger than the section alignment.");
            
            peFile.UpdateHeaders();

            header.Magic = image.PEKind;
            header.MajorLinkerVersion = 30;
            header.MinorLinkerVersion = 0;

            header.SizeOfCode = 0;
            header.SizeOfInitializedData = 0;
            header.SizeOfUninitializedData = 0;
            header.SizeOfImage = 0;
            foreach (var section in peFile.Sections)
            {
                uint physicalSize = section.GetPhysicalSize();
                if (section.IsContentCode)
                    header.SizeOfCode += physicalSize;
                if (section.IsContentInitializedData)
                    header.SizeOfInitializedData += physicalSize;
                if (section.IsContentUninitializedData)
                    header.SizeOfUninitializedData += physicalSize;
                header.SizeOfImage += section.GetVirtualSize();
            }

            header.AddressOfEntrypoint = GetEntrypointAddress(peFile, image, context);
            
            header.BaseOfCode = peFile.Sections.FirstOrDefault(s => s.IsContentCode)?.Rva ?? 0;
            header.BaseOfData = peFile.Sections.FirstOrDefault(s => s.IsContentInitializedData)?.Rva ?? 0;

            header.MajorOperatingSystemVersion = 4;
            header.MinorOperatingSystemVersion = 0;
            header.MajorImageVersion = 0;
            header.MinorImageVersion = 0;
            header.MajorSubsystemVersion = 4;
            header.MinorSubsystemVersion = 0;
            header.Win32VersionValue = 0;
            header.SizeOfImage = header.SizeOfImage.Align(header.SectionAlignment);
            header.SizeOfHeaders = (peFile.DosHeader.GetPhysicalSize()
                                    + peFile.FileHeader.GetPhysicalSize()
                                    + peFile.OptionalHeader.GetPhysicalSize()
                                    + (uint) peFile.Sections.Count * SectionHeader.SectionHeaderSize)
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

            var dataDirectories = CreateDataDirectories(peFile, image, context).ToList();
            for (int i = 0; i < OptionalHeader.DefaultNumberOfRvasAndSizes; i++)
            {
                header.DataDirectories.Add(i < dataDirectories.Count 
                    ? dataDirectories[i] 
                    : new DataDirectory(0, 0));
            }

            header.NumberOfRvaAndSizes = OptionalHeader.DefaultNumberOfRvasAndSizes;
        }

        /// <summary>
        /// Updates the fields in the file header of the PE file.
        /// </summary>
        /// <param name="peFile">The (incomplete) PE file to update.</param>
        /// <param name="image">The image that the PE file was based on.</param>
        /// <param name="context">The object containing the intermediate values used during the PE file construction.</param>
        protected virtual void ComputeFileHeaderFields(PEFile peFile, IPEImage image, TContext context)
        {
            var header = peFile.FileHeader;

            header.Machine = image.MachineType;
            header.NumberOfSections = (ushort) peFile.Sections.Count;
            header.TimeDateStamp = (uint) (image.TimeDateStamp - new DateTime(1970, 1, 1)).TotalSeconds;
            header.NumberOfSymbols = 0;
            header.PointerToSymbolTable = 0;
            header.SizeOfOptionalHeader = (ushort) peFile.OptionalHeader.GetPhysicalSize();
            header.Characteristics = image.Characteristics;
        }
        
    }
}