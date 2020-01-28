using System;
using System.Collections.Generic;
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
        private readonly SerializedModuleDefinition _parentModule;
        private readonly AssemblyReferenceRow _row;

        /// <summary>
        /// Creates an assembly reference from an assembly reference metadata row.
        /// </summary>
        /// <param name="parentModule">The module that contained the reference.</param>
        /// <param name="token">The token to initialize the reference for.</param>
        /// <param name="row">The metadata table row to base the assembly reference on.</param>
        public SerializedAssemblyReference(SerializedModuleDefinition parentModule, MetadataToken token, AssemblyReferenceRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;

            Attributes = row.Attributes;
            Version = new Version(row.MajorVersion, row.MinorVersion, row.BuildNumber, row.RevisionNumber);
        }

        /// <inheritdoc />
        protected override string GetName() => _parentModule.DotNetDirectory.Metadata
            .GetStream<StringsStream>()?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override string GetCulture() => _parentModule.DotNetDirectory.Metadata
            .GetStream<StringsStream>()?.GetStringByIndex(_row.Culture);

        /// <inheritdoc />
        protected override byte[] GetPublicKeyOrToken() => _parentModule.DotNetDirectory.Metadata
            .GetStream<BlobStream>()?.GetBlobByIndex(_row.PublicKeyOrToken);
      
        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _parentModule.GetCustomAttributeCollection(this);  
    }
}