using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents a data directory in the executable.
    /// </summary>
    public class DataDirectory
    {
        internal DataDirectory(DataDirectoryName name, Section[] assemblySections, uint offset, uint rva, uint size)
        {
            try
            {
                this.name = name;
                if (rva == 0)
                {
                    offset = 0;
                    targetOffset = new Offset(0, 0, 0, ASM.OperandType.Normal);
                    size = 0;
                }
                else
                {
                    this.headerOffset = offset;

                    targetSection = Section.GetSectionByRva(assemblySections, rva);
                    this.size = size;
                    this.targetOffset = Offset.FromRva(rva, assemblySections[0].ParentAssembly);
                }
            }
            catch (Exception ex)
            {
            }
        }
        internal DataDirectory(Section targetSection, uint headerOffset, uint rva, uint size)
        {
            this.headerOffset = headerOffset;
            this.size = size;
            if (rva != 0)
            {
                OffsetConverter converter = new OffsetConverter(targetSection);
                this.targetOffset = Offset.FromRva(rva, targetSection.ParentAssembly);
                this.targetSection = targetSection;
            }
        }
        internal uint headerOffset;
        internal Offset targetOffset;
        internal uint size;
        internal Section targetSection;
        internal DataDirectoryName name;

        /// <summary>
        /// Gets the offset the data directory is located.
        /// </summary>
        public uint HeaderOffset
        {
            get { return headerOffset; }
        }
        /// <summary>
        /// Gets the offset the data directory is pointing at.
        /// </summary>
        public Offset TargetOffset
        {
            get { return targetOffset; }
        }
        /// <summary>
        /// Gets the size of the data directory.
        /// </summary>
        public uint Size
        {
            get { return size; }
        }
        /// <summary>
        /// Gets the section the data directory is pointing at.
        /// </summary>
        public Section Section
        {
            get { return targetSection; }
        }
        /// <summary>
        /// Gets the name of the data directory.
        /// </summary>
        public DataDirectoryName Name
        {
            get { return name; }
        }
    }
}
