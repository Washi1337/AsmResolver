using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.Serialized
{
    public class SerializedLocalConstant : LocalConstant
    {
        private readonly PdbReaderContext _context;
        private readonly LocalConstantRow _row;

        public SerializedLocalConstant(PdbReaderContext context, MetadataToken token, in LocalConstantRow row)
            : base(token)
        {
            _context = context;
            _row = row;
        }

        protected override LocalScope? GetOwner() => throw new NotImplementedException();

        protected override LocalConstantSignature? GetSignature()
        {
            if (_context.BlobStream?.TryGetBlobReaderByIndex(_row.Signature, out var reader) ?? false)
            {
                var context = new BlobReaderContext(_context.OwningModule.ReaderContext);
                return LocalConstantSignature.FromReader(ref context, ref reader);
            }
            return null;
        }
    }
}
