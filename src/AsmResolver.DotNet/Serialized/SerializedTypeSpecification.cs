using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="TypeSpecification"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedTypeSpecification : TypeSpecification
    {
        private readonly ModuleReadContext _context;
        private readonly TypeSpecificationRow _row;

        /// <summary>
        /// Creates a type specification from a type metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the type for.</param>
        /// <param name="row">The metadata table row to base the type specification on.</param>
        public SerializedTypeSpecification(ModuleReadContext context, MetadataToken token, in TypeSpecificationRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;
        }

        /// <inheritdoc />
        protected override TypeSignature GetSignature()
        {
            var reader = _context.Image.DotNetDirectory.Metadata
                .GetStream<BlobStream>()
                .GetBlobReaderByIndex(_row.Signature);

            var context = new BlobReadContext(_context);
            context.TraversedTokens.Add(MetadataToken);
            return TypeSignature.FromReader(context, reader);
        }
        
        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}