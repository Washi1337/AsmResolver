using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="GenericParameterConstraint"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedGenericParameterConstraint : GenericParameterConstraint
    {
        private readonly SerializedModuleDefinition _parentModule;
        private readonly GenericParameterConstraintRow _row;

        /// <summary>
        /// Creates a generic parameter from a generic parameter metadata row.
        /// </summary>
        /// <param name="parentModule">The module that contains the generic parameter.</param>
        /// <param name="token">The token to initialize the generic parameter for.</param>
        /// <param name="row">The metadata table row to base the generic parameter on.</param>
        public SerializedGenericParameterConstraint(SerializedModuleDefinition parentModule, MetadataToken token,
            GenericParameterConstraintRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;
        }

        /// <inheritdoc />
        protected override GenericParameter GetOwner()
        {
            var token = _parentModule.GetGenericParameterConstraintOwner(_row.Owner);
            return _parentModule.TryLookupMember(token, out var member)
                ? member as GenericParameter
                : null;
        }

        /// <inheritdoc />
        protected override ITypeDefOrRef GetConstraint()
        {
            var encoder = _parentModule.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetIndexEncoder(CodedIndex.TypeDefOrRef);

            var token = encoder.DecodeIndex(_row.Constraint);
            return _parentModule.TryLookupMember(token, out var member)
                ? member as ITypeDefOrRef
                : null;
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _parentModule.GetCustomAttributeCollection(this);
    }
}