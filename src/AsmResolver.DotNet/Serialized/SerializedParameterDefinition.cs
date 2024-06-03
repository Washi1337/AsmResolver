using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="ParameterDefinition"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedParameterDefinition : ParameterDefinition
    {
        private readonly ModuleReaderContext _context;
        private readonly ParameterDefinitionRow _row;

        /// <summary>
        /// Creates a parameter definition from a parameter metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the parameter for.</param>
        /// <param name="row">The metadata table row to base the parameter definition on.</param>
        public SerializedParameterDefinition(ModuleReaderContext context, MetadataToken token, in ParameterDefinitionRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            Sequence = row.Sequence;
            Attributes = row.Attributes;
        }

        /// <inheritdoc />
        protected override Utf8String? GetName() => _context.StringsStream?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override MethodDefinition? GetMethod()
        {
            var ownerToken = new MetadataToken(TableIndex.Method, _context.ParentModule.GetParameterOwner(MetadataToken.Rid));
            return _context.ParentModule.TryLookupMember(ownerToken, out var member)
                ? member as MethodDefinition
                : _context.BadImageAndReturn<MethodDefinition>(
                    $"Parameter {MetadataToken.ToString()} is not in a range of a method.");
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);

        /// <inheritdoc />
        protected override Constant? GetConstant() =>
            _context.ParentModule.GetConstant(MetadataToken);

        /// <inheritdoc />
        protected override MarshalDescriptor? GetMarshalDescriptor() =>
            _context.ParentModule.GetFieldMarshal(MetadataToken);

    }
}
