using AsmResolver.Net.Builder;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class ModuleReference : MetadataMember<MetadataRow<uint>>, IHasCustomAttribute, IMemberRefParent, IResolutionScope
    {
        private readonly LazyValue<string> _name;

        public ModuleReference(string name)
            : base(null, new MetadataToken(MetadataTokenType.ModuleRef))
        {
            _name = new LazyValue<string>(name);
            CustomAttributes = new CustomAttributeCollection(this);
        }

        internal ModuleReference(MetadataImage image, MetadataRow<uint> row)
            : base(image, row.MetadataToken)
        {
            _name = new LazyValue<string>(() => image.Header.GetStream<StringStream>().GetStringByOffset(row.Column1));
            CustomAttributes = new CustomAttributeCollection(this);
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

        public override void AddToBuffer(MetadataBuffer buffer)
        {
            buffer.TableStreamBuffer.GetTable<ModuleReferenceTable>().Add(new MetadataRow<uint>
            {
                Column1 = buffer.StringStreamBuffer.GetStringOffset(Name)
            });

            foreach (var attribute in CustomAttributes)
                attribute.AddToBuffer(buffer);
        }
    }
}
