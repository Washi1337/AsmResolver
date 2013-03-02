using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents a data directory in the executable.
    /// </summary>
    public class DataDirectory
    {
        internal DataDirectory(DataDirectoryName name, Section[] assemblySections, uint offset, Structures.IMAGE_DATA_DIRECTORY rawDataDir)
        {
            
            this.rawDataDir = rawDataDir;
            this.name = name;
            if (rawDataDir.RVA == 0)
            {
                targetOffset = new Offset(0, 0, 0, ASM.OperandType.Normal);
            }
            else
            {
                this.headerOffset = offset;

                targetSection = Section.GetSectionByRva(assemblySections, rawDataDir.RVA);
                if (targetSection == null)
                    this.TargetOffset = new Offset(0, rawDataDir.RVA, 0, ASM.OperandType.Normal);
                else
                    this.targetOffset = Offset.FromRva(rawDataDir.RVA, assemblySections[0].ParentAssembly);
            }
            
        }
        internal DataDirectory(DataDirectoryName name, Section targetSection, uint headerOffset, Structures.IMAGE_DATA_DIRECTORY rawDataDir)
        {
            this.name = name;
            this.headerOffset = headerOffset;
            this.rawDataDir = rawDataDir;
            if (rawDataDir.RVA == 0)
            {
                targetOffset = new Offset(0, 0, 0, ASM.OperandType.Normal);
            }
            else
            {
                OffsetConverter converter = new OffsetConverter(targetSection);
                this.targetOffset = Offset.FromRva(rawDataDir.RVA, targetSection.ParentAssembly);
                this.targetSection = targetSection;
            }
        }
        internal uint headerOffset;
        internal Offset targetOffset;
        internal Structures.IMAGE_DATA_DIRECTORY rawDataDir;
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
            set
            {
                rawDataDir.RVA = value.Rva;
                targetOffset = value;
            }
        }
        /// <summary>
        /// Gets the size of the data directory.
        /// </summary>
        public uint Size
        {
            get { return rawDataDir.Size; }
            set { rawDataDir.Size = value; }
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
        /// <summary>
        /// Gets the contents of the data directory.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            if (Section == null)
                return null;

            return Section.GetBytes(TargetOffset.FileOffset, (int)Size);
        }
    }
}
