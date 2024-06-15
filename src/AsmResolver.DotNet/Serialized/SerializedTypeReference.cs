using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="TypeReference"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedTypeReference : TypeReference
    {
        private readonly ModuleReaderContext _context;
        private readonly TypeReferenceRow _row;

        /// <summary>
        /// Creates a type reference from a type metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the type for.</param>
        /// <param name="row">The metadata table row to base the type definition on.</param>
        public SerializedTypeReference(ModuleReaderContext context, MetadataToken token, in TypeReferenceRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            Module = context.ParentModule;
        }

        /// <inheritdoc />
        protected override Utf8String? GetNamespace() => _context.StringsStream?.GetStringByIndex(_row.Namespace);

        /// <inheritdoc />
        protected override Utf8String? GetName() => _context.StringsStream?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override IResolutionScope? GetScope()
        {
            if (_row.ResolutionScope == 0)
                return null;

            var tablesStream = _context.TablesStream;
            var decoder = tablesStream.GetIndexEncoder(CodedIndex.ResolutionScope);
            var token = decoder.DecodeIndex(_row.ResolutionScope);

            return !_context.ParentModule.TryLookupMember(token, out var scope)
                ? _context.BadImageAndReturn<IResolutionScope>($"Invalid resolution scope in type reference {MetadataToken}.")
                : scope as IResolutionScope;
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);

    }
}
