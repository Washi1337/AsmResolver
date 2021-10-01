using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="AssemblyReference"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedAssemblyReference : AssemblyReference
    {
        private readonly ModuleReaderContext _context;
        private readonly AssemblyReferenceRow _row;

        /// <summary>
        /// Creates an assembly reference from an assembly reference metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the reference for.</param>
        /// <param name="row">The metadata table row to base the assembly reference on.</param>
        public SerializedAssemblyReference(ModuleReaderContext context, MetadataToken token, in AssemblyReferenceRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            Attributes = row.Attributes;
            Version = new Version(row.MajorVersion, row.MinorVersion, row.BuildNumber, row.RevisionNumber);
        }

        /// <inheritdoc />
        protected override Utf8String? GetName()
        {
            return _context.Metadata.TryGetStream<StringsStream>(out var stringsStream)
                ? stringsStream.GetStringByIndex(_row.Name)
                : null;
        }

        /// <inheritdoc />
        protected override Utf8String? GetCulture()
        {
            return _context.Metadata.TryGetStream<StringsStream>(out var stringsStream)
                ? stringsStream.GetStringByIndex(_row.Culture)
                : null;
        }

        /// <inheritdoc />
        protected override byte[]? GetPublicKeyOrToken()
        {
            return _context.Metadata.TryGetStream<BlobStream>(out var blobStream)
                ? blobStream.GetBlobByIndex(_row.PublicKeyOrToken)
                : null;
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}
