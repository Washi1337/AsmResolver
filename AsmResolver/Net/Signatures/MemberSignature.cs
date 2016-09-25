using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net.Signatures
{
    public abstract class MemberSignature : CallingConventionSignature, IHasTypeSignature
    {
        protected abstract TypeSignature TypeSignature
        {
            get;
        }

        TypeSignature IHasTypeSignature.TypeSignature
        {
            get { return TypeSignature; }
        }

        public override string ToString()
        {
            return (HasThis ? "instance " : string.Empty) + TypeSignature.FullName;
        }
    }
}
