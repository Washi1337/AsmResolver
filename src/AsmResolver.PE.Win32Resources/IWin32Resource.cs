namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides a high level view of a native win32 resource that can be stored in the resources data directory of a
    /// portable executable (PE) image.
    /// </summary>
    public interface IWin32Resource
    {
        /// <summary>
        /// Serializes the win32 resource to the provided root win32 resource data directory.
        /// </summary>
        /// <param name="rootDirectory">The root directory to submit the changes to.</param>
        void InsertIntoDirectory(IResourceDirectory rootDirectory);
    }
}
