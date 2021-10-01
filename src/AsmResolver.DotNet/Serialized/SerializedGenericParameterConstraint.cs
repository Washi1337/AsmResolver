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
        private readonly ModuleReaderContext _context;
        private readonly GenericParameterConstraintRow _row;

        /// <summary>
        /// Creates a generic parameter constraint from a generic parameter constraint metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the constraint for.</param>
        /// <param name="row">The metadata table row to base the constraint on.</param>
        public SerializedGenericParameterConstraint(
            ModuleReaderContext context,
            MetadataToken token,
            in GenericParameterConstraintRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;
        }

        /// <inheritdoc />
        protected override GenericParameter? GetOwner()
        {
            var token = _context.ParentModule.GetGenericParameterConstraintOwner(MetadataToken.Rid);
            return _context.ParentModule.TryLookupMember(token, out var member)
                ? member as GenericParameter
                : _context.BadImageAndReturn<GenericParameter>(
                    $"Invalid owner in generic parameter constraint {MetadataToken.ToString()}.");
        }

        /// <inheritdoc />
        protected override ITypeDefOrRef? GetConstraint()
        {
            var token = _context.Metadata
                .GetStream<TablesStream>()
                .GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .DecodeIndex(_row.Constraint);

            return _context.ParentModule.TryLookupMember(token, out var member)
                ? member as ITypeDefOrRef
                : _context.BadImageAndReturn<ITypeDefOrRef>(
                    $"Invalid constraint type in generic parameter constraint {MetadataToken.ToString()}.");
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}
