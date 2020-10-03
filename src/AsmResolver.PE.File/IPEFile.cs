using System;
using System.Collections.Generic;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.File
{
    /// <summary>
    /// Provides a writable implementation of the <see cref="IPEFile"/> interface.
    /// </summary>
    public interface IPEFile : ISegmentReferenceResolver, IOffsetConverter
    {
        /// <summary>
        /// Gets or sets the DOS header of the PE file.
        /// </summary>
        DosHeader DosHeader
        {
            get;
        }

        /// <summary>
        /// Gets or sets the COFF file header of the portable executable (PE) file.
        /// </summary>
        FileHeader FileHeader
        {
            get;
        }

        /// <summary>
        /// Gets or sets the optional header of the portable executable (PE) file.
        /// </summary>
        OptionalHeader OptionalHeader
        {
            get;
        }

        /// <summary>
        /// Gets a collection of sections present in the portable executable (PE) file.
        /// </summary>
        IList<PESection> Sections
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating the mapping mode of the PE file. If the PE file is in its mapped form,
        /// then every offset of all segments in the PE file will be equal to the physical memory address.
        /// If the file is in its unmapped form, the offsets will be equal to the file offset. 
        /// </summary>
        PEMappingMode MappingMode
        {
            get;
        }
        
        /// <summary>
        /// Finds the section containing the provided virtual address.
        /// </summary>
        /// <param name="rva">The virtual address.</param>
        /// <returns>The section containing the virtual address.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the virtual address does not fall within any of the sections.</exception>
        PESection GetSectionContainingRva(uint rva);

        /// <summary>
        /// Attempts to find the section containing the provided virtual address.
        /// </summary>
        /// <param name="rva">The virtual address.</param>
        /// <param name="section">The section that was found.</param>
        /// <returns><c>true</c> if the section was found, <c>false</c> otherwise.</returns>
        bool TryGetSectionContainingRva(uint rva, out PESection section);

        /// <summary>
        /// Obtains a reader that spans the provided data directory.
        /// </summary>
        /// <param name="dataDirectory">The data directory to read.</param>
        /// <returns>The reader.</returns>
        IBinaryStreamReader CreateDataDirectoryReader(DataDirectory dataDirectory);

        /// <summary>
        /// Attempts to create a reader that spans the provided data directory.
        /// </summary>
        /// <param name="dataDirectory">The data directory to read.</param>
        /// <param name="reader">The reader that was created.</param>
        /// <returns><c>true</c> if the reader was created successfully, <c>false</c> otherwise.</returns>
        bool TryCreateDataDirectoryReader(DataDirectory dataDirectory, out IBinaryStreamReader reader);

        /// <summary>
        /// Creates a new reader at the provided virtual address.
        /// </summary>
        /// <param name="rva">The virtual address to start reading at.</param>
        /// <returns>The reader.</returns>
        IBinaryStreamReader CreateReaderAtRva(uint rva);

        /// <summary>
        /// Attempts to create a new reader at the provided virtual address.
        /// </summary>
        /// <param name="rva">The virtual address to start reading at.</param>
        /// <param name="reader">The reader that was created.</param>
        /// <returns><c>true</c> if the reader was created successfully, <c>false</c> otherwise.</returns>
        bool TryCreateReaderAtRva(uint rva, out IBinaryStreamReader reader);

        /// <summary>
        /// Creates a new reader of a chunk of data at the provided virtual address.
        /// </summary>
        /// <param name="rva">The virtual address to start reading at.</param>
        /// <param name="size">The number of bytes in the chunk.</param>
        /// <returns>The reader.</returns>
        IBinaryStreamReader CreateReaderAtRva(uint rva, uint size);

        /// <summary>
        /// Attempts to create a new reader of a chunk of data at the provided virtual address.
        /// </summary>
        /// <param name="rva">The virtual address to start reading at.</param>
        /// <param name="size">The number of bytes in the chunk.</param>
        /// <param name="reader">The reader that was created.</param>
        /// <returns><c>true</c> if the reader was created successfully, <c>false</c> otherwise.</returns>
        bool TryCreateReaderAtRva(uint rva, uint size, out IBinaryStreamReader reader);
        
        /// <summary>
        /// Creates a new reader at the provided file offset.
        /// </summary>
        /// <param name="fileOffset">The file offset to start reading at.</param>
        /// <returns>The reader.</returns>
        IBinaryStreamReader CreateReaderAtFileOffset(uint fileOffset);

        /// <summary>
        /// Attempts to create a new reader at the provided file offset.
        /// </summary>
        /// <param name="fileOffset">The file offset to start reading at.</param>
        /// <param name="reader">The reader that was created.</param>
        /// <returns><c>true</c> if the reader was created successfully, <c>false</c> otherwise.</returns>
        public bool TryCreateReaderAtFileOffset(uint fileOffset, out IBinaryStreamReader reader);

        /// <summary>
        /// Creates a new reader of a chunk of data at the provided file offset.
        /// </summary>
        /// <param name="fileOffset">The file offset to start reading at.</param>
        /// <param name="size">The number of bytes in the chunk.</param>
        /// <returns>The reader.</returns>
        public IBinaryStreamReader CreateReaderAtFileOffset(uint fileOffset, uint size);

        /// <summary>
        /// Attempts to create a new reader of a chunk of data at the provided file offset.
        /// </summary>
        /// <param name="fileOffset">The file offset to start reading at.</param>
        /// <param name="size">The number of bytes in the chunk.</param>
        /// <param name="reader">The reader that was created.</param>
        /// <returns><c>true</c> if the reader was created successfully, <c>false</c> otherwise.</returns>
        public bool TryCreateReaderAtFileOffset(uint fileOffset, uint size, out IBinaryStreamReader reader);
    }
}