using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.IO;
using System.Drawing;

namespace TUP.AsmResolver.PE.Readers
{
    internal class ResourcesReader
    {
        NTHeader header;
        PeImage image;
        Stream stream;
        BinaryReader reader;
        internal ResourceDirectory rootDirectory;
        DataDirectory resourceDirectory;
        internal ResourcesReader(NTHeader header)
        {
            this.header = header;
            this.image = header._assembly._peImage;
            resourceDirectory = header.OptionalHeader.DataDirectories[(int)DataDirectoryName.Resource];
            if (header._assembly._peImage.TrySetOffset(resourceDirectory.TargetOffset.FileOffset))
            {
                stream = header._assembly._peImage.ReadStream((int)resourceDirectory.Size);
                reader = new BinaryReader(stream);
                ReadRootDirectory();
            }
        }

        internal void ReadRootDirectory()
        {
            if (resourceDirectory.TargetOffset.FileOffset != 0)
            {
                rootDirectory = ReadDirectory(0, null);
            }
        }

        internal ResourceDirectoryEntry ReadDirectoryEntry(uint offset)
        {
            var rawEntry = ASMGlobals.ReadStructureFromReader<Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY>(reader);
            string customName = string.Empty;
            ResourceDirectoryEntry resourceEntry = new ResourceDirectoryEntry(image, offset, rawEntry, customName); 

            return resourceEntry;
        }

        internal ResourceDirectory ReadDirectory(uint offset, ResourceDirectoryEntry entry)
        {
            if (TrySetOffset(offset))
            {
                var rawDirectory = ASMGlobals.ReadStructureFromReader<Structures.IMAGE_RESOURCE_DIRECTORY>(reader);

                return new ResourceDirectory(image, offset, this, entry, rawDirectory);
            }
            return null;
        }

        internal ResourceDirectoryEntry[] ReadChildEntries(uint offset, int count)
        {
            if (TrySetOffset(offset))
            {
                ResourceDirectoryEntry[] entries = ConstructChildEntries(count);
                FillChildEntries(ref entries);
                return entries;
            }
            return null;
        }

        internal ResourceDirectoryEntry[] ConstructChildEntries(int count)
        {
            ResourceDirectoryEntry[] entries = new ResourceDirectoryEntry[count];
            for (int i = 0; i < count; i++)
                entries[i] = ReadDirectoryEntry((uint)stream.Position);
            return entries;
        }

        internal void FillChildEntries(ref ResourceDirectoryEntry[] entries)
        {
            for (int i = 0; i < entries.Length; i++)
            {

                if (!entries[i].IsEntryToData)
                {
                    entries[i].Directory = ReadDirectory(entries[i].OffsetToData - 0x80000000, entries[i]);
                }
                else
                {
                    entries[i].DataEntry = ReadDataEntry(entries[i].OffsetToData, entries[i]);
                }
            }
        }

        internal ResourceDataEntry ReadDataEntry(uint offset, ResourceDirectoryEntry entry)
        {
            if (TrySetOffset(offset))
            {
                var rawDataEntry = ASMGlobals.ReadStructureFromReader<Structures.IMAGE_RESOURCE_DATA_ENTRY>(reader);
                return new ResourceDataEntry(image, resourceDirectory.TargetOffset.FileOffset + offset, entry, rawDataEntry);
            }
            return null;
        }
        
        internal bool TrySetOffset(uint offset)
        {
            try
            {
                if (offset < 0 || offset > stream.Length)
                    return false;
                stream.Seek(offset, SeekOrigin.Begin);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
