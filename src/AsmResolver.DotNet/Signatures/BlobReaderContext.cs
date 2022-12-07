using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides a context in which a metadata blob parser exists in. This includes the original module reader context
    /// as well as a mechanism to protect against infinite recursion.
    /// </summary>
    public struct BlobReaderContext
    {
        private Stack<MetadataToken>? _traversedTokens;

        /// <summary>
        /// Creates a new instance of the <see cref="BlobReaderContext"/> structure.
        /// </summary>
        /// <param name="readerContext">The original read context.</param>
        public BlobReaderContext(ModuleReaderContext readerContext)
            : this(readerContext, PhysicalTypeSignatureResolver.Instance)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BlobReaderContext"/> structure.
        /// </summary>
        /// <param name="readerContext">The original read context.</param>
        /// <param name="resolver">The object responsible for resolving raw type metadata tokens and addresses.</param>
        public BlobReaderContext(ModuleReaderContext readerContext, ITypeSignatureResolver resolver)
        {
            ReaderContext = readerContext;
            TypeSignatureResolver = resolver;
            _traversedTokens = null;
        }

        /// <summary>
        /// Gets the module reader context.
        /// </summary>
        public ModuleReaderContext ReaderContext
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for resolving raw type metadata tokens and addresses.
        /// </summary>
        public ITypeSignatureResolver TypeSignatureResolver
        {
            get;
        }

        /// <summary>
        /// Records a step in the blob reading process where a metadata token into the tables stream is about to
        /// be traversed.
        /// </summary>
        /// <param name="token">The token to traverse</param>
        /// <returns>
        /// <c>true</c> if this token was recorded, <c>false</c> if the token was already traversed before.
        /// </returns>
        public bool StepInToken(MetadataToken token)
        {
            if (_traversedTokens is null)
                _traversedTokens = new Stack<MetadataToken>();
            else if (_traversedTokens.Contains(token))
                return false;

            _traversedTokens.Push(token);
            return true;
        }

        /// <summary>
        /// Records a step in the blob reading process where the last recorded metadata token into the tables stream
        /// was traversed and processed completely.
        /// </summary>
        /// <exception cref="InvalidOperationException">Occurs when there was no token traversed.</exception>
        public void StepOutToken()
        {
            if (_traversedTokens is null)
                throw new InvalidOperationException();

            _traversedTokens.Pop();
        }
    }
}
