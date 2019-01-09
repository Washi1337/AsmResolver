using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a blob signature that can be referred to by a metadata token.  
    /// </summary>
    /// <remarks>
    /// Usually this signature contains a local variable signature, but is also used for other purposes such as
    /// storing method signatures for calli instructions.
    /// </remarks>
    public class StandAloneSignature : MetadataMember<MetadataRow<uint>>, IHasCustomAttribute
    {
        private readonly LazyValue<CallingConventionSignature> _signature;

        public StandAloneSignature(CallingConventionSignature signature)
            : this(signature, null)
        {
        }

        public StandAloneSignature(CallingConventionSignature signature, MetadataImage image)
            : base(new MetadataToken(MetadataTokenType.StandAloneSig))
        {
            Image = image;
            _signature = new LazyValue<CallingConventionSignature>(signature);
            CustomAttributes = new CustomAttributeCollection(this);
        }

        internal StandAloneSignature(MetadataImage image, MetadataRow<uint> row)
            : base(row.MetadataToken)
        {
            Image = image;
            _signature = new LazyValue<CallingConventionSignature>(() =>
                image.Header.GetStream<BlobStream>().TryCreateBlobReader(row.Column1, out var reader)
                    ? CallingConventionSignature.FromReader(image, reader, true)
                    : null);

            CustomAttributes = new CustomAttributeCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get;
        }

        /// <summary>
        /// Gets the blob signature referenced by this stand-alone signature.
        /// </summary>
        public CallingConventionSignature Signature
        {
            get => _signature.Value;
            set => _signature.Value = value;
        }

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
        }
    }
}