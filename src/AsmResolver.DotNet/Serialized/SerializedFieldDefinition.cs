using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
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
        private readonly SerializedModuleDefinition _parentModule;
        private readonly FieldDefinitionRow _row;

        /// <summary>
        /// Creates a field definition from a field metadata row.
        /// </summary>
        /// <param name="parentModule">The module that contains the field.</param>
        /// <param name="token">The token to initialize the field for.</param>
        /// <param name="row">The metadata table row to base the field definition on.</param>
        public SerializedFieldDefinition(SerializedModuleDefinition parentModule, MetadataToken token, FieldDefinitionRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;

            Attributes = row.Attributes;
        }

        /// <inheritdoc />
        protected override string GetName() => _parentModule.DotNetDirectory.Metadata
            .GetStream<StringsStream>()
            .GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override FieldSignature GetSignature() => FieldSignature.FromReader(_parentModule,
            _parentModule.DotNetDirectory.Metadata
                .GetStream<BlobStream>()
                .GetBlobReaderByIndex(_row.Signature));

        /// <inheritdoc />
        protected override TypeDefinition GetDeclaringType()
        {
            var declaringTypeToken = new MetadataToken(TableIndex.TypeDef, _parentModule.GetFieldDeclaringType(MetadataToken.Rid));
            return _parentModule.TryLookupMember(declaringTypeToken, out var member)
                ? member as TypeDefinition
                : null;
        }
        
        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _parentModule.GetCustomAttributeCollection(this);
    }
}