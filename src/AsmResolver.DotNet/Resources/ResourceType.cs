namespace AsmResolver.DotNet.Resources
{
    /// <summary>
    /// Represents a resource data type in a resource set.
    /// </summary>
    public abstract class ResourceType
    {
        /// <summary>
        /// Gets the full name of the type that was referenced.
        /// </summary>
        public abstract string FullName
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString() => FullName;
    }
}
