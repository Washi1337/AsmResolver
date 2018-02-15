using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class ModuleReference : MetadataMember<MetadataRow<uint>>, IHasCustomAttribute, IMemberRefParent, IResolutionScope
    {
        private readonly LazyValue<string> _name;
        private CustomAttributeCollection _customAttributes;

        public ModuleReference(string name)
            : base(null, new MetadataToken(MetadataTokenType.ModuleRef))
        {
            _name = new LazyValue<string>(name);
        }

        internal ModuleReference(MetadataImage image, MetadataRow<uint> row)
            : base(image, row.MetadataToken)
        {
            _name = new LazyValue<string>(() => image.Header.GetStream<StringStream>().GetStringByOffset(row.Column1));
        }

        public string Name
        {
            get { return _name.Value; }
            set { _name.Value = value; }
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
