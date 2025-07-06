using System.Collections.Generic;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that can be referenced by a HasSemantics coded index.
    /// </summary>
    public interface IHasSemantics : IMemberDefinition
    {
        /// <summary>
        /// Gets a collection of methods that are associated with this member through special semantics.
        /// </summary>
        IList<MethodSemantics> Semantics
        {
            get;
        }
    }
}
