using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a reference to an external (unmanaged) module.
    /// </summary>
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
        public override MetadataImage Image => Referrer?.Image;

        /// <summary>
        /// Gets the assembly that refers to this module.
        /// </summary>
        public AssemblyDefinition Referrer
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the name of the external module.
        /// </summary>
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
