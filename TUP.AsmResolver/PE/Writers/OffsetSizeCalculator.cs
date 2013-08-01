using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TUP.AsmResolver.NET;

namespace TUP.AsmResolver.PE.Writers
{
    public class OffsetSizeCalculator : RebuildingTask 
    {
        public OffsetSizeCalculator(PEConstructor constructor)
            : base(constructor)
        {
        }

        public override void RunProcedure(Workspace workspace)
        {
            UpdateStreamHeaders(workspace);
            ConstructNetDirectories(workspace.NewNetHeader);
            ConstructNativeDirectories(workspace);

        }

        private void UpdateStreamHeaders(Workspace workspace)
        {
            // offset is relative to offset md header.
            uint currentOffset = (uint)CalculateMetaDataHeaderSize(workspace);
            foreach (MetaDataStream stream in workspace.NewNetHeader.MetaDataStreams)
            {
                stream._streamHeader.Offset = currentOffset;
                stream._streamHeader.Size = (uint)stream._mainStream.Length;

                // next stream comes right after current stream.
                currentOffset += stream.StreamSize;
            }
        }

        private int CalculateMetaDataHeaderSize(Workspace workspace)
        {
            int metaDataHeader1Size = Marshal.SizeOf(typeof(Structures.METADATA_HEADER_1));
            int versionSize = Align(Constructor.OriginalAssembly.NETHeader.MetaDataHeader.VersionString.Length, 4);
            int metaDataHeader2Size = Marshal.SizeOf(typeof(Structures.METADATA_HEADER_2));
            int streamHeaderSize = CalculateStreamHeaderSize(workspace);

            return metaDataHeader1Size + versionSize + metaDataHeader2Size + streamHeaderSize;
        }

        private int CalculateStreamHeaderSize(Workspace workspace)
        {
            // all offset + sizes (uint + uint = 8 bytes)
            int size = (8 * workspace.NewNetHeader.MetaDataStreams.Length);
            foreach (MetaDataStream stream in workspace.NewNetHeader.MetaDataStreams)
            {
                // name + 0 terminator
                int length = stream.Name.Length + 1;
                // align to 4 bytes.
                length = Align(length, 4);

                size += length;
            }
            return size;
        }

        private void UpdateDataDirectories(Workspace workspace)
        {
            UpdateNetDirectories(workspace.NewNetHeader);   

        }

        private void UpdateNetDirectories(NETHeader newHeader)
        {
            ConstructNetDirectories(newHeader);

            // first stream has offset equal to md header size.
            uint mdSize = newHeader.MetaDataStreams[0]._streamHeader.Offset;

            // add sizes of streams.
            foreach (MetaDataStream stream in newHeader.MetaDataStreams)
                mdSize += stream.StreamSize;

            newHeader.MetaDataDirectory.Size = mdSize;

            // TODO: resources directory.
        }

        private void ConstructNetDirectories(NETHeader newHeader)
        {
            newHeader.DataDirectories = new DataDirectory[] {
             new DataDirectory(DataDirectoryName.NETMetadata, default(Section), 0, newHeader._rawHeader.MetaData),
             new DataDirectory(DataDirectoryName.NETResource, default(Section), 0, newHeader._rawHeader.Resources),
             new DataDirectory(DataDirectoryName.NETStrongName, default(Section), 0, newHeader._rawHeader.StrongNameSignature),
             new DataDirectory(DataDirectoryName.NETCodeManager, default(Section), 0, newHeader._rawHeader.CodeManagerTable),
             new DataDirectory(DataDirectoryName.NETVTableFixups, default(Section), 0, newHeader._rawHeader.VTableFixups),
             new DataDirectory(DataDirectoryName.NETExport, default(Section), 0, newHeader._rawHeader.ExportAddressTableJumps),
             new DataDirectory(DataDirectoryName.NETNativeHeader, default(Section), 0, newHeader._rawHeader.ManagedNativeHeader),
            };

        }

        private void ConstructNativeDirectories(Workspace workspace)
        {
            workspace.NewDataDirectories = new DataDirectory[16];

            var originalDirectories = Constructor.OriginalAssembly.NTHeader.OptionalHeader.DataDirectories;
            for (int i = 0; i < originalDirectories.Length; i++)
            {
                workspace.NewDataDirectories[i] = new DataDirectory(originalDirectories[i].Name, default(Section), 0, originalDirectories[i]._rawDataDir);
            }

            workspace.NewDataDirectories[(int)DataDirectoryName.Clr]._rawDataDir.Size = workspace.NewNetHeader._rawHeader.cb;

        }

        private void ConstructPeSections(Workspace workspace)
        {
            Section textSection = CreateTextSection(workspace);
            Section[] sections = new Section[3]
            {
                textSection,
                Section.GetSectionByName(Constructor.OriginalAssembly, ".rsrc"),
                Section.GetSectionByName(Constructor.OriginalAssembly, ".reloc"),
            };

        }

        private Section CreateTextSection(Workspace workspace)
        {
            Section section;
            using (MemoryStream sectionStream = new MemoryStream())
            {
                using (BinaryWriter sectionWriter = new BinaryWriter(sectionStream))
                {
                    // write import address dir space.
                    sectionWriter.Write(new byte[8]);

                    // write .net header
                    sectionWriter.WriteStructure(workspace.NewNetHeader._rawHeader);

                    // write method bodies
                    sectionWriter.Write(workspace.MethodBodyTable.Stream.ToArray());

                    // write resources dir
                    sectionWriter.Write(Constructor.OriginalAssembly.NETHeader.ResourcesDirectory.GetBytes());

                    // write metadata
                    WriteMetaData(workspace, sectionWriter);

                    // write import dir
                }

                section = new Section(".text", 0, sectionStream.ToArray());
            }

            return section;
        }

        private void WriteMetaData(Workspace workspace, BinaryWriter sectionWriter)
        {                    
            // write metadata header
            sectionWriter.WriteStructure(workspace.NewNetHeader.MetaDataHeader._reader.metadataHeader1);
            sectionWriter.Write(Encoding.ASCII.GetBytes(workspace.NewNetHeader.MetaDataHeader.VersionString));
            sectionWriter.WriteStructure(workspace.NewNetHeader.MetaDataHeader._reader.metadataHeader2);

            // write metadata stream headers
            foreach (MetaDataStream stream in workspace.NewNetHeader.MetaDataStreams)
            {
                sectionWriter.WriteStructure(stream._streamHeader);
                sectionWriter.Write(Encoding.ASCII.GetBytes(stream.Name));
                sectionWriter.Write(new byte[Align(stream.Name.Length + 1, 4)]);
            }

            // write metadata streams
            foreach (MetaDataStream stream in workspace.NewNetHeader.MetaDataStreams)
            {
                sectionWriter.Write(stream._mainStream.ToArray());
            }
        }

        private int Align(int number, int align)
        {
            while (number % align != 0)
                number++;
            return number;
        }
    }
}
