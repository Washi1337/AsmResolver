using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE;
namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents a directory of resources inside a Portable Executable.
    /// </summary>
    public class ResourceDirectory
    {
        PeImage image;
        Structures.IMAGE_RESOURCE_DIRECTORY rawDirectory;
        uint offset;
        internal ResourceDirectory(PeImage image, uint offset, ResourceDirectoryEntry parentEntry, ResourceDirectoryEntry[] childEntries, PE.Structures.IMAGE_RESOURCE_DIRECTORY rawDirectory)
        {
            this.image = image;
            this.ParentEntry = parentEntry;
            this.ChildEntries = childEntries;
            this.offset = offset;
            this.rawDirectory = rawDirectory;
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
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating the characteristics of the directory. This value is ignored.
        /// </summary>
        [Obsolete("This value is ignored by the windows loader.")]
        public uint Characteristics
        {
            get { return rawDirectory.Characteristics; }
            set
            {
                image.SetOffset(offset);
                image.writer.Write(value);
                rawDirectory.Characteristics = value;
            }
        }
        /// <summary>
        /// Gets a value indicating the time stamp of the directory. This value is ignored.
        /// </summary>
        [Obsolete("This value is ignored by the windows loader.")]
        public uint TimeStamp
        {
            get { return rawDirectory.TimeDateStamp; }
            set
            {
                image.SetOffset(offset + Structures.DataOffsets[typeof(Structures.IMAGE_RESOURCE_DIRECTORY)][1]);
                image.writer.Write(value);
                rawDirectory.TimeDateStamp = value;
            }
        }
        /// <summary>
        /// Gets the mayor version number. This value is ignored
        /// </summary>
        [Obsolete("This value is ignored by the windows loader.")]
        public ushort MayorVersion
        {
            get { return rawDirectory.MajorVersion; }
            set
            {
                image.SetOffset(offset + Structures.DataOffsets[typeof(Structures.IMAGE_RESOURCE_DIRECTORY)][2]);
                image.writer.Write(value);
                rawDirectory.MajorVersion = value;
            }
        }
        /// <summary>
        /// Gets the minor version number. This value is ignored
        /// </summary>
        [Obsolete("This value is ignored by the windows loader.")]
        public ushort MinorVersion
        {
            get { return rawDirectory.MinorVersion; }
            set
            {
                image.SetOffset(offset + Structures.DataOffsets[typeof(Structures.IMAGE_RESOURCE_DIRECTORY)][3]);
                image.writer.Write(value);
                rawDirectory.MinorVersion = value;
            }
        }
        /// <summary>
        /// Gets a value indicating the amount of named directory entries.
        /// </summary>
        public ushort NumberOfNamedEntries
        {
            get { return rawDirectory.NumberOfNamedEntries; }
            set
            {
                image.SetOffset(offset + Structures.DataOffsets[typeof(Structures.IMAGE_RESOURCE_DIRECTORY)][4]);
                image.writer.Write(value);
                rawDirectory.NumberOfNamedEntries = value;
            }
        }
        /// <summary>
        /// Gets a value indicating the amount of directory entries that holds an Id number.
        /// </summary>
        public ushort NumberOfIdEntries
        {
            get { return rawDirectory.NumberOfIdEntries; }
            set
            {
                image.SetOffset(offset + Structures.DataOffsets[typeof(Structures.IMAGE_RESOURCE_DIRECTORY)][5]);
                image.writer.Write(value);
                rawDirectory.NumberOfIdEntries = value;
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
