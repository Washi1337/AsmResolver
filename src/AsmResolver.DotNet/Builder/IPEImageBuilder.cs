namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides members for serializing a .NET module to a PE image.
    /// </summary>
    public interface IPEImageBuilder
    {
        /// <summary>
        /// Constructs a PE image from a .NET module.
        /// </summary>
        /// <param name="module">The module to serialize.</param>
        /// <returns>The constructed PE image.</returns>
        PEImageBuildResult CreateImage(ModuleDefinition module);
    }
}