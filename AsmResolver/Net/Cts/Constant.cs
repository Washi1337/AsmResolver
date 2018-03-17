using System;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a constant value for a field, property or parameter definition.
    /// </summary>
    public class Constant : MetadataMember<MetadataRow<ElementType, byte, uint, uint>>
    {
        private readonly LazyValue<IHasConstant> _parent;
        private readonly LazyValue<DataBlobSignature> _value;
        private MetadataImage _image;

        public Constant(ElementType constantType, DataBlobSignature value)
            : base(new MetadataToken(MetadataTokenType.Constant))
        {
            ConstantType = constantType;
            _value = new LazyValue<DataBlobSignature>(value);
            _parent = new LazyValue<IHasConstant>();
        }

        internal Constant(MetadataImage image, MetadataRow<ElementType, byte, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            
            ConstantType = row.Column1;

            _parent = new LazyValue<IHasConstant>(() =>
            {
                var tableStream = image.Header.GetStream<TableStream>();
                var hasConstantToken = tableStream.GetIndexEncoder(CodedIndex.HasConstant).DecodeIndex(row.Column3);
                return hasConstantToken.Rid != 0 ? (IHasConstant) image.ResolveMember(hasConstantToken) : null;
            });

            _value = new LazyValue<DataBlobSignature>(() => 
                DataBlobSignature.FromReader(image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column4)));
        }

        public override MetadataImage Image
        {
            get { return _parent.IsInitialized && _parent.Value != null ? _parent.Value.Image : _image; }
        }

        /// <summary>
        /// Gets or sets the element type of the constant.
        /// </summary>
        public ElementType ConstantType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the parent member that owns this constant value.
        /// </summary>
        public IHasConstant Parent
        {
            get { return _parent.Value; }
            internal set
            {
                _parent.Value = value;
                _image = null;
            }
        }

        /// <summary>
        /// Gets or sets the raw data blob containing the actual value of the constant.  
        /// </summary>
        public DataBlobSignature Value
        {
            get { return _value.Value; }
            set { _value.Value = value; }
        }
    }
}
