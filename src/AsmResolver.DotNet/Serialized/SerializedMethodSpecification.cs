using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="MethodSpecification"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedMethodSpecification : MethodSpecification
    {
        private readonly ModuleReaderContext _context;
        private readonly MethodSpecificationRow _row;

        /// <summary>
        /// Creates a method specification from a method specification metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the method specification for.</param>
        /// <param name="row">The metadata table row to base the method specification on.</param>
        public SerializedMethodSpecification(ModuleReaderContext context, MetadataToken token, in MethodSpecificationRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;
        }

        /// <inheritdoc />
        protected override IMethodDefOrRef? GetMethod()
        {
            var methodToken = _context.TablesStream
                .GetIndexEncoder(CodedIndex.MethodDefOrRef)
                .DecodeIndex(_row.Method);

            return _context.ParentModule.TryLookupMember(methodToken, out var member)
                ? member as IMethodDefOrRef
                : _context.BadImageAndReturn<IMethodDefOrRef>(
                    $"Invalid method in method specification {MetadataToken.ToString()}.");
        }

        /// <inheritdoc />
        protected override GenericInstanceMethodSignature? GetSignature()
        {
            if (_context.BlobStream is not { } blobStream
                || !blobStream.TryGetBlobReaderByIndex(_row.Instantiation, out var reader))
            {
                return _context.BadImageAndReturn<GenericInstanceMethodSignature>(
                    $"Invalid instantiation blob index in method specification {MetadataToken.ToString()}.");
            }

            var context = new BlobReaderContext(_context);
            return GenericInstanceMethodSignature.FromReader(ref context, ref reader);
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}
