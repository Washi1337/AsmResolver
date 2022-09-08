using System;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="ImplementationMap"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedImplementationMap : ImplementationMap
    {
        private readonly ModuleReaderContext _context;
        private readonly ImplementationMapRow _row;

        /// <summary>
        /// Creates a member reference from an implementation map metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the mapping for.</param>
        /// <param name="row">The metadata table row to base the mapping on.</param>
        public SerializedImplementationMap(
            ModuleReaderContext context,
            MetadataToken token,
            in ImplementationMapRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            Attributes = _row.Attributes;
        }

        /// <inheritdoc />
        protected override IMemberForwarded? GetMemberForwarded()
        {
            var ownerToken = _context.ParentModule.GetImplementationMapOwner(MetadataToken.Rid);
            return _context.ParentModule.TryLookupMember(ownerToken, out var member)
                ? member as IMemberForwarded
                :  _context.BadImageAndReturn<IMemberForwarded>(
                    $"Invalid forwarded member in implementation map {MetadataToken.ToString()}.");;
        }

        /// <inheritdoc />
        protected override Utf8String? GetName() => _context.StringsStream?.GetStringByIndex(_row.ImportName);

        /// <inheritdoc />
        protected override ModuleReference? GetScope()
        {
            return _context.ParentModule.TryLookupMember(new MetadataToken(TableIndex.ModuleRef, _row.ImportScope),
                out var member)
                ? member as ModuleReference
                : _context.BadImageAndReturn<ModuleReference>(
                    $"Invalid import scope in implementation map {MetadataToken.ToString()}.");
        }
    }
}
