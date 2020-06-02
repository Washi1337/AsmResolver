using AsmResolver.PE;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides a default implementation of <see cref="IPEImageBuilder"/>.
    /// </summary>
    public class ManagedPEImageBuilder : IPEImageBuilder
    {
        /// <summary>
        /// Gets or sets the factory responsible for constructing the .NET data directory.
        /// </summary>
        public IDotNetDirectoryFactory DotNetDirectoryFactory
        {
            get;
            set;
        } = new DotNetDirectoryFactory();
        
        /// <inheritdoc />
        public IPEImage CreateImage(ModuleDefinition module)
        {
            var image = new PEImage();
            image.DotNetDirectory = DotNetDirectoryFactory.CreateDotNetDirectory(module);
            return image;
        }
    }
}