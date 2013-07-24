using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TUP.AsmResolver.PE;
using TUP.AsmResolver.PE.Readers;
using TUP.AsmResolver.ASM;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents a section of a portable executable.
    /// </summary>
    public class Section : IImageProvider
    {    
        private Structures.IMAGE_SECTION_HEADER _rawHeader;
        private byte[] _contents;

        public Section(string name, uint offset, byte[] contents)
        {
            this.HasImage = false;
            _rawHeader = new Structures.IMAGE_SECTION_HEADER();
            this.Name = name;
            this.RawOffset = offset;
            this.RawSize = (uint)contents.Length;
            this._contents = contents;
        }

        internal Section(Win32Assembly assembly,
            uint headeroffset, 
            Structures.IMAGE_SECTION_HEADER rawHeader)
        {
            this._rawHeader = rawHeader;
            this.headeroffset = headeroffset;
            this.assembly = assembly;
            this.HasImage = true;
        }

        #region Variables
        /// <summary>
        /// The maximum length a section name can have.
        /// </summary>
        public const int MaxSectionNameLength = 8;

        uint headeroffset;
        Win32Assembly assembly;
        #endregion

        #region Properties

        public bool HasImage
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the raw offset of the section header.
        /// </summary>
        public uint RawHeaderOffset
        {
            get
            {
                return headeroffset;
            }
        }
        /// <summary>
        /// Gets or sets the flags that describes the section.
        /// </summary>
        public SectionFlags Flags
        {
            get { return (SectionFlags)_rawHeader.Characteristics; }
            set
            {
                _rawHeader.Characteristics = (uint)value;
                if (HasImage)
                {
                    int targetoffset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_SECTION_HEADER)][9];
                    assembly._peImage.SetOffset(targetoffset);
                    assembly._peImage.Writer.Write(_rawHeader.Characteristics);
                }
            }
        }
        /// <summary>
        /// Gets or sets the name of the section.
        /// </summary>
        public string Name
        {
            get { return _rawHeader.Name; }
            set
            {
                if (value.Length > MaxSectionNameLength)
                    throw new ArgumentException("The string cannot be larger than the MaxSectionNameLength.", "value");

                if (HasImage)
                {
                    List<byte> bytes = Encoding.ASCII.GetBytes(value).ToList();

                    while (bytes.Count < MaxSectionNameLength)
                        bytes.Add(0);

                    int targetoffset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_SECTION_HEADER)][0];
                    assembly._peImage.SetOffset(targetoffset);
                    assembly._peImage.Writer.Write(bytes.ToArray());
                }
                _rawHeader.Name = value;
            }
        }
        /// <summary>
        /// Gets the raw offset of the section.
        /// </summary>
        public uint RawOffset
        {
            get { return _rawHeader.PointerToRawData; }
            set
            {
                _rawHeader.PointerToRawData = value;
                if (HasImage)
                {
                    assembly._peImage.SetOffset(headeroffset + Structures.DataOffsets[typeof(Structures.IMAGE_SECTION_HEADER)][4]);
                    assembly._peImage.Writer.Write(value);
                }
            }
        }
        /// <summary>
        /// Gets the raw size of the section.
        /// </summary>
        public uint RawSize
        {
            get { return _rawHeader.SizeOfRawData; }
            set
            {
                _rawHeader.SizeOfRawData = value;
                if (HasImage)
                {
                    assembly._peImage.SetOffset(headeroffset + Structures.DataOffsets[typeof(Structures.IMAGE_SECTION_HEADER)][3]);
                    assembly._peImage.Writer.Write(value);
                }
            }
        }
        /// <summary>
        /// Gets the virtual offset of the section.
        /// </summary>
        public uint RVA
        {
            get { return _rawHeader.VirtualAddress; }
            set
            {
                _rawHeader.VirtualAddress = value;
                if (HasImage)
                {
                    assembly._peImage.SetOffset(headeroffset + Structures.DataOffsets[typeof(Structures.IMAGE_SECTION_HEADER)][2]);
                    assembly._peImage.Writer.Write(value);
                }
            }
        }
        /// <summary>
        /// Gets the virtual size of the section.
        /// </summary>
        public uint VirtualSize
        {
            get { return _rawHeader.VirtualSize; }
            set
            {
                _rawHeader.VirtualSize = value;
                if (HasImage)
                {
                    assembly._peImage.SetOffset(headeroffset + Structures.DataOffsets[typeof(Structures.IMAGE_SECTION_HEADER)][1]);
                    assembly._peImage.Writer.Write(value);
                }
            }
        }
        /// <summary>
        /// Gets the value indicating whether the section contents can be executed in memory.
        /// </summary>
        public bool CanExecute
        {
            get
            {
                return Flags.HasFlag(SectionFlags.MemoryExecute);
            }
        }
        /// <summary>
        ///Gets the value indicating whether the section can be written in memory.
        /// </summary>
        public bool CanWrite
        {
            get
            {
                return Flags.HasFlag(SectionFlags.MemoryWrite);
            }
        }
        /// <summary>
        /// Gets the value indicating whether the section can be read in memory.
        /// </summary>
        public bool CanRead
        {
            get
            {
                return Flags.HasFlag(SectionFlags.MemoryRead);
            }
        }
        /// <summary>
        /// Gets the value indicating whether the section can be shared in memory.
        /// </summary>
        public bool CanShared
        {
            get
            {
                return Flags.HasFlag(SectionFlags.MemoryShared);
            }
        }
        /// <summary>
        /// Gets the value indicating whether the section contains executable code.
        /// </summary>
        public bool ContainsCode
        {
            get
            {
                return Flags.HasFlag(SectionFlags.ContentCode);
            }
        }
        /// <summary>
        /// Gets the parent assembly container of this NT header.
        /// </summary>
        public Win32Assembly ParentAssembly
        {
            get
            {
                return assembly;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the raw bytes of the section.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            if (!HasImage)
                return _contents;
            return assembly.Image.ReadBytes(RawOffset, (int)RawSize);
        }
        /// <summary>
        /// Gets the raw bytes of the section given by an offset and size
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public byte[] GetBytes(uint offset, int size)
        {
            return assembly.Image.ReadBytes(offset, size);
        }
        /// <summary>
        /// Disassembles the section to 32 bit assembly instructions.
        /// </summary>
        public InstructionCollection Disassemble()
        {
            return assembly._disassembler.Disassemble(RawOffset, RawSize);
        }
        /// <summary>
        /// Disassembles the section from the start with the given length.
        /// </summary>
        /// <param name="bytelength">The amount of bytes to read.</param>
        public InstructionCollection Disassemble(uint bytelength)
        {
            if (bytelength > RawSize)
                throw new ArgumentOutOfRangeException("The specified length is bigger than the size of the section.");
            return assembly._disassembler.Disassemble(RawOffset, bytelength);
        }
        /// <summary>
        /// Disassembles the section from the given start offset and the given length.
        /// </summary>
        /// <param name="startoffset">The offset to start reading.</param>
        /// <param name="bytelength">The amount of bytes to read.</param>
        public InstructionCollection Disassemble(uint startoffset, uint bytelength)
        {
            if (startoffset < RawOffset | startoffset > RawOffset + RawSize)
                throw new ArgumentOutOfRangeException("startoffset", "The specified rawStartOffset targetoffset is outside the section.");
            if (startoffset + bytelength > RawOffset + RawSize)
                throw new ArgumentOutOfRangeException("bytelength", "The specified length ends outside the section.");
            return assembly._disassembler.Disassemble(startoffset, bytelength);
        }

        /// <summary>
        /// Converts a relative virtual address to a file offset
        /// </summary>
        /// <param name="rva"></param>
        /// <returns></returns>
        public uint RVAToFileOffset(uint rva)
        {
            return (uint)(rva - this.RVA + RawOffset);
        }
        
        /// <summary>
        /// Returns true if the specified virtual offset is located in the current section.
        /// </summary>
        /// <param name="Rva">The rva to check.</param>
        /// <returns></returns>
        public bool ContainsRva(uint Rva)
        {
            int endoffset = (int)( this.RVA + this.VirtualSize);
            return ((Rva >= this.RVA) & (Rva <= endoffset));
        }
        /// <summary>
        /// Returns true if the specified raw offset is located in the current section.
        /// </summary>
        /// <param name="rawoffset">The raw offset to check.</param>
        /// <returns></returns>
        public bool ContainsRawOffset(uint rawoffset)
        {
            return ((rawoffset >= this.RawOffset) & (rawoffset < (this.RawOffset + this.RawSize)));
        }

        
        #endregion

        #region Static Methods
        /// <summary>
        /// Gets the section of an assembly by it's name.
        /// </summary>
        /// <param name="assembly">The assembly to search in.</param>
        /// <param name="sectionname">The section name to search for.</param>
        /// <returns></returns>
        public static Section GetSectionByName(Win32Assembly assembly, string sectionname)
        {
            return GetSectionByName(assembly.NTHeader.Sections, sectionname);
        }
        /// <summary>
        /// Gets the section of a list of sections by it's name.
        /// </summary>
        /// <param name="sections">The section list to search in.</param>
        /// <param name="sectionname">The section name to search for.</param>
        /// <returns></returns>
        public static Section GetSectionByName(IEnumerable<Section> sections, string sectionname)
        {
            foreach (Section s in sections)
            {
                if (s.Name == sectionname) return s;
            }
            return null;
        }

        /// <summary>
        /// Gets the section of an assembly by it's raw offset.
        /// </summary>
        /// <param name="assembly">The assembly to search in.</param>
        /// <param name="rawoffset">The raw offset to search for.</param>
        /// <returns></returns>
        public static Section GetSectionByFileOffset(Win32Assembly assembly, uint rawoffset)
        {
            return GetSectionByFileOffset(assembly._ntHeader.Sections, rawoffset);
        }
        /// <summary>
        /// Gets the section of a list of sections by it's raw offset
        /// </summary>
        /// <param name="sections">The section list to search in.</param>
        /// <param name="rawoffset">The raw offset to search for.</param>
        /// <returns></returns>
        public static Section GetSectionByFileOffset(IEnumerable<Section> sections, uint rawoffset)
        {
            foreach (Section s in sections)
            {
                if (s.ContainsRawOffset(rawoffset)) return s;
            }
            return null;
        }

        /// <summary>
        /// Gets the section of an assembly by it's virtual offset.
        /// </summary>
        /// <param name="assembly">The assembly to search in.</param>
        /// <param name="va">The virtual offset to search for.</param>
        /// <returns></returns>
        public static Section GetSectionByRva(Win32Assembly assembly, uint va)
        {
            return GetSectionByRva(assembly._ntHeader.Sections, va);
        }
        /// <summary>
        /// Gets the section of a list of sections by it's virtual offset
        /// </summary>
        /// <param name="sections">The section list to search in.</param>
        /// <param name="virtualoffset">The virtual offset to search for.</param>
        /// <returns></returns>
        public static Section GetSectionByRva(IEnumerable<Section> sections, uint virtualoffset)
        {
            foreach (Section s in sections)
            {
                if (s.ContainsRva(virtualoffset)) return s;
            }
            return null;
        }

        /// <summary>
        /// Gets the first section of an assembly that contains the specified flag
        /// </summary>
        /// <param name="assembly">The assembly to search in.</param>
        /// <param name="characteristics">The flag to search for.</param>
        /// <returns></returns>
        public static Section GetFirstSectionByFlag(Win32Assembly assembly, SectionFlags characteristics)
        {
            return GetFirstSectionByFlag(assembly._ntHeader.Sections, characteristics);
        }
        /// <summary>
        /// Gets the first section of a list of sections that contains the specified flag
        /// </summary>
        /// <param name="sections">The section list to search in.</param>
        /// <param name="characteristics">The flag to search for.</param>
        /// <returns></returns>
        public static Section GetFirstSectionByFlag(IEnumerable<Section> sections, SectionFlags characteristics)
        {
            foreach (Section s in sections)
            {
                if (s.Flags.HasFlag(characteristics)) return s;
            }
            return null;
        }

        /// <summary>
        /// Gets the last section of an assembly that contains the specified flag
        /// </summary>
        /// <param name="assembly">The assembly to search in.</param>
        /// <param name="characteristics">The flag to search for.</param>
        /// <returns></returns>
        public static Section GetLastSectionByFlag(Win32Assembly assembly, SectionFlags characteristics)
        {
            return GetLastSectionByFlag(assembly._ntHeader.Sections, characteristics);
        }
        /// <summary>
        /// Gets the last section of a list of sections that contains the specified flag
        /// </summary>
        /// <param name="sections">The section list to search in.</param>
        /// <param name="characteristics">The flag to search for.</param>
        /// <returns></returns>
        public static Section GetLastSectionByFlag(IEnumerable<Section> sections, SectionFlags characteristics)
        {
            Section sec = null;
            foreach (Section s in sections)
            {
                if (s.Flags.HasFlag(characteristics)) sec = s;
            }
            return sec;
        }
        #endregion

    }
}
