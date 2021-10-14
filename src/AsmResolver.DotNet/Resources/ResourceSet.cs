using AsmResolver.Collections;

namespace AsmResolver.DotNet.Resources
{
    /// <summary>
    /// Represents a set of resources embedded into a ".resources" file.
    /// </summary>
    public class ResourceSet : LazyList<ResourceSetEntry>
    {
        /// <summary>
        /// Gets the version of the file format that is used for this resource set.
        /// </summary>
        public int FormatVersion
        {
            get;
            set;
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
        }
    }
}
