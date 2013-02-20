using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public enum ManifestResourceAttributes : uint
    {
        VisibilityMask = 0x0007,
        Public = 0x0001,     // The Resource is exported from the Assembly.
        Private = 0x0002,     // The Resource is private to the Assembly.
    }
}
