using System;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Code.Native;

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
        /// <param name="symbolsProvider">
        /// The object responsible for providing symbols referenced by native method bodies.
        /// </param>
        /// <param name="errorListener">
        /// The object responsible for collecting diagnostic information during the serialization process.
        /// </param>
        public MethodBodySerializationContext(IMetadataTokenProvider tokenProvider, INativeSymbolsProvider symbolsProvider, IErrorListener errorListener)
        {
            TokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            SymbolsProvider = symbolsProvider ?? throw new ArgumentNullException(nameof(symbolsProvider));
            ErrorListener = errorListener ?? throw new ArgumentNullException(nameof(errorListener));
        }

        /// <summary>
        /// Gets the object responsible for providing new metadata tokens to members referenced by instructions.
        /// </summary>
        public IMetadataTokenProvider TokenProvider
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for providing symbols referenced by native method bodies.
        /// </summary>
        public INativeSymbolsProvider SymbolsProvider
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for collecting diagnostic information during the serialization process.
        /// </summary>
        public IErrorListener ErrorListener
        {
            get;
        }
    }
}
