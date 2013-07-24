using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents the MZ header of a portable executable file
    /// </summary>
    public class MZHeader : IHeader
    {
        internal MZHeader()
        {
        }

        internal Win32Assembly _assembly;
        /// <summary>
        /// Gets the Portable Executeable's MZ header by specifing the assembly.
        /// </summary>
        /// <param name="assembly">The assembly to read the mz header</param>
        /// <returns></returns>
        public static MZHeader FromAssembly(Win32Assembly assembly)
        {
            MZHeader a = new MZHeader();
            a._assembly = assembly;
            return a;
        }

        /// <summary>
        /// Gets the amount of pages in the executable file.
        /// </summary>
        public ushort PagesInFile
        {
            get
            { return _assembly._headerReader.dosHeader.e_cp; }
        }
        /// <summary>
        /// Gets the offset of the NT header that is specified in the MZ header.
        /// </summary>
        public int NTHeaderOffset
        {
            get
            {
                return _assembly._headerReader.dosHeader.e_lfanew;
            }
        }
        /// <summary>
        /// Gets the offset of the relocation table that is specified in the MZ header.
        /// </summary>
        public ushort RelocationTableOffset
        {
            get
            {
                return _assembly._headerReader.dosHeader.e_lfarlc;
            }

        }

        /// <summary>
        /// The maximum length the stop message can have.
        /// </summary>
        public const int MaxStopMessageLength = 39;

        /// <summary>
        /// Gets or sets the string that will be displayed when the program cannot be run in DOS mode. Default is: "This program cannot be run in DOS mode."
        /// </summary>
        public string StopMessage
        {
            get
            {
                return _assembly._peImage.ReadByteTerminatedString(0x4E, 0x24);
            }
            set
            {
                if (value.Length > MaxStopMessageLength)
                    throw new ArgumentException("The string cannot be larger than the MaxStopMessageLength.", "value");

                List<byte> bytes = Encoding.ASCII.GetBytes(value).ToList();

                while (bytes.Count < MaxStopMessageLength)
                    bytes.Add(0);

                _assembly._peImage.SetOffset(0x4E);
                _assembly._peImage.Writer.Write(bytes.ToArray());
                
            }
        }

        /// <summary>
        /// Gets the raw file offset of the header.
        /// </summary>
        public long RawOffset
        {
            get
            {
                return 0;
            }
        }
        /// <summary>
        /// Gets the parent assembly container of the MZ header.
        /// </summary>
        public Win32Assembly ParentAssembly
        {
            get
            {
                return _assembly;
            }
        }


    }
}
