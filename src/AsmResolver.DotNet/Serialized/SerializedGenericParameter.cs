using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="GenericParameter"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedGenericParameter : GenericParameter
    {
        private readonly IMetadata _metadata;
        private readonly SerializedModuleDefinition _parentModule;
        private readonly GenericParameterRow _row;

        /// <summary>
        /// Creates a generic parameter from a generic parameter metadata row.
        /// </summary>
        /// <param name="metadata">The object providing access to the underlying metadata streams.</param>
        /// <param name="parentModule">The module that contains the generic parameter.</param>
        /// <param name="token">The token to initialize the generic parameter for.</param>
        /// <param name="row">The metadata table row to base the generic parameter on.</param>
        public SerializedGenericParameter(IMetadata metadata, SerializedModuleDefinition parentModule, MetadataToken token, GenericParameterRow row)
            : base(token)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;

            Attributes = _row.Attributes;
            Number = row.Number;            
        }

        /// <inheritdoc />
        protected override string GetName() =>
            _metadata.GetStream<StringsStream>().GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override IHasGenericParameters GetOwner()
        {
            var encoder = _metadata.GetStream<TablesStream>().GetIndexEncoder(CodedIndex.TypeOrMethodDef);
            var ownerToken = encoder.DecodeIndex(_row.Owner);
            return _parentModule.TryLookupMember(ownerToken, out var member)
                ? member as IHasGenericParameters
                : null;
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _parentModule.GetCustomAttributeCollection(this);
    }
}