using System;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Describes a context in which a blob signature is to be serialized in.
    /// </summary>
    public readonly struct BlobSerializationContext
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BlobSerializationContext"/> class.
        /// </summary>
        /// <param name="writer">The output stream to write the raw data to.</param>
        /// <param name="indexProvider">The object responsible for obtaining coded indices to types.</param>
        /// <param name="errorListener">The object responsible for collecting diagnostic information during the serialization process.</param>
        /// <param name="diagnosticSource">When available, the object that is reported in diagnostics when serialization of the blob fails.</param>
        public BlobSerializationContext(
            BinaryStreamWriter writer,
            ITypeCodedIndexProvider indexProvider,
            IErrorListener errorListener,
            object? diagnosticSource)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            IndexProvider = indexProvider ?? throw new ArgumentNullException(nameof(indexProvider));
            ErrorListener = errorListener ?? throw new ArgumentNullException(nameof(errorListener));
            DiagnosticSource = diagnosticSource;
        }

        /// <summary>
        /// Gets the output stream to write the raw data to.
        /// </summary>
        public BinaryStreamWriter Writer
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
        public IErrorListener ErrorListener
        {
            get;
        }

        /// <summary>
        /// When available, gets the object that is reported in diagnostics when serialization of the blob fails.
        /// </summary>
        public object? DiagnosticSource
        {
            get;
        }
    }
}
