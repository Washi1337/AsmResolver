using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Specifies reading arguments that are being used to read a win32 assembly.
    /// </summary>
    public class ReadingParameters
    {
        internal Win32Assembly _assembly;
        private bool _onlyManaged, _ignoreDataDirectoryAmount;

        /// <summary>
        /// Creates a new instance of the ReadingParameters, and sets the arguments to their default values.
        /// </summary>
        public ReadingParameters()
        {
            _onlyManaged = false;
            _ignoreDataDirectoryAmount = false;
        }

        /// <summary>
        /// Indicates that only managed data and the vital headers will be read from the assembly.
        /// </summary>
        public bool OnlyManaged
        {
            get { return _onlyManaged; }
            set
            {
                if (_assembly != null)
                    ThrowInvalidOperation();
                _onlyManaged = value;
            }
        }
        /// <summary>
        /// Indicates that the standard amount of data directories will be used instead of the amount specified in the Optional Header
        /// </summary>
        public bool IgnoreDataDirectoryAmount
        {
            get { return _ignoreDataDirectoryAmount; }
            set
            {
                if (_assembly != null)
                    ThrowInvalidOperation();
                _ignoreDataDirectoryAmount = value;
            }
        }

        private void ThrowInvalidOperation()
        {
            throw new InvalidOperationException("Cannot edit properties when reading arguments are already used.");
        }
    }
}
