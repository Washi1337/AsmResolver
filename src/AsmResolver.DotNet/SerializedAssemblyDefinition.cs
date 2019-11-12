using System;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="AssemblyDefinition"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedAssemblyDefinition : AssemblyDefinition
    {
        private readonly IMetadata _metadata;
        private readonly AssemblyDefinitionRow _row;
        private readonly ModuleDefinition _manifestModule;

        /// <summary>
        /// Creates an assembly definition from an assembly metadata row.
        /// </summary>
        /// <param name="metadata">The object providing access to the underlying metadata streams.</param>
        /// <param name="token">The token to initialize the module for.</param>
        /// <param name="row">The metadata table row to base the assembly definition on.</param>
        /// <param name="manifestModule">The instance containing the manifest module definition.</param>
        public SerializedAssemblyDefinition(IMetadata metadata, MetadataToken token, AssemblyDefinitionRow row, ModuleDefinition manifestModule) 
            : base(token)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _row = row;
            _manifestModule = manifestModule ?? throw new ArgumentNullException(nameof(manifestModule));
            
            Attributes = row.Attributes;
            Version = new Version(row.MajorVersion, row.MinorVersion, row.BuildNumber, row.RevisionNumber);;
        }

        /// <inheritdoc />
        protected override string GetName() => _metadata.GetStream<StringsStream>()?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override string GetCulture() => _metadata.GetStream<StringsStream>()?.GetStringByIndex(_row.Culture);

        /// <inheritdoc />
        protected override byte[] GetPublicKey() => _metadata.GetStream<BlobStream>()?.GetBlobByIndex(_row.PublicKey);
    }
}