using System;
using AsmResolver.DotNet.Code.Cil;

namespace AsmResolver.DotNet.Code
{
    /// <summary>
    /// Describes an environment in which a method body serializer currently exists in.
    /// </summary>
    public class MethodBodySerializationContext
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MethodBodySerializationContext"/> class.
        /// </summary>
        /// <param name="tokenProvider">
        /// The object responsible for providing new metadata tokens to members referenced by instructions.
        /// </param>
        /// <param name="diagnosticBag">
        /// The bag that is used to collect diagnostic information during the serialization process.
        /// </param>
        public MethodBodySerializationContext(IMetadataTokenProvider tokenProvider, DiagnosticBag diagnosticBag)
        {
            TokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            DiagnosticBag = diagnosticBag ?? throw new ArgumentNullException(nameof(diagnosticBag));
        }
        
        /// <summary>
        /// Gets the object responsible for providing new metadata tokens to members referenced by instructions.
        /// </summary>
        public IMetadataTokenProvider TokenProvider
        {
            get;
        }

        /// <summary>
        /// Gets the bag that is used to collect diagnostic information during the serialization process.
        /// </summary>
        public DiagnosticBag DiagnosticBag
        {
            get;
        }
    }
}