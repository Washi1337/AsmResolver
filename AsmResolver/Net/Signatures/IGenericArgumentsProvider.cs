using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsmResolver.Net.Signatures
{
    public interface IGenericArgumentsProvider
    {
        IList<TypeSignature> GenericArguments
        {
            get;
        }
    }
}
