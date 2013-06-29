using System;
using System.Collections.Generic;
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
        }

        private void UpdateStreamHeaders(Workspace workspace)
        {
            // offset is relative to offset md header.
            uint currentOffset = (uint)CalculateMetaDataHeaderSize(workspace);
            foreach (MetaDataStream stream in workspace.NewNetHeader.MetaDataStreams)
            {
                stream.streamHeader.Offset = currentOffset;
                stream.streamHeader.Size = (uint)stream.mainStream.Length;

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
            uint mdSize = newHeader.MetaDataStreams[0].streamHeader.Offset;

            // add sizes of streams.
            foreach (MetaDataStream stream in newHeader.MetaDataStreams)
                mdSize += stream.StreamSize;

            newHeader.MetaDataDirectory.Size = mdSize;

            // TODO: resources directory.
        }

        private void ConstructNetDirectories(NETHeader newHeader)
        {
            newHeader.DataDirectories = new DataDirectory[] {
             new DataDirectory(DataDirectoryName.NETMetadata, default(Section), 0, newHeader.rawHeader.MetaData),
             new DataDirectory(DataDirectoryName.NETResource, default(Section), 0, newHeader.rawHeader.Resources),
             new DataDirectory(DataDirectoryName.NETStrongName, default(Section), 0, newHeader.rawHeader.StrongNameSignature),
             new DataDirectory(DataDirectoryName.NETCodeManager, default(Section), 0, newHeader.rawHeader.CodeManagerTable),
             new DataDirectory(DataDirectoryName.NETVTableFixups, default(Section), 0, newHeader.rawHeader.VTableFixups),
             new DataDirectory(DataDirectoryName.NETExport, default(Section), 0, newHeader.rawHeader.ExportAddressTableJumps),
             new DataDirectory(DataDirectoryName.NETNativeHeader, default(Section), 0, newHeader.rawHeader.ManagedNativeHeader),
            };

        }

        private void ConstructNativeDirectories(Workspace workspace)
        {
            workspace.NewDataDirectories = new DataDirectory[16];
          //  workspace.NewDataDirectories[
        }

        public int Align(int number, int align)
        {
            while (number % align != 0)
                number++;
            return number;
        }
    }
}
