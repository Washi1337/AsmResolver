using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.PE.Writers
{
    internal class ResourceWriter : IWriterTask 
    {
        DataDirectory resourceDirectory;

        internal ResourceWriter(PEWriter writer)
        {
            Writer = writer;
            resourceDirectory = Writer.OriginalAssembly.NTHeader.OptionalHeader.DataDirectories[(int)DataDirectoryName.Resource];

        }

        public PEWriter Writer
        {
            get;
            private set;
        }
        
        public void RunProcedure()
        {
            if (Writer.Parameters.RebuildResources && resourceDirectory.TargetOffset.FileOffset != 0)
            {
                Writer.MoveToOffset(resourceDirectory.TargetOffset.FileOffset);
                WriteDirectory(Writer.OriginalAssembly.RootResourceDirectory);
            }
        }
        private void WriteDirectoryEntry(ResourceDirectoryEntry entry)
        {
            if (entry.IsEntryToData)
                WriteDataEntry(entry.DataEntry);
            else
                WriteDirectory(entry.Directory);
        }
        private void WriteDirectory(ResourceDirectory directory)
        {
            Writer.WriteStructure<Structures.IMAGE_RESOURCE_DIRECTORY>(directory.rawDirectory);
            foreach (ResourceDirectoryEntry entry in directory.ChildEntries)
            {
                Writer.WriteStructure<Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY>(entry.rawEntry);
            }
            foreach (ResourceDirectoryEntry entry in directory.ChildEntries)
            {
                uint offsetToData = entry.OffsetToData;
                if (offsetToData >= 0x80000000)
                    offsetToData -= 0x80000000;

                Writer.MoveToOffset(offsetToData + resourceDirectory.TargetOffset.FileOffset);
                WriteDirectoryEntry(entry);
            }
        }
        private void WriteDataEntry(ResourceDataEntry entry)
        {  
            Writer.WriteStructure<Structures.IMAGE_RESOURCE_DATA_ENTRY>(entry.rawDataEntry);
            uint targetOffset = entry.OffsetToData - resourceDirectory.Section.RVA + resourceDirectory.Section.RawOffset;
            Writer.MoveToOffset(targetOffset);
            Writer.BinWriter.Write(entry.GetContents());
        }
    }
}
