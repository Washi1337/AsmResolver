using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.Serialized
{
    public class SerializedMethodDebugInformation : MethodDebugInformation
    {
        private readonly PdbReaderContext _context;
        private readonly MethodDebugInformationRow _row;

        public SerializedMethodDebugInformation(PdbReaderContext context, MetadataToken token, in MethodDebugInformationRow row) : base(token)
        {
            _context = context;
            _row = row;
        }

        protected override Document? GetDocument() => _context.Pdb.LookupDocument(new MetadataToken(TableIndex.Document, _row.Document));

        protected override SequencePointCollection GetSequencePoints()
        {
            if (!_context.BlobStream!.TryGetBlobReaderByIndex(_row.SequencePoints, out var reader))
            {
                return new SequencePointCollection(this);
            }
            return SequencePointCollection.FromReader(_context, this, ref reader);
        }
    }
}
