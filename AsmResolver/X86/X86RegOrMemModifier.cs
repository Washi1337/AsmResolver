using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.X86
{
    public enum X86RegOrMemModifier
    {
        RegisterPointer,
        RegisterDispShortPointer,
        RegisterDispLongPointer,
        RegisterOnly,
    }
}
