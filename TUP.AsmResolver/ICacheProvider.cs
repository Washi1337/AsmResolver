using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    /// <summary>
    /// An object that saves data temporarily.
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// Clears the cached values.
        /// </summary>
        void ClearCache();
        /// <summary>
        /// Loads all cache values.
        /// </summary>
        void LoadCache();
    }
}
