using System.Collections.Generic;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a metadata member capable of owning types, a TypeDefinition or ModuleDefinition.
    /// </summary>
    public interface ITypeOwner : IMetadataMember
    {
        /// <summary>
        /// Gets the list of types this member owns.
        /// </summary>
        IList<TypeDefinition> OwnedTypes { get; }
    }
}
