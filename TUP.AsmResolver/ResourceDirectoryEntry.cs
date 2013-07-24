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
        private PeImage _image;
        private uint _offset;
        private Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY _rawEntry;
        private string _customName;
        
        internal ResourceDirectoryEntry(PeImage image, uint offset, Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY rawEntry, string customName)
        {
            this._image = image;
            this._offset = offset;
            this._rawEntry = rawEntry;
            this._customName = customName;
        }

        /// <summary>
        /// Gets the native name ID of the directory entry.
        /// </summary>
        public uint NameID { 
            get { return _rawEntry.Name; }
            set
            {
                _image.SetOffset(_offset);
                _image.Writer.Write(value);
                _rawEntry.Name = value;
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
                    return _customName;
                else
                    return Type.ToString();
            }
        }
        /// <summary>
        /// Gets the offset to the contents of this directoy entry. This offset is relative to the resource directory offset.
        /// </summary>
        public uint OffsetToData {
            get { return _rawEntry.OffsetToData; }
            set
            {
                _image.SetOffset(_offset + sizeof(uint));
                _image.Writer.Write(value);
                _rawEntry.Name = value;
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
