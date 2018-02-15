using System.Collections.Generic;

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
