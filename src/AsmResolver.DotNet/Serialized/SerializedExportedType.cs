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
    /// Represents a lazily initialized implementation of <see cref="ExportedType"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedExportedType : ExportedType
    {
        private readonly ModuleReaderContext _context;
        private readonly ExportedTypeRow _row;

        /// <summary>
        /// Creates a exported type from a exported type metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the exported type for.</param>
        /// <param name="row">The metadata table row to base the exported type on.</param>
        public SerializedExportedType(ModuleReaderContext context, MetadataToken token, in ExportedTypeRow row)
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
        protected override string? GetNamespace()
        {
            return _context.Metadata.TryGetStream<StringsStream>(out var stringsStream)
                ? stringsStream.GetStringByIndex(_row.Namespace)
                : null;
        }

        /// <inheritdoc />
        protected override IImplementation? GetImplementation()
        {
            var encoder = _context.Metadata
                .GetStream<TablesStream>()
                .GetIndexEncoder(CodedIndex.Implementation);

            var token = encoder.DecodeIndex(_row.Implementation);
            return _context.ParentModule.TryLookupMember(token, out var member)
                ? member as IImplementation
                : _context.BadImageAndReturn<IImplementation>(
                    $"Invalid implementation reference in exported type {MetadataToken.ToString()}.");
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}
