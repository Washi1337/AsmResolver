using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="TypeSpecification"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedTypeSpecification : TypeSpecification
    {
        private readonly ModuleReaderContext _context;
        private readonly TypeSpecificationRow _row;

        /// <summary>
        /// Creates a type specification from a type metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the type for.</param>
        /// <param name="row">The metadata table row to base the type specification on.</param>
        public SerializedTypeSpecification(ModuleReaderContext context, MetadataToken token, in TypeSpecificationRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;
        }

        /// <inheritdoc />
        protected override TypeSignature? GetSignature()
        {
            if (_context.BlobStream is not { } blobStream
                || !blobStream.TryGetBlobReaderByIndex(_row.Signature, out var reader))
            {
                return _context.BadImageAndReturn<TypeSignature>(
                    $"Invalid blob signature for type specification {MetadataToken.ToString()}");
            }

            var context = new BlobReaderContext(_context);
            context.StepInToken(MetadataToken);
            return TypeSignature.FromReader(ref context, ref reader);
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}
