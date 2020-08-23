using System;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Describes a context in which a blob signature is to be serialized in.
    /// </summary>
    public class BlobSerializationContext
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BlobSerializationContext"/> class.
        /// </summary>
        /// <param name="writer">The output stream to write the raw data to.</param>
        /// <param name="indexProvider">The object responsible for obtaining coded indices to types.</param>
        /// <param name="diagnosticBag">The bag used to collect diagnostic information during the serialization process.</param>
        public BlobSerializationContext(IBinaryStreamWriter writer, ITypeCodedIndexProvider indexProvider, DiagnosticBag diagnosticBag)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            IndexProvider = indexProvider ?? throw new ArgumentNullException(nameof(indexProvider));
            DiagnosticBag = diagnosticBag ?? throw new ArgumentNullException(nameof(diagnosticBag));
        }
        
        /// <summary>
        /// Gets the output stream to write the raw data to.
        /// </summary>
        public IBinaryStreamWriter Writer
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for obtaining coded indices to types.
        /// </summary>
        public ITypeCodedIndexProvider IndexProvider
        {
            get;
        }

        /// <summary>
        /// Gets the bag used to collect diagnostic information during the serialization process.
        /// </summary>
        public DiagnosticBag DiagnosticBag
        {
            get;
        }
    }
}