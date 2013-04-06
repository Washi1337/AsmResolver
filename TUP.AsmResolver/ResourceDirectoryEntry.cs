using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents a directory entry in the resource directory.
    /// </summary>
    public class ResourceDirectoryEntry
    {
        PeImage image;
        uint offset;
        internal Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY rawEntry;
        string customName;
        
        internal ResourceDirectoryEntry(PeImage image, uint offset, Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY rawEntry, string customName)
        {
            this.image = image;
            this.offset = offset;
            this.rawEntry = rawEntry;
            this.customName = customName;
        }

        /// <summary>
        /// Gets the native name ID of the directory entry.
        /// </summary>
        public uint NameID { 
            get { return rawEntry.Name; }
            set
            {
                image.SetOffset(offset);
                image.Writer.Write(value);
                rawEntry.Name = value;
            }
        }
        /// <summary>
        /// Gets the resource type of the directory entry.
        /// </summary>
        public ResourceDirectoryType Type
        {
            get
            {
                if (NameID > 24)
                    return ResourceDirectoryType.CustomNamed;

                return (ResourceDirectoryType)NameID;
            }
        }
        /// <summary>
        /// Gets the name of the directory entry.
        /// </summary>
        public string Name
        {
            get
            {
                if (Type == ResourceDirectoryType.CustomNamed)
                    return customName;
                else
                    return Type.ToString();
            }
        }
        /// <summary>
        /// Gets the offset to the contents of this directoy entry. This offset is relative to the resource directory offset.
        /// </summary>
        public uint OffsetToData {
            get { return rawEntry.OffsetToData; }
            set
            {
                image.SetOffset(offset + sizeof(uint));
                image.Writer.Write(value);
                rawEntry.Name = value;
            }
        }
        /// <summary>
        /// Gets the directory (if available) that the directory entry is pointing to.
        /// </summary>
        public ResourceDirectory Directory
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets the data entry (if available) that the directory entry is pointing to.
        /// </summary>
        public ResourceDataEntry DataEntry
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets a value indicating the directory entry is pointing to a data entry instead of a directory.
        /// </summary>
        public bool IsEntryToData
        {
            get { return (OffsetToData >> 0x1F) == 0; }
        }
    }
}
