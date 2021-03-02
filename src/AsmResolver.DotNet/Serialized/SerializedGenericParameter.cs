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
        private readonly ModuleReaderContext _context;
        private readonly GenericParameterRow _row;

        /// <summary>
        /// Creates a generic parameter from a generic parameter metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the generic parameter for.</param>
        /// <param name="row">The metadata table row to base the generic parameter on.</param>
        public SerializedGenericParameter(ModuleReaderContext context, MetadataToken token, in GenericParameterRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            Attributes = _row.Attributes;
            //Number = row.Number;
        }

        /// <inheritdoc />
        protected override string GetName() => _context.Image.DotNetDirectory.Metadata
            .GetStream<StringsStream>()
            .GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override IHasGenericParameters GetOwner()
        {
            var encoder = _context.Image.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetIndexEncoder(CodedIndex.TypeOrMethodDef);

            var ownerToken = encoder.DecodeIndex(_row.Owner);
            return _context.ParentModule.TryLookupMember(ownerToken, out var member)
                ? member as IHasGenericParameters
                : null;
        }

        /// <inheritdoc />
        protected override IList<GenericParameterConstraint> GetConstraints()
        {
            var result = new OwnedCollection<GenericParameter, GenericParameterConstraint>(this);

            var module = _context.ParentModule;
            foreach (uint rid in module.GetGenericParameterConstraints(MetadataToken))
            {
                var constraintToken = new MetadataToken(TableIndex.GenericParamConstraint, rid);
                result.Add((GenericParameterConstraint) module.LookupMember(constraintToken));
            }

            return result;
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}
