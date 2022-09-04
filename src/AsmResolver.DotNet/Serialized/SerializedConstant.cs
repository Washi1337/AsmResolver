using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="Constant"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedConstant : Constant
    {
        private readonly ModuleReaderContext _context;
        private readonly ConstantRow _row;

        /// <summary>
        /// Creates a constant from a constant metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the constant for.</param>
        /// <param name="row">The metadata table row to base the constant on.</param>
        public SerializedConstant(
            ModuleReaderContext context,
            MetadataToken token,
            in ConstantRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            Type = row.Type;
        }

        /// <inheritdoc />
        protected override IHasConstant? GetParent()
        {
            var token = _context.ParentModule.GetConstantOwner(MetadataToken.Rid);
            return _context.ParentModule.TryLookupMember(token, out var member)
                ? member as IHasConstant
                : _context.BadImageAndReturn<IHasConstant>($"Invalid parent member in constant {MetadataToken.ToString()}.");
        }

        /// <inheritdoc />
        protected override DataBlobSignature? GetValue()
        {
            if (_context.BlobStream is not { } blobStream
                || !blobStream.TryGetBlobReaderByIndex(_row.Value, out var reader))
            {
                // Don't report error. null constants are allowed (e.g. null strings).
                return null;
            }

            return DataBlobSignature.FromReader(ref reader);
        }
    }
}
