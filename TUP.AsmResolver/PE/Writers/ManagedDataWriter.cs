using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.NET;
namespace TUP.AsmResolver.PE.Writers
{
    internal class ManagedDataWriter : IWriterTask 
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
        
        public void RunProcedure()
        {
            if (clrDirectory.TargetOffset.FileOffset != 0)
            {
                Writer.MoveToOffset(clrDirectory.TargetOffset.FileOffset);
                Writer.WriteStructure<Structures.IMAGE_COR20_HEADER>(Writer.OriginalAssembly.NETHeader.reader.netheader);
                metadataDirectory = Writer.OriginalAssembly.NETHeader.MetaDataDirectory;
                WriteMetaDataHeader();
                WriteStreamHeaders();
                WriteStreams();
            }
        }

        private void WriteMetaDataHeader()
        {
            Writer.MoveToOffset(metadataDirectory.TargetOffset.FileOffset);
            Writer.WriteStructure<Structures.METADATA_HEADER_1>(Writer.OriginalAssembly.NETHeader.reader.metadataheader1);
            byte[] versionBytes = Encoding.ASCII.GetBytes(Writer.OriginalAssembly.NETHeader.MetaDataHeader.VersionString);
            Writer.BinWriter.Write(versionBytes);
            Writer.WriteStructure<Structures.METADATA_HEADER_2>(Writer.OriginalAssembly.NETHeader.reader.metadataheader2);
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
            //TODO: Rebuild heaps.
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
