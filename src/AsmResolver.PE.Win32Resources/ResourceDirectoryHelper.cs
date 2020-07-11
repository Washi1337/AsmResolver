namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides useful methods for working with resource directories.
    /// </summary>
    internal static class ResourceDirectoryHelper
    {
        /// <summary>
        /// Finds the index of a resource directory of a given resource type.
        /// </summary>
        /// <param name="rootDirectory">The root resource directory where the search is started.</param>
        /// <param name="resourceType">The resource type to search for.</param>
        /// <returns>Returns the index of the found resource directory of the given resource type, otherwise -1.</returns>
        public static int IndexOfResourceDirectoryType(IResourceDirectory rootDirectory, ResourceType resourceType)
        {
            for (int i = 0; i < rootDirectory.Entries.Count; i++)
            {
                if (rootDirectory.Entries[i] is IResourceDirectory directory
                    && directory.Type == resourceType)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
