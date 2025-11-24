using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

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
        }

        /// <inheritdoc />
        public override bool HasCustomAttributes => CustomAttributesInternal is null
            ? _context.ParentModule.HasNonEmptyCustomAttributes(this)
            : CustomAttributes.Count > 0;

        /// <inheritdoc />
        public override bool HasConstraints => ConstraintsInternal is null
            ? _context.ParentModule.GetGenericParameterConstraints(MetadataToken).Count > 0
            : ConstraintsInternal.Count > 0;

        /// <inheritdoc />
        protected override Utf8String? GetName() => _context.StringsStream?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override IHasGenericParameters? GetOwner()
        {
            var ownerToken = _context.TablesStream
                .GetIndexEncoder(CodedIndex.TypeOrMethodDef)
                .DecodeIndex(_row.Owner);

            return _context.ParentModule.TryLookupMember(ownerToken, out var member)
                ? member as IHasGenericParameters
                : _context.BadImageAndReturn<IHasGenericParameters>(
                    $"Invalid owner in generic parameter {MetadataToken.ToString()}.");
        }

        /// <inheritdoc />
        protected override IList<GenericParameterConstraint> GetConstraints()
        {
            var module = _context.ParentModule;
            var rids = module.GetGenericParameterConstraints(MetadataToken);
            var result = new MemberCollection<GenericParameter, GenericParameterConstraint>(this, rids.Count);

            foreach (uint rid in rids)
            {
                var constraintToken = new MetadataToken(TableIndex.GenericParamConstraint, rid);
                result.AddNoOwnerCheck((GenericParameterConstraint) module.LookupMember(constraintToken));
            }

            return result;
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}
