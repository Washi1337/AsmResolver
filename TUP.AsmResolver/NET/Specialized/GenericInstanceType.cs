using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class GenericInstanceType : TypeSpecification, IGenericInstance, IGenericContext 
    {
        public GenericInstanceType(TypeReference typeRef)
            : base(typeRef)
        {
        }

        internal TypeReference[] _genericArguments;

        public override bool IsGenericInstance
        {
            get
            {
                return true;
            }
        }

        public override GenericParameter[] GenericParameters
        {
            get
            {
                return OriginalType.GenericParameters;
            }
        }

        public TypeReference[] GenericArguments { get { return _genericArguments; } }

        public bool HasGenericArguments
        {
            get { return GenericArguments != null && GenericArguments.Length != 0; }
        }

        IGenericParamProvider IGenericContext.Type
        {
            get { return this.OriginalType; }
        }

    }
}
