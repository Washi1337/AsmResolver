using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.File
{
    /// <summary>
    /// Represents a single section in a portable executable (PE) file.
    /// </summary>
    public class PESection : ISegment
    {
        /// <summary>
        /// Creates a new section.
        /// </summary>
        /// <param name="header">The header to associate to the section.</param>
        /// <param name="contents">The contents of the section.</param>
        public PESection(SectionHeader header, IReadableSegment contents)
        {
            Header = header;
            Contents = contents;
        }
        
        /// <summary>
        /// Gets or sets the header associated to the section.
        /// </summary>
        public SectionHeader Header
        {
            get;
        }

        /// <summary>
        /// Gets or sets the contents of the section.
        /// </summary>
        public IReadableSegment Contents
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint FileOffset => Contents?.FileOffset ?? Header.PointerToRawData;

        /// <inheritdoc />
        public uint Rva => Contents?.Rva ?? Header.VirtualAddress;

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            Contents.UpdateOffsets(newFileOffset, newRva);
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            return Contents.GetPhysicalSize();
        }

        /// <inheritdoc />
        public uint GetVirtualSize()
        {
            return Contents.GetVirtualSize();
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            Contents.Write(writer);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Header.Name;
        }
        
    }
}