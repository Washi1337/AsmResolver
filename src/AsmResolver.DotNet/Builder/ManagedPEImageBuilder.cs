using System;
using AsmResolver.DotNet.Builder.Metadata;
using AsmResolver.PE;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Guid;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.UserStrings;

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
            
            ImportTablesStreamIfSpecified(dotNetDirectoryBuffer, module);
            
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
                metadataBuffer.BlobStream.ImportStream(originalMetadata.GetStream<BlobStream>());

            // Import original contents of the GUID stream if specified.
            if ((BuilderParameters.MetadataBuilderFlags & MetadataBuilderFlags.PreserveGuidIndices) != 0)
                metadataBuffer.GuidStream.ImportStream(originalMetadata.GetStream<GuidStream>());

            // Import original contents of the strings stream if specified.
            if ((BuilderParameters.MetadataBuilderFlags & MetadataBuilderFlags.PreserveStringIndices) != 0)
                metadataBuffer.StringsStream.ImportStream(originalMetadata.GetStream<StringsStream>());

            // Import original contents of the strings stream if specified.
            if ((BuilderParameters.MetadataBuilderFlags & MetadataBuilderFlags.PreserveUserStringIndices) != 0)
                metadataBuffer.UserStringsStream.ImportStream(originalMetadata.GetStream<UserStringsStream>());

            return metadataBuffer;
        }
        
        private void ImportTablesStreamIfSpecified(DotNetDirectoryBuffer buffer, ModuleDefinition module)
        {
            if ((BuilderParameters.MetadataBuilderFlags & MetadataBuilderFlags.PreserveTypeReferenceIndices) != 0)
                ImportTableIntoTableBuffers<TypeReference>(module, TableIndex.TypeRef, buffer.GetTypeReferenceToken);
        }

        private static void ImportTableIntoTableBuffers<TMember>(ModuleDefinition module, TableIndex tableIndex,
            Func<TMember, MetadataToken> importAction)
        {
            int count = module.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable(tableIndex)
                .Count;

            for (uint rid = 1; rid <= count; rid++)
                importAction((TMember) module.LookupMember(new MetadataToken(tableIndex, rid)));
        }
        
    }
}