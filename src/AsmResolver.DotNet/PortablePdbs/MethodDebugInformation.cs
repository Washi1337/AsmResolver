using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs
{
    public class MethodDebugInformation : IMetadataMember
    {
        private readonly LazyVariable<MethodDebugInformation, Document?> _document;
        private readonly LazyVariable<MethodDebugInformation, SequencePointCollection> _sequencePoints;

        public MethodDebugInformation(Document document) : this(new MetadataToken(TableIndex.MethodDebugInformation, 0))
        {
            Document = document;
        }

        public MethodDebugInformation(MetadataToken token)
        {
            MetadataToken = token;

            _document = new(mdi => mdi.GetDocument());
            _sequencePoints = new(mdi => mdi.GetSequencePoints());
        }

        public MetadataToken MetadataToken { get; }

        public Document? Document
        {
            get => _document.GetValue(this);
            set => _document.SetValue(value);
        }

        public SequencePointCollection SequencePoints => _sequencePoints.GetValue(this);

        protected virtual Document? GetDocument() => null;

        protected virtual SequencePointCollection GetSequencePoints() => new(this);
    }
}
