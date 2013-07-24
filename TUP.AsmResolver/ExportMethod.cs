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
            this._lib = lib;
            this._name = name;
            this._nameRva = nameRva;
            this._rva = rva;
            this._ordinal = ordinal;
        }
        internal string _lib;
        internal string _name;
        internal uint _nameRva;
        internal uint _rva;
        internal ushort _ordinal;

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }        
        /// <summary>
        /// Gets the name of the declaring library of the method.
        /// </summary>
        public string LibraryName
        {
            get { return System.IO.Path.GetFileName(_lib); }
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
            get { return _rva; }
        }
        /// <summary>
        /// Gets the ordinal of the method.
        /// </summary>
        public ushort Ordinal
        {
            get { return _ordinal; }
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
