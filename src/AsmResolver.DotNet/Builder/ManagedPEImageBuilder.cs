using AsmResolver.DotNet.Builder.Metadata;
using AsmResolver.PE;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata.Blob;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides a default implementation of <see cref="IPEImageBuilder"/>.
    /// </summary>
    public class ManagedPEImageBuilder : IPEImageBuilder
    {
        /// <summary>
        /// Gets the parameters for constructing the .NET data directory. 
        /// </summary>
        public DotNetDirectoryBuilderParameters BuilderParameters
        {
            get;
        } = new DotNetDirectoryBuilderParameters();
        
        /// <inheritdoc />
        public IPEImage CreateImage(ModuleDefinition module)
        {
            var image = new PEImage();

            image.DotNetDirectory = CreateDotNetDirectory(image, module);

            return image;
        }

        private IDotNetDirectory CreateDotNetDirectory(PEImage image, ModuleDefinition module)
        {
            var metadataBuffer = CreateMetadataBuffer(module);
            var dotNetDirectoryBuffer = new DotNetDirectoryBuffer(module, BuilderParameters.MethodBodySerializer, metadataBuffer);
            
            // If module is the manifest module, include the entire assembly.
            if (module.Assembly?.ManifestModule == module)
                dotNetDirectoryBuffer.AddAssembly(module.Assembly);
            else
                dotNetDirectoryBuffer.AddModule(module);

            return dotNetDirectoryBuffer.CreateDirectory();
        }

        private IMetadataBuffer CreateMetadataBuffer(ModuleDefinition module)
        {
            var metadataBuffer = new MetadataBuffer();
            
            // Check if there exists a .NET directory to base off the metadata buffer on.
            var originalMetadata = module.DotNetDirectory?.Metadata;
            if (originalMetadata is null)
                return metadataBuffer;
            
            // Import original contents of the blob stream if specified.
            if ((BuilderParameters.MetadataBuilderFlags & MetadataBuilderFlags.PreserveBlobIndices) != 0)
                metadataBuffer.BlobStream.ImportBlobStream(originalMetadata.GetStream<BlobStream>());

            return metadataBuffer;
        }
    }
}