using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class StandAloneSignature : MetadataMember<MetadataRow<uint>>, IHasCustomAttribute
    {
        private CustomAttributeCollection _customAttributes;
        private readonly LazyValue<CallingConventionSignature> _signature;

        public StandAloneSignature(CallingConventionSignature signature)
            : this(signature, null)
        {
        }

        public StandAloneSignature(CallingConventionSignature signature, MetadataImage image)
            : base(image, new MetadataToken(MetadataTokenType.StandAloneSig))
        {
            _signature = new LazyValue<CallingConventionSignature>(signature);
        }

        internal StandAloneSignature(MetadataImage image, MetadataRow<uint> row)
            : base(image, row.MetadataToken)
        {
            _signature = new LazyValue<CallingConventionSignature>(() =>
            {
                IBinaryStreamReader reader;
                return image.Header.GetStream<BlobStream>().TryCreateBlobReader(row.Column1, out reader)
                    ? CallingConventionSignature.FromReader(image, reader)
                    : null;
            });
        }

        public CallingConventionSignature Signature
        {
            get { return _signature.Value; }
            set { _signature.Value = value; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get
            {
                if (_customAttributes != null)
                    return _customAttributes;
                return _customAttributes = new CustomAttributeCollection(this);
            }
        }
    }
}