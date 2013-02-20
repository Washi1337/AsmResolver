using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    /// <summary>
    /// An enumeration that represents all possible data directory names in the Optional Header.
    /// </summary>
    public enum DataDirectoryName
    {
        Export,
        Import,
        Resource,
        Exception,
        Security,
        Relocation,
        Debug,
        Architecture,
        Reserved_1,
        Tls,
        Configuration,
        BoundImport,
        ImportAddress,
        DelayImport,
        Clr,
        Reserved_2,
    }
}
