using AsmResolver.DotNet.Blob;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a blob signature that can be referenced by metadata token. 
    /// </summary>
    /// <remarks>
    /// Stand-alone signatures are often used by the runtime for referencing local variable signatures, or serve
    /// as an operand for calli instructions.
    /// </remarks>
    public class StandAloneSignature : IMetadataMember
    {
        private readonly LazyVariable<CallingConventionSignature> _signature;
        
        /// <summary>
        /// Initializes a new stand-alone signature.
        /// </summary>
        /// <param name="token">The token of the stand-alone signature.</param>
        protected StandAloneSignature(MetadataToken token)
        {
            MetadataToken = token;
            _signature = new LazyVariable<CallingConventionSignature>(GetSignature);
        }
        
        /// <summary>
        /// Wraps a blob signature into a new stand-alone signature.
        /// </summary>
        /// <param name="signature">The signature to assign a metadata token.</param>
        public StandAloneSignature(CallingConventionSignature signature)
            : this(new MetadataToken(TableIndex.StandAloneSig, 0))
        {
            Signature = signature;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the signature that was referenced by this metadata member.
        /// </summary>
        public CallingConventionSignature Signature
        {
            get => _signature.Value;
            set => _signature.Value = value;
        }

        /// <summary>
        /// Obtains the signature referenced by this metadata member.
        /// </summary>
        /// <returns>The signature</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Signature"/> property.
        /// </remarks>
        protected virtual CallingConventionSignature GetSignature() => null;

        /// <inheritdoc />
        public override string ToString() => Signature.ToString();
    }
}