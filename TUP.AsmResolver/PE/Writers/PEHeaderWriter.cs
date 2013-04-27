using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.PE.Writers
{
    internal class PEHeaderWriter : IWriterTask , ICalculationTask
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

        public uint NewSize
        {
            get;
            private set;
        }

        public void CalculateOffsetsAndSizes()
        {
            if (Writer.OriginalAssembly.NTHeader.IsManagedAssembly)
                ReorderDataDirectories(Writer.OriginalAssembly.NETHeader.DataDirectories);
            ReorderDataDirectories(Writer.OriginalAssembly.NTHeader.OptionalHeader.DataDirectories);
        }

        public void RunProcedure()
        {
            WriteMZHeader();
            WriteNTHeaders();
        }

        private void ReorderDataDirectories(DataDirectory[] directories)
        {
            // Definitely need work :D
            for (int i = 0; i < directories.Length;i ++)
            {
                if (directories[i].TargetOffset.FileOffset != 0)
                {
                    DataDirectory overlap = GetOverlappingDirectory(directories, directories[i]);
                    while (overlap != null)
                    {
                        directories[i].TargetOffset = Offset.FromFileOffset(overlap.TargetOffset.FileOffset + overlap.Size, Writer.OriginalAssembly);
                        overlap = GetOverlappingDirectory(directories, directories[i]);
                    }
                }
            }
        }

        private DataDirectory GetOverlappingDirectory(DataDirectory[] parentDirectories, DataDirectory directory)
        {
            int currentIndex = Array.IndexOf(parentDirectories, directory);

            for (int i = 0; i < parentDirectories.Length; i++)
            {
                if (parentDirectories[i].TargetOffset.FileOffset != 0)
                {
                    if (parentDirectories[i].Name == DataDirectoryName.Clr)
                    {
                        DataDirectory dir = GetOverlappingDirectory(Writer.OriginalAssembly.NETHeader.DataDirectories, directory);
                        if (dir != null)
                            return dir;
                    }
                    
                    if (currentIndex != i &&
                        (((parentDirectories[i].TargetOffset.FileOffset >= directory.TargetOffset.FileOffset) &&
                        (parentDirectories[i].TargetOffset.FileOffset < directory.TargetOffset.FileOffset + directory.Size)) ||
                        ((directory.TargetOffset.FileOffset >= parentDirectories[i].TargetOffset.FileOffset) &&
                        (directory.TargetOffset.FileOffset < parentDirectories[i].TargetOffset.FileOffset + parentDirectories[i].Size))))

                        return parentDirectories[i];
                }
            }
            return null;
        }
        
        private void WriteMZHeader()
        {
            Writer.WriteStructure<Structures.IMAGE_DOS_HEADER>(Writer.OriginalAssembly.headerReader.dosHeader);
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
            Writer.WriteStructure<Structures.IMAGE_FILE_HEADER>(Writer.OriginalAssembly.headerReader.fileHeader);
        }
        private void WriteOptionalHeader()
        {
            uint optionalheaderOffset = (uint)Writer.OutputStream.Position;
            if (Writer.OriginalAssembly.NTHeader.OptionalHeader.Is32Bit)
                Writer.WriteStructure<Structures.IMAGE_OPTIONAL_HEADER32>(Writer.OriginalAssembly.headerReader.optionalHeader32);
            else
                Writer.WriteStructure<Structures.IMAGE_OPTIONAL_HEADER64>(Writer.OriginalAssembly.headerReader.optionalHeader64);

            WriteDataDirectories(Writer.OriginalAssembly.NTHeader.OptionalHeader.DataDirectories);
            Writer.WritePaddingZeros(optionalheaderOffset + Writer.OriginalAssembly.NTHeader.FileHeader.OptionalHeaderSize);
            WriteSectionHeaders();
        }

        private void WriteDataDirectories(DataDirectory[] directories)
        {
            foreach (DataDirectory directory in directories)
            {
                Writer.WriteStructure<Structures.IMAGE_DATA_DIRECTORY>(directory.rawDataDir);
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
