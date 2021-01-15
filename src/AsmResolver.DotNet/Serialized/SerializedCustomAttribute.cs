using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="CustomAttribute"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedCustomAttribute : CustomAttribute
    {
        private readonly ModuleReadContext _context;
        private readonly CustomAttributeRow _row;

        /// <summary>
        /// Creates a custom attribute from a custom attribute metadata row.
        /// </summary>
        /// <param name="context">The reader context..</param>
        /// <param name="token">The token to initialize the custom attribute for.</param>
        /// <param name="row">The metadata table row to base the custom attribute on.</param>
        public SerializedCustomAttribute(ModuleReadContext context, MetadataToken token, in CustomAttributeRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;
        }

        /// <inheritdoc />
        protected override IHasCustomAttribute GetParent()
        {
            var ownerToken = _context.ParentModule.GetCustomAttributeOwner(MetadataToken.Rid);
            return _context.ParentModule.TryLookupMember(ownerToken, out var member)
                ? member as IHasCustomAttribute
                : null;
        }

        /// <inheritdoc />
        protected override ICustomAttributeType GetConstructor()
        {
            var tablesStream = _context.Image.DotNetDirectory.Metadata.GetStream<TablesStream>();
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.CustomAttributeType);

            var token = encoder.DecodeIndex(_row.Type);
            return _context.ParentModule.TryLookupMember(token, out var member)
                ? member as ICustomAttributeType
                : null;
        }

        /// <inheritdoc />
        protected override CustomAttributeSignature GetSignature()
        {
            return CustomAttributeSignature.FromReader(new BlobReadContext(_context), Constructor,
                _context.Image.DotNetDirectory.Metadata.GetStream<BlobStream>().GetBlobReaderByIndex(_row.Value));
        }
    }
}