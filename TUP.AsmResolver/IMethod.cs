using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents a method that is used in native portable executables.
    /// </summary>
    public interface IMethod
    {
        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Gets the full name of the method, including the Declaring Library and the Name.
        /// </summary>
        string FullName { get; }
        /// <summary>
        /// Gets the declaring library's name.
        /// </summary>
        string LibraryName { get; }
        /// <summary>
        /// Gets the ordinal of the method.
        /// </summary>
        ushort Ordinal { get; }
        /// <summary>
        /// Gets the Relative Virtual Address of the method.
        /// </summary>
        uint RVA { get; }

    }
}
