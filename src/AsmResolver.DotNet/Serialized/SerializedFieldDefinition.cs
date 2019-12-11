using System;
using AsmResolver.DotNet.Blob;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="FieldDefinition"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedFieldDefinition : FieldDefinition
    {
        private readonly IMetadata _metadata;
        private readonly SerializedModuleDefinition _parentModule;
        private readonly FieldDefinitionRow _row;

        /// <summary>
        /// Creates a field definition from a field metadata row.
        /// </summary>
        /// <param name="metadata">The object providing access to the underlying metadata streams.</param>
        /// <param name="parentModule"></param>
        /// <param name="token">The token to initialize the field for.</param>
        /// <param name="row">The metadata table row to base the field definition on.</param>
        public SerializedFieldDefinition(IMetadata metadata, SerializedModuleDefinition parentModule, MetadataToken token, FieldDefinitionRow row)
            : base(token)
        {
            _metadata = metadata;
            _parentModule = parentModule;
            _row = row;
        }

        /// <inheritdoc />
        protected override string GetName() => 
            _metadata.GetStream<StringsStream>().GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override FieldSignature GetSignature() => 
            FieldSignature.FromReader(_parentModule,
                _metadata.GetStream<BlobStream>().GetBlobReaderByIndex(_row.Signature));

        /// <inheritdoc />
        protected override TypeDefinition GetDeclaringType()
        {
            var declaringTypeToken = new MetadataToken(TableIndex.TypeDef, _parentModule.GetFieldDeclaringType(MetadataToken.Rid));
            return _parentModule.TryLookupMember(declaringTypeToken, out var member)
                ? member as TypeDefinition
                : null;
        }
    }
}