using System;
using System.Text;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.File
{
    /// <summary>
    /// Represents a single section in a portable executable (PE) file.
    /// </summary>
    public class PESection : IReadableSegment, IOffsetConverter
    {
        private string _name;

        /// <summary>
        /// Creates a new empty section.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <param name="characteristics">The section flags.</param>
        public PESection(string name, SectionFlags characteristics)
        {
            Name = name;
            Characteristics = characteristics;
        }

        /// <summary>
        /// Creates a new empty section.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <param name="characteristics">The section flags.</param>
        /// <param name="contents">The contents of the section.</param>
        public PESection(string name, SectionFlags characteristics, ISegment contents)
        {
            Name = name;
            Characteristics = characteristics;
            Contents = contents;
        }

        /// <summary>
        /// Copy a new section.
        /// </summary>
        /// <param name="section">The section to be copied.</param>
        public PESection(PESection section) 
            : this (section.Name, section.Characteristics, section.Contents)
        {
        }

        /// <summary>
        /// Creates a new section with the provided contents.
        /// </summary>
        /// <param name="header">The header to associate to the section.</param>
        /// <param name="contents">The contents of the section.</param>
        public PESection(SectionHeader header, ISegment contents)
        {
            Name = header.Name;
            Characteristics = header.Characteristics;
            Contents = contents;
        }

        public PEFile ContainingFile
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the name of the section.
        /// </summary>
        /// <remarks>
        /// The name of the section is a UTF-8 string that can be no longer than 8 characters long.
        /// </remarks>
        public string Name
        {
            get => _name;
            set
            {
                if (Encoding.UTF8.GetByteCount(value) > 8)
                    throw new ArgumentException("Name is too long.");
                _name = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the characteristics of the section.
        /// </summary>
        public SectionFlags Characteristics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the section contains executable code.
        /// </summary>
        public bool IsContentCode
        {
            get => (Characteristics & SectionFlags.ContentCode) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.ContentCode)
                                     | (value ? SectionFlags.ContentCode : 0);
        }
        /// <summary>
        /// Gets or sets a value indicating the section contains initialized data.
        /// </summary>
        public bool IsContentInitializedData
        {
            get => (Characteristics & SectionFlags.ContentInitializedData) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.ContentInitializedData)
                                     | (value ? SectionFlags.ContentInitializedData : 0);
        }
        /// <summary>
        /// Gets or sets a value indicating the section contains uninitialized data.
        /// </summary>
        public bool IsContentUninitializedData
        {
            get => (Characteristics & SectionFlags.ContentUninitializedData) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.ContentUninitializedData)
                                     | (value ? SectionFlags.ContentUninitializedData : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the section can be discarded as needed.
        /// </summary>
        public bool IsMemoryDiscardable
        {
            get => (Characteristics & SectionFlags.MemoryDiscardable) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.MemoryDiscardable)
                                     | (value ? SectionFlags.MemoryDiscardable : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the section cannot be cached.
        /// </summary>
        public bool IsMemoryNotCached
        {
            get => (Characteristics & SectionFlags.MemoryNotCached) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.MemoryNotCached)
                                     | (value ? SectionFlags.MemoryNotCached : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the section is not pageable.
        /// </summary>
        public bool IsMemoryNotPaged
        {
            get => (Characteristics & SectionFlags.MemoryNotPaged) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.MemoryNotPaged)
                                     | (value ? SectionFlags.MemoryNotPaged : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the section can be shared in memory.
        /// </summary>
        public bool IsMemoryShared
        {
            get => (Characteristics & SectionFlags.MemoryShared) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.MemoryShared)
                                     | (value ? SectionFlags.MemoryShared : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the section can be executed as code.
        /// </summary>
        public bool IsMemoryExecute
        {
            get => (Characteristics & SectionFlags.MemoryExecute) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.MemoryExecute)
                                     | (value ? SectionFlags.MemoryExecute : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the section can be read.
        /// </summary>
        public bool IsMemoryRead
        {
            get => (Characteristics & SectionFlags.MemoryRead) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.MemoryRead)
                                     | (value ? SectionFlags.MemoryRead : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the section can be written.
        /// </summary>
        public bool IsMemoryWrite
        {
            get => (Characteristics & SectionFlags.MemoryWrite) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.MemoryWrite)
                                     | (value ? SectionFlags.MemoryWrite : 0);
        }

        /// <summary>
        /// Gets or sets the contents of the section.
        /// </summary>
        public ISegment Contents
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the section is readable using a binary stream reader.
        /// </summary>
        public bool IsReadable => Contents is IReadableSegment;

        /// <inheritdoc />
        public uint FileOffset => Contents?.FileOffset ?? 0;

        /// <inheritdoc />
        public uint Rva => Contents?.Rva ?? 0;

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva) => Contents.UpdateOffsets(newFileOffset, newRva);

        /// <inheritdoc />
        public uint GetPhysicalSize() => Contents.GetPhysicalSize();

        /// <inheritdoc />
        public uint GetVirtualSize() => Contents.GetVirtualSize();

        /// <inheritdoc />
        public IBinaryStreamReader CreateReader(uint fileOffset, uint size)
        {
            if (!IsReadable)
                throw new InvalidOperationException("Section contents is not readable.");
            return ((IReadableSegment) Contents).CreateReader(fileOffset, size);
        }

        /// <summary>
        /// Creates a new header for this section.
        /// </summary>
        public SectionHeader CreateHeader()
        {
            uint alignment = ContainingFile?.OptionalHeader.FileAlignment ?? 0x200;
            return new SectionHeader(Name, Characteristics)
            {
                PointerToRawData = FileOffset,
                SizeOfRawData = GetPhysicalSize().Align(alignment),
                VirtualAddress = Rva,
                VirtualSize = GetVirtualSize(),
                NumberOfRelocations = 0,
                PointerToRelocations = 0,
                NumberOfLineNumbers = 0,
                PointerToLineNumbers = 0
            };
        }

        /// <summary>
        /// Determines whether the provided file offset falls within the section that the header describes. 
        /// </summary>
        /// <param name="fileOffset">The offset to check.</param>
        /// <returns><c>true</c> if the file offset falls within the section, <c>false</c> otherwise.</returns>
        public bool ContainsFileOffset(uint fileOffset)
        {
            return FileOffset <= fileOffset && fileOffset < FileOffset + GetPhysicalSize();
        }

        /// <summary>
        /// Determines whether the provided virtual address falls within the section that the header describes. 
        /// </summary>
        /// <param name="rva">The virtual address to check.</param>
        /// <returns><c>true</c> if the virtual address falls within the section, <c>false</c> otherwise.</returns>
        public bool ContainsRva(uint rva)
        {
            return Rva <= rva && rva < Rva + GetVirtualSize();
        }

        /// <inheritdoc />
        public uint FileOffsetToRva(uint fileOffset)
        {
            if (!ContainsFileOffset(fileOffset))
                throw new ArgumentOutOfRangeException(nameof(fileOffset));
            return fileOffset - FileOffset + Rva;
        }

        /// <inheritdoc />
        public uint RvaToFileOffset(uint rva)
        {
            if (!ContainsRva(rva))
                throw new ArgumentOutOfRangeException(nameof(rva));
            return rva - Rva + FileOffset;
        }

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer) => Contents.Write(writer);
    }
}