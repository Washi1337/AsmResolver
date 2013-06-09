using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ByReferenceType : TypeSpecification
    {

        public ByReferenceType(TypeReference typeRef)
            : base(typeRef)
        {
        }

        public override string Name
        {
            get
            {
                return base.Name + "&";
            }
        }

        public override bool IsByReference
        {
            get
            {
                return true;
            }
        }

    }
}
