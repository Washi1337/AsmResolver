using System.Collections.Generic;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides members for describing an instantiation of a type or method.
    /// </summary>
    public interface IGenericArgumentsProvider
    {
        /// <summary>
        /// Gets a collection of type arguments used to instantiate the generic member.
        /// </summary>
        IList<TypeSignature> TypeArguments
        {
            get;
        }
    }
}