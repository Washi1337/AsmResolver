using System.Collections.Generic;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that can be referenced by a TypeOrMethod coded index, and exposes generic parameters.
    /// </summary>
    public interface IHasGenericParameters : IMemberDefinition
    {
        /// <summary>
        /// Gets a collection of generic parameters this member defines.
        /// </summary>
        IList<GenericParameter> GenericParameters
        {
            get;
        }
    }
}
