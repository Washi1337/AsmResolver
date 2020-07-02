using System;
using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
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
        private readonly SerializedModuleDefinition _parentModule;
        private readonly GenericParameterRow _row;

        /// <summary>
        /// Creates a generic parameter from a generic parameter metadata row.
        /// </summary>
        /// <param name="parentModule">The module that contains the generic parameter.</param>
        /// <param name="token">The token to initialize the generic parameter for.</param>
        /// <param name="row">The metadata table row to base the generic parameter on.</param>
        public SerializedGenericParameter(SerializedModuleDefinition parentModule, MetadataToken token, GenericParameterRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;

            Attributes = _row.Attributes;
            Number = row.Number;            
        }

        /// <inheritdoc />
        protected override string GetName() => _parentModule.DotNetDirectory.Metadata
            .GetStream<StringsStream>()
            .GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override IHasGenericParameters GetOwner()
        {
            var encoder = _parentModule.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetIndexEncoder(CodedIndex.TypeOrMethodDef);
            
            var ownerToken = encoder.DecodeIndex(_row.Owner);
            return _parentModule.TryLookupMember(ownerToken, out var member)
                ? member as IHasGenericParameters
                : null;
        }

        /// <inheritdoc />
        protected override IList<GenericParameterConstraint> GetConstraints()
        {
            var result = new OwnedCollection<GenericParameter, GenericParameterConstraint>(this);

            foreach (uint rid in _parentModule.GetGenericParameterConstraints(MetadataToken))
            {
                var constraintToken = new MetadataToken(TableIndex.GenericParamConstraint, rid);
                result.Add((GenericParameterConstraint) _parentModule.LookupMember(constraintToken));
            }

            return result;
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _parentModule.GetCustomAttributeCollection(this);
    }
}