using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class GenericParamReference : TypeSpecification
    {
        internal GenericParamReference(int index, TypeReference baseType)
            : base(baseType)
        {
            Index = index;
            IsMethodVar = baseType.elementType == ElementType.MVar;

        }

        public int Index { get; internal set; }
        public bool IsMethodVar { get; internal set; }
        
    }
}
