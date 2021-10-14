namespace AsmResolver.DotNet.Resources
{
    /// <summary>
    /// Represents a user-defined resource data type.
    /// </summary>
    public class UserDefinedResourceType : ResourceType
    {
        /// <inheritdoc />
        public UserDefinedResourceType(string fullName)
        {
            FullName = fullName;
        }

        /// <inheritdoc />
        public override string FullName
        {
            get;
        }
    }
}
