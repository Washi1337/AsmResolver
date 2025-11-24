using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="StandAloneSignature"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedStandAloneSignature : StandAloneSignature
    {
        private readonly ModuleReaderContext _context;
        private readonly StandAloneSignatureRow _row;

        /// <summary>
        /// Creates a stand-alone signature from a stand-alone sig metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the signature for.</param>
        /// <param name="row">The metadata table row to base the signature on.</param>
        public SerializedStandAloneSignature(
            ModuleReaderContext context,
            MetadataToken token,
            in StandAloneSignatureRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;
        }

        /// <inheritdoc />
        public override bool HasCustomAttributes => CustomAttributesInternal is null
            ? _context.ParentModule.HasNonEmptyCustomAttributes(this)
            : CustomAttributes.Count > 0;

        /// <inheritdoc />
        protected override CallingConventionSignature? GetSignature()
        {
            if (_context.BlobStream is not { } blobStream
                || !blobStream.TryGetBlobReaderByIndex(_row.Signature, out var reader))
            {
                return _context.BadImageAndReturn<CallingConventionSignature>(
                    $"Invalid signature blob index in stand-alone signature {MetadataToken.ToString()}.");
            }

            var context = new BlobReaderContext(_context);
            return CallingConventionSignature.FromReader(ref context, ref reader);
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}
