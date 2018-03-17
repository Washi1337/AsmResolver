using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class ModuleReference : MetadataMember<MetadataRow<uint>>, IHasCustomAttribute, IMemberRefParent, IResolutionScope
    {
        private readonly LazyValue<string> _name;

        public ModuleReference(string name)
            : base(new MetadataToken(MetadataTokenType.ModuleRef))
        {
            _name = new LazyValue<string>(name);
            CustomAttributes = new CustomAttributeCollection(this);
        }

        internal ModuleReference(MetadataImage image, MetadataRow<uint> row)
            : base(row.MetadataToken)
        {
            Referrer = image.Assembly;
            _name = new LazyValue<string>(() => image.Header.GetStream<StringStream>().GetStringByOffset(row.Column1));
            CustomAttributes = new CustomAttributeCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return Referrer != null ? Referrer.Image : null; }
        }

        public AssemblyDefinition Referrer
        {
            get;
            internal set;
        }

        public string Name
        {
            get { return _name.Value; }
            set { _name.Value = value; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
