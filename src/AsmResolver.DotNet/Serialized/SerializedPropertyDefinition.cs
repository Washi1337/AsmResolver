using AsmResolver.DotNet.Blob;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="PropertyDefinition"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedPropertyDefinition : PropertyDefinition
    {
        private readonly IMetadata _metadata;
        private readonly SerializedModuleDefinition _parentModule;
        private readonly PropertyDefinitionRow _row;

        /// <summary>
        /// Creates a property definition from a property metadata row.
        /// </summary>
        /// <param name="metadata">The object providing access to the underlying metadata streams.</param>
        /// <param name="parentModule">The module that contains the property.</param>
        /// <param name="token">The token to initialize the field for.</param>
        /// <param name="row">The metadata table row to base the property definition on.</param>
        public SerializedPropertyDefinition(IMetadata metadata, SerializedModuleDefinition parentModule, MetadataToken token, PropertyDefinitionRow row)
            : base(token)
        {
            _metadata = metadata;
            _parentModule = parentModule;
            _row = row;

            Attributes = row.Attributes;
        }

        /// <inheritdoc />
        protected override string GetName() =>
            _metadata.GetStream<StringsStream>().GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override PropertySignature GetSignature()
        {
            var reader = _metadata.GetStream<BlobStream>().GetBlobReaderByIndex(_row.Type);
            return PropertySignature.FromReader(_parentModule, reader);
        }

        /// <inheritdoc />
        protected override TypeDefinition GetDeclaringType()
        {
            var declaringTypeToken = new MetadataToken(TableIndex.TypeDef, _parentModule.GetPropertyOwner(MetadataToken.Rid));
            return _parentModule.TryLookupMember(declaringTypeToken, out var member)
                ? member as TypeDefinition
                : null;
        }
    }
}