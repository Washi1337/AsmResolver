using System;
using System.Collections.Generic;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.File
{
    /// <summary>
    /// Models a file using the portable executable (PE) file format. It provides access to various PE headers, as well
    /// as the raw contents of each section present in the file. 
    /// </summary>
    public class PEFile
    {
        public const uint ValidPESignature = 0x4550; // "PE\0\0"
        
        /// <summary>
        /// Reads a PE file from the disk.
        /// </summary>
        /// <param name="path">The file path to the PE file.</param>
        /// <returns>The PE file that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEFile FromFile(string path)
        {
            return FromReader(new ByteArrayReader(System.IO.File.ReadAllBytes(path)));
        }

        /// <summary>
        /// Reads a PE file from memory.
        /// </summary>
        /// <param name="raw">The raw bytes representing the contents of the PE file to read.</param>
        /// <returns>The PE file that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEFile FromBytes(byte[] raw)
        {
            return FromReader(new ByteArrayReader(raw));
        }
        
        /// <summary>
        /// Reads a PE file from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The PE file that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEFile FromReader(IBinaryStreamReader reader)
        {
            var dosHeader = DosHeader.FromReader(reader);
            reader.FileOffset = dosHeader.NextHeaderOffset;

            uint signature = reader.ReadUInt32();
            if (signature != ValidPESignature)
                throw new BadImageFormatException();

            var peFile = new PEFile
            {
                DosHeader = dosHeader,
                FileHeader = FileHeader.FromReader(reader),
                OptionalHeader = OptionalHeader.FromReader(reader)
            };

            reader.FileOffset = peFile.OptionalHeader.FileOffset + peFile.FileHeader.SizeOfOptionalHeader;

            for (int i = 0; i < peFile.FileHeader.NumberOfSections; i++)
            {
                var header = SectionHeader.FromReader(reader);
                var contentsReader = reader.Fork(header.PointerToRawData, header.SizeOfRawData);
                peFile.Sections.Add(new PESection(header, DataDirectory.FromReader(contentsReader)));
            }
            
            return peFile;
        }

        private PEFile()
        {
        }

        /// <summary>
        /// Gets or sets the DOS header of the PE file.
        /// </summary>
        public DosHeader DosHeader
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the COFF file header of the portable executable (PE) file.
        /// </summary>
        public FileHeader FileHeader
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the optional header of the portable executable (PE) file.
        /// </summary>
        public OptionalHeader OptionalHeader
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of sections present in the portable executable (PE) file.
        /// </summary>
        public IList<PESection> Sections
        {
            get;
        } = new List<PESection>();

        public void Write(IBinaryStreamWriter writer)
        {
            
        }
        
    }
}