namespace AsmResolver.DotNet.Blob
{
    /// <summary>
    /// Provides fields for describing all possible members that can be referenced by name in a custom attribute.
    /// </summary>
    public enum CustomAttributeArgumentMemberType : byte
    {
        /// <summary>
        /// Indicates the referenced member is a field. 
        /// </summary>
        Field = 0x53,

        /// <summary>
        /// Indicates the referenced member is a property.
        /// </summary>
        Property = 0x54
    }
}