using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class PointerType : TypeSpecification
    {
        public PointerType(TypeReference typeRef) : base(typeRef)
        {
            this.name = typeRef.Name + "*";
        }

        public override bool IsPointer
        {
            get
            {
                return true;
            }
        }
    }
}
