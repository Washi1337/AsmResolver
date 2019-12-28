using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Blob;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="StandAloneSignature"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedStandAloneSignature : StandAloneSignature
    {    
        private readonly IMetadata _metadata;
        private readonly SerializedModuleDefinition _parentModule;
        private readonly StandAloneSignatureRow _row;
        
        /// <summary>
        /// Creates a stand-alone signature from a stand-alone sig metadata row.
        /// </summary>
        /// <param name="metadata">The object providing access to the underlying metadata streams.</param>
        /// <param name="parentModule"></param>
        /// <param name="token">The token to initialize the signature for.</param>
        /// <param name="row">The metadata table row to base the signature on.</param>
        public SerializedStandAloneSignature(IMetadata metadata, SerializedModuleDefinition parentModule,
            MetadataToken token, StandAloneSignatureRow row)
            : base(token)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;
        }

        /// <inheritdoc />
        protected override CallingConventionSignature GetSignature()
        {
            var reader = _metadata.GetStream<BlobStream>().GetBlobReaderByIndex(_row.Signature);
            return CallingConventionSignature.FromReader(_parentModule, reader);
        }
        
        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _parentModule.GetCustomAttributeCollection(this);
    }
}