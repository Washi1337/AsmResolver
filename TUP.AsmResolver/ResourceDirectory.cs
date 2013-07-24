using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TUP.AsmResolver.PE;
using TUP.AsmResolver.PE.Readers;
namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents a directory of resources inside a Portable Executable.
    /// </summary>
    public class ResourceDirectory
    {
        private PeImage _image;
        internal Structures.IMAGE_RESOURCE_DIRECTORY _rawDirectory;
        private uint _offset;
        private uint _fileOffset;
        private ResourceDirectoryEntry[] _childEntries;
        private ResourcesReader _reader;

        internal ResourceDirectory(PeImage image, uint offset, ResourcesReader reader, ResourceDirectoryEntry parentEntry, PE.Structures.IMAGE_RESOURCE_DIRECTORY rawDirectory)
        {
            this._image = image;
            this.ParentEntry = parentEntry;
            this._offset = offset;
            this._fileOffset = offset + image.ParentAssembly._ntHeader.OptionalHeader.DataDirectories[(int)DataDirectoryName.Resource].TargetOffset.FileOffset;
            this._rawDirectory = rawDirectory;
            this._reader = reader;
        }


        /// <summary>
        /// Gets the parent directory entry of the directory.
        /// </summary>
        public ResourceDirectoryEntry ParentEntry
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets the child directory entries of the directory.
        /// </summary>
        public ResourceDirectoryEntry[] ChildEntries
        {
            get
            {
                if (_childEntries == null)
                {
                    _childEntries = _reader.ReadChildEntries(_offset + (uint)Marshal.SizeOf(_rawDirectory), _rawDirectory.NumberOfIdEntries + _rawDirectory.NumberOfNamedEntries);
                    if (_childEntries == null)
                        _childEntries = new ResourceDirectoryEntry[0];
                }
                
                return _childEntries;
            }
        }

        /// <summary>
        /// Gets a value indicating the characteristics of the directory. This value is ignored.
        /// </summary>
        [Obsolete("This value is ignored by the windows loader.")]
        public uint Characteristics
        {
            get { return _rawDirectory.Characteristics; }
            set
            {
                _image.SetOffset(_fileOffset);
                _image.Writer.Write(value);
                _rawDirectory.Characteristics = value;
            }
        }
        /// <summary>
        /// Gets a value indicating the time stamp of the directory. This value is ignored.
        /// </summary>
        [Obsolete("This value is ignored by the windows loader.")]
        public uint TimeStamp
        {
            get { return _rawDirectory.TimeDateStamp; }
            set
            {
                _image.SetOffset(_fileOffset + Structures.DataOffsets[typeof(Structures.IMAGE_RESOURCE_DIRECTORY)][1]);
                _image.Writer.Write(value);
                _rawDirectory.TimeDateStamp = value;
            }
        }
        /// <summary>
        /// Gets the mayor version number. This value is ignored
        /// </summary>
        [Obsolete("This value is ignored by the windows loader.")]
        public ushort MayorVersion
        {
            get { return _rawDirectory.MajorVersion; }
            set
            {
                _image.SetOffset(_fileOffset + Structures.DataOffsets[typeof(Structures.IMAGE_RESOURCE_DIRECTORY)][2]);
                _image.Writer.Write(value);
                _rawDirectory.MajorVersion = value;
            }
        }
        /// <summary>
        /// Gets the minor version number. This value is ignored
        /// </summary>
        [Obsolete("This value is ignored by the windows loader.")]
        public ushort MinorVersion
        {
            get { return _rawDirectory.MinorVersion; }
            set
            {
                _image.SetOffset(_fileOffset + Structures.DataOffsets[typeof(Structures.IMAGE_RESOURCE_DIRECTORY)][3]);
                _image.Writer.Write(value);
                _rawDirectory.MinorVersion = value;
            }
        }
        /// <summary>
        /// Gets a value indicating the amount of named directory entries.
        /// </summary>
        public ushort NumberOfNamedEntries
        {
            get { return _rawDirectory.NumberOfNamedEntries; }
            set
            {
                _image.SetOffset(_fileOffset + Structures.DataOffsets[typeof(Structures.IMAGE_RESOURCE_DIRECTORY)][4]);
                _image.Writer.Write(value);
                _rawDirectory.NumberOfNamedEntries = value;
            }
        }
        /// <summary>
        /// Gets a value indicating the amount of directory entries that holds an Id number.
        /// </summary>
        public ushort NumberOfIdEntries
        {
            get { return _rawDirectory.NumberOfIdEntries; }
            set
            {
                _image.SetOffset(_fileOffset + Structures.DataOffsets[typeof(Structures.IMAGE_RESOURCE_DIRECTORY)][5]);
                _image.Writer.Write(value);
                _rawDirectory.NumberOfIdEntries = value;
            }
        }
        /// <summary>
        /// Gets a value indicating the directory is the root directory of the entire resource data directory.
        /// </summary>
        public bool IsRootDirectory
        {
            get { return ParentEntry == null; }
        }

    }
}
