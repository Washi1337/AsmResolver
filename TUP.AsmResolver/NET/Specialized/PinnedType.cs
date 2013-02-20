using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class PinnedType :TypeSpecification
    {
        public PinnedType(TypeReference typeRef)
            : base(typeRef)
        {
        }

        public override bool IsPinned
        {
            get
            {
                return true;
            }
        }
        public override bool IsValueType
        {
            get
            {
                return false;
            }
            internal set
            {
                return;
            }
        }
    }
}
