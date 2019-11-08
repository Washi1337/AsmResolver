// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

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
    public abstract class PEFileBuilderBase<TContext> : IPEFileBuilder
    {
        /// <inheritdoc />
        public virtual PEFile ConstructPEFile(IPEImage image)
        {
            var peFile = new PEFile();
            var context = CreateContext(image);
            
            foreach (var section in CreateSections(image, context))
                peFile.Sections.Add(section);
            
            ComputeHeaderFields(peFile, image, context);
            
            return peFile;
        }

        protected abstract TContext CreateContext(IPEImage image);

        /// <summary>
        /// Creates the sections of the PE image. 
        /// </summary>
        /// <param name="image">The image to create sections for.</param>
        /// <returns>The sections.</returns>
        protected abstract IEnumerable<PESection> CreateSections(IPEImage image, TContext context);

        protected abstract void ComputeOffsets(PEFile peFile, IPEImage image, TContext context);
        
        /// <summary>
        /// Creates the data directory headers stored in the optional header of the PE file. 
        /// </summary>
        /// <param name="peFile">The (incomplete) PE file that contains the sections.</param>
        /// <param name="image">The image to create the data directories for.</param>
        /// <returns>The data directories.</returns>
        protected abstract IEnumerable<DataDirectory> CreateDataDirectories(PEFile peFile, IPEImage image, TContext context);

        /// <summary>
        /// Gets the relative virtual address (RVA) to the entrypoint of the PE file.
        /// </summary>
        /// <param name="peFile">The (incomplete) PE file containing the entrypoint.</param>
        /// <param name="image">The image that the PE file was based on.</param>
        /// <returns>The relative virtual address to the entrypoin.</returns>
        protected abstract uint GetEntrypointAddress(PEFile peFile, IPEImage image, TContext context);

        /// <summary>
        /// Gets the file alignment for the new PE file.
        /// </summary>
        /// <param name="peFile">The (incomplete) PE file to be aligned.</param>
        /// <param name="image">The image that the PE file was based on.</param>
        /// <returns>The file alignment. This should be a power of 2 between 512 and 64,000.</returns>
        protected abstract uint GetFileAlignment(PEFile peFile, IPEImage image, TContext context);
        
        /// <summary>
        /// Gets the section alignment for the new PE file.
        /// </summary>
        /// <param name="peFile">The (incomplete) PE file to be aligned.</param>
        /// <param name="image">The image that the PE file was based on.</param>
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
        /// <returns>The image base.</returns>
        protected abstract uint GetImageBase(PEFile peFile, IPEImage image, TContext context);
        
        /// <summary>
        /// Updates the fields in the file header and optional header of the PE file.
        /// </summary>
        /// <param name="peFile">The (incomplete) PE file to update.</param>
        /// <param name="image">The image that the PE file was based on.</param>
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
                if (section.Header.IsContentCode)
                    header.SizeOfCode += section.Header.SizeOfRawData;
                if (section.Header.IsContentInitializedData)
                    header.SizeOfInitializedData += section.Header.SizeOfRawData;
                if (section.Header.IsContentUninitializedData)
                    header.SizeOfUninitializedData += section.Header.GetPhysicalSize();
                header.SizeOfImage += section.Header.VirtualSize;
            }

            header.AddressOfEntrypoint = GetEntrypointAddress(peFile, image, context);
            header.BaseOfCode = peFile.GetSectionContainingRva(header.AddressOfEntrypoint).Header.VirtualAddress;
            header.BaseOfData = peFile.Sections.FirstOrDefault(s => s.Header.IsContentInitializedData)?.Header
                                    .VirtualAddress ?? 0;

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