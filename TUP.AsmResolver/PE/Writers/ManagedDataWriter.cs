using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.NET;
namespace TUP.AsmResolver.PE.Writers
{
    internal class ManagedDataWriter : IWriterTask , IReconstructionTask
    {
        DataDirectory clrDirectory;
        DataDirectory metadataDirectory;
        internal ManagedDataWriter(PEWriter writer)
        {
            Writer = writer;
            clrDirectory = Writer.OriginalAssembly.NTHeader.OptionalHeader.DataDirectories[(int)DataDirectoryName.Clr];
        }

        public PEWriter Writer
        {
            get;
            private set;
        }

        public void Reconstruct()
        {
            if (Writer.OriginalAssembly.ntHeader.IsManagedAssembly)
            {
                NETHeader netHeader = Writer.OriginalAssembly.NETHeader;

                uint streamOffset = netHeader.MetaDataStreams[0].streamHeader.Offset;
                netHeader.reader.netHeader.MetaData.Size = streamOffset;
                DataDirectory dataDir = netHeader.MetaDataDirectory;
                dataDir.rawDataDir.Size = streamOffset;

                // reconstruct blob (temporary solution, will be removed once blobs are being re-serialized).
                netHeader.BlobHeap.Reconstruct();
                // rebuild tables heap and update all other heaps.
                netHeader.TablesHeap.Reconstruct();

                foreach (MetaDataStream stream in netHeader.MetaDataStreams)
                {
                    // reset offset to prevent overwriting of expanded streams.
                    stream.streamHeader.Offset = streamOffset;
                    // calculate next stream offset.
                    streamOffset += stream.StreamSize;
                    // increase total md dir size
                    Writer.OriginalAssembly.NETHeader.reader.netHeader.MetaData.Size += stream.StreamSize;
                    dataDir.rawDataDir.Size += stream.StreamSize;
                }
            }
            
        }

        public void RunProcedure()
        {
            if (clrDirectory.TargetOffset.FileOffset != 0)
            {
                Writer.MoveToOffset(clrDirectory.TargetOffset.FileOffset);
                Writer.WriteStructure<Structures.IMAGE_COR20_HEADER>(Writer.OriginalAssembly.NETHeader.reader.netHeader);
                metadataDirectory = Writer.OriginalAssembly.NETHeader.MetaDataDirectory;
                WriteMetaDataHeader();
                WriteStreamHeaders();
                WriteStreams();
            }
        }

        private void WriteMetaDataHeader()
        {
            Writer.MoveToOffset(metadataDirectory.TargetOffset.FileOffset);
            Writer.WriteStructure<Structures.METADATA_HEADER_1>(Writer.OriginalAssembly.NETHeader.reader.metadataHeader1);
            byte[] versionBytes = Encoding.ASCII.GetBytes(Writer.OriginalAssembly.NETHeader.MetaDataHeader.VersionString);
            Writer.BinWriter.Write(versionBytes);
            Writer.WriteStructure<Structures.METADATA_HEADER_2>(Writer.OriginalAssembly.NETHeader.reader.metadataHeader2);
        }

        private void WriteStreamHeaders()
        {
            foreach (MetaDataStream stream in Writer.OriginalAssembly.NETHeader.MetaDataStreams)
            {
                Writer.WriteStructure<Structures.METADATA_STREAM_HEADER>(stream.streamHeader);
                Writer.WriteAsciiZString(stream.Name);
                Align(4);
            }
        }

        private void WriteStreams()
        {
            foreach (MetaDataStream stream in Writer.OriginalAssembly.NETHeader.MetaDataStreams)
            {
                Writer.MoveToOffset(stream.StreamOffset);
                Writer.BinWriter.Write(stream.Contents);
            }
        }

        private void Align(int align)
        {
            align--;
            Writer.BinWriter.Write(new byte[(((int)Writer.OutputStream.Position + align) & ~align) - (int)Writer.OutputStream.Position]);
        }

    }
}
