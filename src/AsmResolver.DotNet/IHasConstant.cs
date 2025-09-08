namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that can be assigned a constant value, and can be referenced by a HasConstant coded index.
    /// </summary>
    public interface IHasConstant : IMetadataDefinition
    {
        /// <summary>
        /// Gets or sets a constant that is assigned to the member.
        /// </summary>
        /// <remarks>
        /// <para>If this property is set to <c>null</c>, the member is not assigned a constant value.</para>
        /// <para>Updating this property does not update any of the attributes associated to the member.</para>
        /// </remarks>
        Constant? Constant
        {
            get;
            set;
        }
    }
}
