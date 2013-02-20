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
        PeImage image;
        uint offset;
        uint targetOffset;
        Structures.IMAGE_RESOURCE_DATA_ENTRY rawDataEntry;

        internal ResourceDataEntry(PeImage image, uint offset, ResourceDirectoryEntry parentEntry, Structures.IMAGE_RESOURCE_DATA_ENTRY rawDataEntry)
        {
            this.image = image;
            this.offset = offset;
            this.ParentEntry = parentEntry;
            this.rawDataEntry = rawDataEntry;

            Section resourceSection = Section.GetSectionByRva(image.assembly, image.assembly.ntheader.OptionalHeader.DataDirectories[(int)DataDirectoryName.Resource].TargetOffset.Rva);
            targetOffset = OffsetToData - resourceSection.RVA + resourceSection.RawOffset;
        }

        public ResourceDirectoryEntry ParentEntry
        {
            get;
            private set;
        }

        public uint OffsetToData
        {
            get { return rawDataEntry.OffsetToData; }
            set
            {
                image.SetOffset(offset);
                image.writer.Write(value);
                rawDataEntry.OffsetToData = value;
            }
        }
        public uint Size
        {
            get { return rawDataEntry.Size; }
            set
            {
                image.SetOffset(Structures.DataOffsets[typeof(Structures.IMAGE_RESOURCE_DATA_ENTRY)][1]);
                image.writer.Write(value);
                rawDataEntry.Size = value;
            }
        }
        public uint CodePage
        {
            get { return rawDataEntry.CodePage; }
            set
            {
                image.SetOffset(Structures.DataOffsets[typeof(Structures.IMAGE_RESOURCE_DATA_ENTRY)][2]);
                image.writer.Write(value);
                rawDataEntry.CodePage = value;
            }
        }

        public Stream GetStream()
        {
            image.SetOffset(targetOffset);
            return image.ReadStream((int)Size, 0x1000);
        }
        public byte[] GetContents()
        {
            return image.ReadBytes(targetOffset, (int)Size);
        }
    }
}
