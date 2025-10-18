using System.Collections.Generic;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that can be assigned security declarations, and can be referenced by a HasDeclSecurity
    /// coded index.
    /// </summary>
    public interface IHasSecurityDeclaration : IMetadataDefinition
    {
        /// <summary>
        /// Gets a value indicating whether the member is assigned security declarations.
        /// </summary>
        bool HasSecurityDeclarations
        {
            get;
        }

        /// <summary>
        /// Gets a collection of security declarations assigned to the member.
        /// </summary>
        IList<SecurityDeclaration> SecurityDeclarations
        {
            get;
        }
    }
}
