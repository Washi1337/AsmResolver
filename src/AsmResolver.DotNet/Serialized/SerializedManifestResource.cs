using System;
using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="ManifestResource"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedManifestResource : ManifestResource
    {
        private readonly ModuleReaderContext _context;
        private readonly ManifestResourceRow _row;

        /// <summary>
        /// Creates a manifest resource from a manifest resource metadata row.
        /// </summary>
        /// <param name="context">The module that contains the resource.</param>
        /// <param name="token">The token to initialize the resource for.</param>
        /// <param name="row">The metadata table row to base the resource \on.</param>
        public SerializedManifestResource(ModuleReaderContext context, MetadataToken token, in ManifestResourceRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            Attributes = row.Attributes;
            ((IOwnedCollectionElement<ModuleDefinition>) this).Owner = context.ParentModule;
        }

        /// <inheritdoc />
        protected override string? GetName()
        {
            return _context.Metadata.TryGetStream<StringsStream>(out var stringsStream)
                ? stringsStream.GetStringByIndex(_row.Name)
                : null;
        }

        /// <inheritdoc />
        protected override IImplementation? GetImplementation()
        {
            if (_row.Implementation == 0)
                return null;

            var encoder = _context.Metadata
                .GetStream<TablesStream>()
                .GetIndexEncoder(CodedIndex.Implementation);

            var token = encoder.DecodeIndex(_row.Implementation);
            return _context.ParentModule.TryLookupMember(token, out var member)
                ? member as IImplementation
                : _context.BadImageAndReturn<IImplementation>(
                    $"Invalid implementation in manifest resource {MetadataToken.ToString()}.");
        }

        /// <inheritdoc />
        protected override ISegment? GetEmbeddedDataSegment()
        {
            if (_row.Implementation != 0)
                return null;

            if (_context.Image.DotNetDirectory!.DotNetResources is not { } resources
                || resources.TryCreateManifestResourceReader(_row.Offset, out var reader))
            {
                return _context.BadImageAndReturn<ISegment>(
                    $"Invalid data offset in manifest resource {MetadataToken.ToString()}.");
            }

            return DataSegment.FromReader(ref reader);
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}
