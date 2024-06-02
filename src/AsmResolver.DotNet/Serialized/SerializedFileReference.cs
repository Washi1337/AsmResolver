using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="FileReference"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedFileReference : FileReference
    {
        private readonly ModuleReaderContext _context;
        private readonly FileReferenceRow _row;

        /// <summary>
        /// Creates a file reference from a file reference metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the reference for.</param>
        /// <param name="row">The metadata table row to base the member reference on.</param>
        public SerializedFileReference(
            ModuleReaderContext context,
            MetadataToken token,
            in FileReferenceRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            Attributes = row.Attributes;
        }

        /// <inheritdoc />
        protected override Utf8String? GetName() => _context.StringsStream?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override byte[]? GetHashValue() => _context.BlobStream?.GetBlobByIndex(_row.HashValue);

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}
