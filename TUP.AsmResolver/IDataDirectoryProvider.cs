using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    public interface IDataDirectoryProvider
    {
        DataDirectory[] DataDirectories { get; }
    }
}
