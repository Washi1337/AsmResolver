using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized.MSIL
{
    public class VariableDefinition
    {

        internal VariableDefinition(int index, TypeReference type)
        {
            Index = index;
            VariableType = type;
            Name = "";
        }

        public int Index { get; internal set; }
        public TypeReference VariableType { get; internal set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return "[" + Index + "]" + (Name != null ? Name : "");
        }
    }
}
