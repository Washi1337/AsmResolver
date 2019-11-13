using System;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="TypeDefinition"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedTypeDefinition : TypeDefinition
    {
        private readonly IMetadata _metadata;
        private readonly TypeDefinitionRow _row;

        /// <summary>
        /// Creates a type definition from a type metadata row.
        /// </summary>
        /// <param name="metadata">The object providing access to the underlying metadata streams.</param>
        /// <param name="token">The token to initialize the type for.</param>
        /// <param name="row">The metadata table row to base the type definition on.</param>
        public SerializedTypeDefinition(IMetadata metadata, MetadataToken token, TypeDefinitionRow row)
            : base(token)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _row = row;
        }

        /// <inheritdoc />
        protected override string GetNamespace() =>
            _metadata.GetStream<StringsStream>()?.GetStringByIndex(_row.Namespace);

        /// <inheritdoc />
        protected override string GetName() =>
            _metadata.GetStream<StringsStream>()?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override ITypeDefOrRef GetBaseType()
        {
            // TODO
            return base.GetBaseType();
        }
    }
}