using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.PE.Writers
{
    internal class PEHeaderWriter : IWriterTask
    {
        internal PEHeaderWriter(PEWriter writer)
        {
            Writer = writer;
        }

        public PEWriter Writer
        {
            get;
            private set;
        }

        public void RunProcedure()
        {
            WriteMZHeader();
            WriteNTHeaders();
        }

        private void WriteMZHeader()
        {
            Writer.WriteStructure<Structures.IMAGE_DOS_HEADER>(Writer.OriginalAssembly.headerreader.dosHeader);
            Writer.BinWriter.Write(new byte[] { 0x0E, 0x1F, 0xBA, 0x0E, 0x00, 0xB4, 0x09, 0xCD, 0x21, 0xB8, 0x01,0x4C, 0xCD, 0x21});
            Writer.BinWriter.Write(Encoding.ASCII.GetBytes(Writer.OriginalAssembly.MZHeader.StopMessage));
            Writer.BinWriter.Write(0x24);
            Writer.BinWriter.Write(0);
        }

        private void WriteNTHeaders()
        {
            Writer.MoveToOffset((uint)Writer.OriginalAssembly.MZHeader.NTHeaderOffset);
            Writer.BinWriter.Write((uint)Writer.OriginalAssembly.NTHeader.Signature);
            WriteFileHeader();
            WriteOptionalHeader();
        }
        private void WriteFileHeader()
        {
            Writer.WriteStructure<Structures.IMAGE_FILE_HEADER>(Writer.OriginalAssembly.headerreader.fileHeader);
        }
        private void WriteOptionalHeader()
        {
            uint optionalheaderOffset = (uint)Writer.OutputStream.Position;
            if (Writer.OriginalAssembly.NTHeader.OptionalHeader.Is32Bit)
                Writer.WriteStructure<Structures.IMAGE_OPTIONAL_HEADER32>(Writer.OriginalAssembly.headerreader.optionalHeader32);
            else
                Writer.WriteStructure<Structures.IMAGE_OPTIONAL_HEADER64>(Writer.OriginalAssembly.headerreader.optionalHeader64);

            WriteDataDirectories(Writer.OriginalAssembly.NTHeader.OptionalHeader.DataDirectories);
            Writer.WritePaddingZeros(optionalheaderOffset + Writer.OriginalAssembly.NTHeader.FileHeader.OptionalHeaderSize);
            WriteSectionHeaders();
        }

        private void WriteDataDirectories(DataDirectory[] directories)
        {
            foreach (DataDirectory directory in directories)
            {
                Writer.BinWriter.Write(directory.TargetOffset.Rva);
                Writer.BinWriter.Write(directory.Size);
            }
        }
        

        private void WriteSectionHeaders()
        {
            foreach (Section section in Writer.OriginalAssembly.NTHeader.Sections)
            {
                byte[] sectionName = Encoding.ASCII.GetBytes(section.Name);
                if (sectionName.Length < 8)
                    Array.Resize(ref sectionName, 8);

                Writer.BinWriter.Write(sectionName);
                Writer.BinWriter.Write(section.VirtualSize);
                Writer.BinWriter.Write(section.RVA);
                Writer.BinWriter.Write(section.RawSize);
                Writer.BinWriter.Write(section.RawOffset);
                Writer.BinWriter.Write(0); // reloc address
                Writer.BinWriter.Write(0); // line numbers
                Writer.BinWriter.Write((ushort)0); // reloc number
                Writer.BinWriter.Write((ushort)0); // line numbers number
                Writer.BinWriter.Write((uint)section.Flags);
            }
        }

    }
}
