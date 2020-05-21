using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="TypeSpecification"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedTypeSpecification : TypeSpecification
    {
        private readonly SerializedModuleDefinition _parentModule;
        private readonly TypeSpecificationRow _row;

        /// <summary>
        /// Creates a type specification from a type metadata row.
        /// </summary>
        /// <param name="parentModule">The module that references the type.</param>
        /// <param name="token">The token to initialize the type for.</param>
        /// <param name="row">The metadata table row to base the type specification on.</param>
        public SerializedTypeSpecification(SerializedModuleDefinition parentModule, MetadataToken token, TypeSpecificationRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;
        }

        /// <inheritdoc />
        protected override TypeSignature GetSignature()
        {
            var reader = _parentModule.DotNetDirectory.Metadata
                .GetStream<BlobStream>()
                .GetBlobReaderByIndex(_row.Signature);
            
            var protection = RecursionProtection.CreateNew();
            protection.TraversedTokens.Add(MetadataToken);
            return TypeSignature.FromReader(_parentModule, reader, protection);
        }
        
        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _parentModule.GetCustomAttributeCollection(this);
    }
}