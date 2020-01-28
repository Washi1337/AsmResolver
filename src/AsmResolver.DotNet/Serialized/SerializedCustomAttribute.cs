using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="CustomAttribute"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedCustomAttribute : CustomAttribute
    {
        private readonly SerializedModuleDefinition _parentModule;
        private readonly CustomAttributeRow _row;

        /// <summary>
        /// Creates a custom attribute from a custom attribute metadata row.
        /// </summary>
        /// <param name="parentModule">The module that contains the custom attribute.</param>
        /// <param name="token">The token to initialize the custom attribute for.</param>
        /// <param name="row">The metadata table row to base the custom attribute on.</param>
        public SerializedCustomAttribute(SerializedModuleDefinition parentModule, MetadataToken token, CustomAttributeRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;
        }

        /// <inheritdoc />
        protected override IHasCustomAttribute GetParent()
        {
            var ownerToken = _parentModule.GetCustomAttributeOwner(MetadataToken.Rid);
            return _parentModule.TryLookupMember(ownerToken, out var member)
                ? member as IHasCustomAttribute
                : null;
        }

        /// <inheritdoc />
        protected override ICustomAttributeType GetConstructor()
        {
            var tablesStream = _parentModule.DotNetDirectory.Metadata.GetStream<TablesStream>();
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.CustomAttributeType);

            var token = encoder.DecodeIndex(_row.Type);
            return _parentModule.TryLookupMember(token, out var member)
                ? member as ICustomAttributeType
                : null;
        }

        /// <inheritdoc />
        protected override CustomAttributeSignature GetSignature()
        {
            return CustomAttributeSignature.FromReader(_parentModule, Constructor,
                _parentModule.DotNetDirectory.Metadata.GetStream<BlobStream>().GetBlobReaderByIndex(_row.Value));
        }
    }
}