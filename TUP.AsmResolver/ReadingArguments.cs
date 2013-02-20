using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Specifies reading arguments that are being used to read a win32 assembly.
    /// </summary>
    public class ReadingArguments
    {
        internal Win32Assembly assembly;
        private bool onlyManaged;
        private bool ignoreDataDirectoryAmount;

        /// <summary>
        /// Indicates that only managed data and the vital headers will be read from the assembly.
        /// </summary>
        public bool OnlyManaged
        {
            get { return onlyManaged; }
            set
            {
                if (assembly != null)
                    throw new InvalidOperationException("Cannot edit properties when reading arguments are already used.");
                onlyManaged = value;
            }
        }
        /// <summary>
        /// Indicates that the standard amount of data directories will be used instead of the amount specified in the Optional Header
        /// </summary>
        public bool IgnoreDataDirectoryAmount
        {
            get { return ignoreDataDirectoryAmount; }
            set
            {
                if (assembly != null)
                    throw new InvalidOperationException("Cannot edit properties when reading arguments are already used.");
                ignoreDataDirectoryAmount = value;
            }
        }
    }
}
