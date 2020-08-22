using System;

namespace AsmResolver.DotNet.Signatures
{
    public class BlobWriterContext
    {
        public BlobWriterContext(IBinaryStreamWriter writer, ITypeCodedIndexProvider indexProvider, DiagnosticBag diagnosticBag)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            IndexProvider = indexProvider ?? throw new ArgumentNullException(nameof(indexProvider));
            DiagnosticBag = diagnosticBag ?? throw new ArgumentNullException(nameof(diagnosticBag));
        }
        
        public IBinaryStreamWriter Writer
        {
            get;
        }

        public ITypeCodedIndexProvider IndexProvider
        {
            get;
        }

        public DiagnosticBag DiagnosticBag
        {
            get;
        }
    }
}