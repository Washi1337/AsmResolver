using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUP.AsmResolver.NET.Specialized.MSIL
{
    
    public enum StackBehaviour : ushort
    {
        Pop0 = 0x0000,
        Pop1 = 0x0001,
        Pop1_pop1 = 0x0002,
        Popi = 0x0003,
        Popi_pop1 = 0x0004,
        Popi_popi = 0x0005,
        Popi_popi8 = 0x0006,
        Popi_popi_popi = 0x0007,
        Popi_popr4 = 0x0008,
        Popi_popr8 = 0x0009,
        Popref = 0x000A,
        Popref_pop1 = 0x000B,
        Popref_popi = 0x000C,
        Popref_popi_pop1 = 0x000D,
        Popref_popi_popi = 0x000E,
        Popref_popi_popi8 = 0x000F,
        Popref_popi_popr4 = 0x0010,
        Popref_popi_popr8 = 0x0011,
        Popref_popi_popref = 0x0012,
        Varpop = 0x0013,

        Push0 = 0x0100,
        Push1 = 0x0200,
        Push1_push1 = 0x0300,
        Pushi = 0x0400,
        Pushi8 = 0x0500,
        Pushr4 = 0x0600,
        Pushr8 = 0x0700,
        Pushref = 0x0800,
        Varpush = 0x0900,
    }

}
