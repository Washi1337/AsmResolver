using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public enum FileAttributes
    {
        /// <summary>
        /// Specifies the file reference contains metadata.
        /// </summary>
        ContainsMetadata,
        /// <summary>
        /// Specifies the file references doesn't contain metadata.
        /// </summary>
        ContainsNoMetadata,
    }
}
