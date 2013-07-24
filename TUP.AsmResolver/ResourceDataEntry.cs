using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents a data entry of a resource directory entry.
    /// </summary>
    public class ResourceDataEntry
    {
        private PeImage _image;
        private uint _offset;
        private uint _targetOffset;
        internal Structures.IMAGE_RESOURCE_DATA_ENTRY _rawDataEntry;

        internal ResourceDataEntry(PeImage image, uint offset, ResourceDirectoryEntry parentEntry, Structures.IMAGE_RESOURCE_DATA_ENTRY rawDataEntry)
        {
            this._image = image;
            this._offset = offset;
            this.ParentEntry = parentEntry;
            this._rawDataEntry = rawDataEntry;

            Section resourceSection = Section.GetSectionByRva(image.ParentAssembly, image.ParentAssembly._ntHeader.OptionalHeader.DataDirectories[(int)DataDirectoryName.Resource].TargetOffset.Rva);
            _targetOffset = OffsetToData - resourceSection.RVA + resourceSection.RawOffset;
        }
        /// <summary>
        /// Gets the parent directory entry of the data entry.
        /// </summary>
        public ResourceDirectoryEntry ParentEntry
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets the offset to the contents of this data entry. This offset is relative to the resource directory offset.
        /// </summary>
        public uint OffsetToData
        {
            get { return _rawDataEntry.OffsetToData; }
            set
            {
                _image.SetOffset(_offset);
                _image.Writer.Write(value);
                _rawDataEntry.OffsetToData = value;
            }
        }
        /// <summary>
        /// Gets the size in bytes of the data contents.
        /// </summary>
        public uint Size
        {
            get { return _rawDataEntry.Size; }
            set
            {
                _image.SetOffset(Structures.DataOffsets[typeof(Structures.IMAGE_RESOURCE_DATA_ENTRY)][1]);
                _image.Writer.Write(value);
                _rawDataEntry.Size = value;
            }
        }
        /// <summary>
        /// Gets the code page of the data entry.
        /// </summary>
        public uint CodePage
        {
            get { return _rawDataEntry.CodePage; }
            set
            {
                _image.SetOffset(Structures.DataOffsets[typeof(Structures.IMAGE_RESOURCE_DATA_ENTRY)][2]);
                _image.Writer.Write(value);
                _rawDataEntry.CodePage = value;
            }
        }
        /// <summary>
        /// Reads the data as a stream with a buffer size of 4096 bytes.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            return GetStream(0x1000);
        }
        /// <summary>
        /// Reads the data as a stream with a specified buffer size.
        /// </summary>
        /// <param name="buffersize">The buffer size to be used to read the data.</param>
        /// <returns></returns>
        public Stream GetStream(int buffersize)
        {
            _image.SetOffset(_targetOffset);
            return _image.ReadStream((int)Size, buffersize);
        }
        /// <summary>
        /// Reads the data and returns it in byte array format.
        /// </summary>
        /// <returns></returns>
        public byte[] GetContents()
        {
            return _image.ReadBytes(_targetOffset, (int)Size);
        }
    }
}
