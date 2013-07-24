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
            
            this._rawDataDir = rawDataDir;
            this._name = name;
            if (rawDataDir.RVA == 0)
            {
                _targetOffset = new Offset(0, 0, 0);
            }
            else
            {
                this._headerOffset = offset;

                _targetSection = Section.GetSectionByRva(assemblySections, rawDataDir.RVA);
                if (_targetSection == null)
                    this.TargetOffset = new Offset(0, rawDataDir.RVA, 0);
                else
                    this._targetOffset = Offset.FromRva(rawDataDir.RVA, assemblySections[0].ParentAssembly);
            }
            
        }

        internal DataDirectory(DataDirectoryName name, Section targetSection, uint headerOffset, Structures.IMAGE_DATA_DIRECTORY rawDataDir)
        {
            this._name = name;
            this._headerOffset = headerOffset;
            this._rawDataDir = rawDataDir;
            if (rawDataDir.RVA == 0 || targetSection == null)
            {
                _targetOffset = new Offset(0, 0, 0);
            }
            else
            {
                this._targetOffset = Offset.FromRva(rawDataDir.RVA, targetSection.ParentAssembly);
                this._targetSection = targetSection;
            }
        }

        internal uint _headerOffset;
        internal Offset _targetOffset;
        internal Structures.IMAGE_DATA_DIRECTORY _rawDataDir;
        internal Section _targetSection;
        internal DataDirectoryName _name;

        /// <summary>
        /// Gets the offset the data directory is located.
        /// </summary>
        public uint HeaderOffset
        {
            get { return _headerOffset; }
        }
        /// <summary>
        /// Gets the offset the data directory is pointing at.
        /// </summary>
        public Offset TargetOffset
        {
            get { return _targetOffset; }
            set
            {
                _rawDataDir.RVA = value.Rva;
                _targetOffset = value;
            }
        }
        /// <summary>
        /// Gets the size of the data directory.
        /// </summary>
        public uint Size
        {
            get { return _rawDataDir.Size; }
            set { _rawDataDir.Size = value; }
        }
        /// <summary>
        /// Gets the section the data directory is pointing at.
        /// </summary>
        public Section Section
        {
            get { return _targetSection; }
        }
        /// <summary>
        /// Gets the name of the data directory.
        /// </summary>
        public DataDirectoryName Name
        {
            get { return _name; }
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
