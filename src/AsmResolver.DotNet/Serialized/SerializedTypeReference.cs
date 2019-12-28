using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="TypeReference"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedTypeReference : TypeReference
    {
        private readonly IMetadata _metadata;
        private readonly TypeReferenceRow _row;
        private readonly SerializedModuleDefinition _parentModule;

        /// <summary>
        /// Creates a type reference from a type metadata row.
        /// </summary>
        /// <param name="metadata">The object providing access to the underlying metadata streams.</param>
        /// <param name="parentModule">The module that references the type.</param>
        /// <param name="token">The token to initialize the type for.</param>
        /// <param name="row">The metadata table row to base the type definition on.</param>
        public SerializedTypeReference(IMetadata metadata, SerializedModuleDefinition parentModule, MetadataToken token, TypeReferenceRow row)
            : base(token)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            Module = _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;
        }

        /// <inheritdoc />
        protected override string GetNamespace() =>
            _metadata.GetStream<StringsStream>()?.GetStringByIndex(_row.Namespace);

        /// <inheritdoc />
        protected override string GetName() =>
            _metadata.GetStream<StringsStream>()?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override IResolutionScope GetScope()
        {
            var tablesStream = _metadata.GetStream<TablesStream>();
            var decoder = tablesStream.GetIndexEncoder(CodedIndex.ResolutionScope);
            var token = decoder.DecodeIndex(_row.ResolutionScope);

            return Module.LookupMember(token) as IResolutionScope;
        }
        
        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _parentModule.GetCustomAttributeCollection(this);
   
    }
}