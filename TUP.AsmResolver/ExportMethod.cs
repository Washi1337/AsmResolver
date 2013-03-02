using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.ASM;
namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents a procedure or method that is inside a portable executable file and can be used by other PE files.
    /// </summary>
    public class ExportMethod : IMethod 
    {
        internal ExportMethod(string lib, string name, uint nameRva, uint rva, ushort ordinal)
        {
            this.lib = lib;
            this.name = name;
            this.nameRva = nameRva;
            this.rva = rva;
            this.ordinal = ordinal;
        }
        internal string lib;
        internal string name;
        internal uint nameRva;
        internal uint rva;
        internal ushort ordinal;

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string Name
        {
            get { return name; }
        }        
        /// <summary>
        /// Gets the name of the declaring library of the method.
        /// </summary>
        public string LibraryName
        {
            get { return System.IO.Path.GetFileName(lib); }
        }
        /// <summary>
        /// Gets the full name of the method, including the declaring library and method name.
        /// </summary>
        public string FullName
        {
            get { return LibraryName + "." + Name; }
        }
        /// <summary>
        /// Gets the Relative Virtual Address of the method.
        /// </summary>
        public uint RVA
        {
            get { return rva; }
        }
        /// <summary>
        /// Gets the ordinal of the method.
        /// </summary>
        public ushort Ordinal
        {
            get { return ordinal; }
        }

        
        /// <summary>
        /// Returns the string representation of the exportable method.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return FullName;
        }


        
    }
}
