using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    [Flags]
    public enum MethodSemanticsAttributes : ushort
    {
        Setter = 0x0001,     // Setter for property
        Getter = 0x0002,     // Getter for property
        Other = 0x0004,     // other method for property or event
        AddOn = 0x0008,     // AddOn method for event
        RemoveOn = 0x0010,     // RemoveOn method for event
        Fire = 0x0020,     // Fire method for event
    }
}
