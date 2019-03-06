using System.Collections.Generic;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Provides members for obtaining generic type arguments.
    /// </summary>
    public interface IGenericArgumentsProvider
    {
        /// <summary>
        /// Gets a collection of generic type arguments to instantiate the member.
        /// </summary>
        IList<TypeSignature> GenericArguments
        {
            get;
        }
    }
}
