using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class GenericInstanceType : TypeSpecification
    {
        public GenericInstanceType(TypeReference typeRef)
            : base(typeRef)
        {
            this.name = typeRef.Name;

        }

        internal TypeReference[] genericArguments;

        public override bool IsGenericInstance
        {
            get
            {
                return true;
            }
        }


        public override TypeReference[] GenericArguments { get { return genericArguments; } }

    }
}
