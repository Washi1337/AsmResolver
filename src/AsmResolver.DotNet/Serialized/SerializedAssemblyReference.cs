using System;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="AssemblyReference"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedAssemblyReference : AssemblyReference
    {
        private readonly IMetadata _metadata;
        private readonly AssemblyReferenceRow _row;

        /// <summary>
        /// Creates an assembly reference from an assembly reference metadata row.
        /// </summary>
        /// <param name="metadata">The object providing access to the underlying metadata streams.</param>
        /// <param name="token">The token to initialize the reference for.</param>
        /// <param name="row">The metadata table row to base the assembly reference on.</param>
        public SerializedAssemblyReference(IMetadata metadata, MetadataToken token, AssemblyReferenceRow row)
            : base(token)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _row = row;
                
            Attributes = row.Attributes;
            Version = new Version(row.MajorVersion, row.MinorVersion, row.BuildNumber, row.RevisionNumber);
        }

        /// <inheritdoc />
        protected override string GetName() => _metadata.GetStream<StringsStream>()?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override string GetCulture() => _metadata.GetStream<StringsStream>()?.GetStringByIndex(_row.Culture);

        /// <inheritdoc />
        protected override byte[] GetPublicKeyOrToken() => _metadata.GetStream<BlobStream>()?.GetBlobByIndex(_row.PublicKeyOrToken);
        
    }
}