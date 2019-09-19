using System;
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
            var dosHeader = Headers.DosHeader.FromReader(reader);
            reader.FileOffset += dosHeader.NextHeaderOffset;

            uint signature = reader.ReadUInt32();
            if (signature != ValidPESignature)
                throw new BadImageFormatException();

            var fileHeader = Headers.FileHeader.FromReader(reader);
            
            var file = new PEFile
            {
                DosHeader = dosHeader,
                FileHeader = fileHeader,
                
            };

            return file;
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
        /// Gets or sets the COFF file header of the PE file.
        /// </summary>
        public FileHeader FileHeader
        {
            get;
            set;
        }
        
    }
}