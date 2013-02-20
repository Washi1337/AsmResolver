using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using TUP.AsmResolver.PE;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents the header of a Portable Executeable file
    /// </summary>
    public class NTHeader : IHeader
    {
        internal PeHeaderReader header;
        internal FileHeader fheader;
        internal string file;
        internal Win32Assembly assembly;
        
        /// <summary>
        /// Gets the file destination the header is located in.
        /// </summary>
        public string FilePath
        {
            get { return file; }
        }
        /// <summary>
        /// Gets the File Header representation of the NT Header.
        /// </summary>
        public FileHeader FileHeader
        {
            get
            {
                return fheader;
            }
        }
        /// <summary>
        /// Gets the Optional Header representation of the NT Header.
        /// </summary>
        public IOptionalHeader OptionalHeader
        {
            get
            {
                return header.optionalHeader;
            }
        }
        /// <summary>
        /// Gets the image signature of the portable executable.
        /// </summary>
        public ImageSignature Signature
        {
            get
            {
                return (ImageSignature)header.ntHeadersSignature;
            }
        }
        /// <summary>
        /// Returns true if the loaded pe file is managed and uses a .NET Runtime.
        /// </summary>
        /// <returns></returns>
         public bool IsManagedAssembly
         {
             get { return header.ManagedDataAvailable; }
         }
        /// <summary>
        /// Gets the sections of the portable executable file.
        /// </summary>
        public Section[] Sections
        {
            get { return header.sections.ToArray(); }
        }
        /// <summary>
        /// Gets the parent assembly container of the NT header.
        /// </summary>
        public Win32Assembly ParentAssembly
        {
            get
            {
                return assembly;
            }
        }
        /// <summary>
        /// Gets the raw file offset of the header.
        /// </summary>
        public long RawOffset
        {
            get
            {
                return header.ntheaderoffset;
            }
        }

        /// <summary>
        /// Gets the Portable Executeable's NT header by specifing the assembly.
        /// </summary>
        /// <param name="assembly">The assembly to read the nt header</param>
        /// <returns></returns>
        public static NTHeader FromAssembly(Win32Assembly assembly)
        {
            NTHeader a = new NTHeader();
            a.assembly = assembly;
            a.file = assembly.path;
            a.header = assembly.headerreader;

            a.fheader = FileHeader.FromAssembly(assembly);
            
            
            return a;
        }
        internal NTHeader()
        {

        }
        internal void Initialize(PeHeaderReader reader)
        {
            //header = assembly.headerreader;
            //
            //fheader.header = header;
            //if (header.Is32BitHeader)
            //    oheader = OptionalHeader32.FromAssembly(assembly);
            //else
            //    oheader = OptionalHeader64.FromAssembly(assembly);

        }


    }
}
