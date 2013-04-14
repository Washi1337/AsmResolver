using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public enum AssemblyHashAlgorithm : uint
    {
        /// <summary>
        /// No assembly hash algorithm is being used.
        /// </summary>
        None = 0x0,
        /// <summary>
        /// The md5 hash algorithm is being used.
        /// </summary>
        MD5 = 0x8003,
        /// <summary>
        /// The sha1 hash algorithm is being used.
        /// </summary>
        SHA1 = 0x8004
    }

 

}
