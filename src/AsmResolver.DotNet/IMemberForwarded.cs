namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that can be assigned Platform Invoke (P/Invoke) implementation mappings, and can be indexed
    /// using a MemberForwarded coded index.
    /// </summary>
    public interface IMemberForwarded : IMemberDefinition
    {
        /// <summary>
        /// Gets or sets the Platform Invoke (P/Invoke) implementation mapping of this member (if available).
        /// </summary>
        ImplementationMap? ImplementationMap
        {
            get;
            set;
        }
    }
}
