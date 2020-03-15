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
        private readonly SerializedModuleDefinition _parentModule;
        private readonly ImplementationMapRow _row;

        /// <summary>
        /// Creates a member reference from an implementation map metadata row.
        /// </summary>
        /// <param name="parentModule">The module that contains the mapping.</param>
        /// <param name="token">The token to initialize the mapping for.</param>
        /// <param name="row">The metadata table row to base the mapping on.</param>
        public SerializedImplementationMap(SerializedModuleDefinition parentModule,
            MetadataToken token, ImplementationMapRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;
        }

        /// <inheritdoc />
        protected override IMemberForwarded GetMemberForwarded()
        {
            var ownerToken = _parentModule.GetImplementationMapOwner(MetadataToken.Rid);
            return _parentModule.TryLookupMember(ownerToken, out var member)
                ? member as IMemberForwarded
                : null;
        }

        /// <inheritdoc />
        protected override string GetName() => _parentModule.DotNetDirectory.Metadata
            .GetStream<StringsStream>()
            .GetStringByIndex(_row.ImportName);

        /// <inheritdoc />
        protected override ModuleReference GetScope()
        {
            return _parentModule.TryLookupMember(new MetadataToken(TableIndex.ModuleRef, _row.ImportScope),
                out var member)
                ? member as ModuleReference
                : null;
        }
    }
}