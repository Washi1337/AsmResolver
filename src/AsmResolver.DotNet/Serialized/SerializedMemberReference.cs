using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="MemberReference"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedMemberReference : MemberReference
    {
        private readonly ModuleReaderContext _context;
        private readonly MemberReferenceRow _row;

        /// <summary>
        /// Creates a member reference from a member reference metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the reference for.</param>
        /// <param name="row">The metadata table row to base the member reference on.</param>
        public SerializedMemberReference(
            ModuleReaderContext context,
            MetadataToken token,
            in MemberReferenceRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;
        }

        /// <inheritdoc />
        protected override IMemberRefParent? GetParent()
        {
            var encoder =  _context.TablesStream
                .GetIndexEncoder(CodedIndex.MemberRefParent);

            var parentToken = encoder.DecodeIndex(_row.Parent);
            return _context.ParentModule.TryLookupMember(parentToken, out var member)
                ? member as IMemberRefParent
                : _context.BadImageAndReturn<IMemberRefParent>(
                    $"Invalid parent in member reference {MetadataToken.ToString()}.");
        }

        /// <inheritdoc />
        protected override Utf8String? GetName() => _context.StringsStream?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override CallingConventionSignature? GetSignature()
        {
            if (_context.BlobStream is not { } blobStream
                || !blobStream.TryGetBlobReaderByIndex(_row.Signature, out var reader))
            {
                return _context.BadImageAndReturn<CallingConventionSignature>(
                    $"Invalid signature blob index in member reference {MetadataToken.ToString()}.");
            }

            return CallingConventionSignature.FromReader(new BlobReadContext(_context), ref reader, true);
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}
