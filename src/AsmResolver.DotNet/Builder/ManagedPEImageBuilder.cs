using AsmResolver.PE;
using AsmResolver.PE.DotNet;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides a default implementation of <see cref="IPEImageBuilder"/>.
    /// </summary>
    public class ManagedPEImageBuilder : IPEImageBuilder
    {
        /// <inheritdoc />
        public IPEImage CreateImage(ModuleDefinition module)
        {
            var image = new PEImage();

            image.DotNetDirectory = CreateDotNetDirectory(image, module);

            return image;
        }

        private IDotNetDirectory CreateDotNetDirectory(PEImage image, ModuleDefinition module)
        {
            var buffer = new DotNetDirectoryBuffer(module, new CilMethodBodySerializer(), new MetadataBuffer());
            
            // If module is the manifest module, include the entire assembly.
            if (module.Assembly?.ManifestModule == module)
                buffer.AddAssembly(module.Assembly);
            else
                buffer.AddModule(module);

            return buffer.CreateDirectory();
        }
    }
}