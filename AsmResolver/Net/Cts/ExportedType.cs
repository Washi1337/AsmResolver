using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class ExportedType : MetadataMember<MetadataRow<TypeAttributes, uint, uint, uint, uint>>, IImplementation, IHasCustomAttribute
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<string> _namespace;
        private readonly LazyValue<IImplementation> _implementation;
        private CustomAttributeCollection _customAttributes;
        private MetadataImage _image;

        public ExportedType(IImplementation implementation, uint typeDefId, string name, string @namespace, TypeAttributes attributes)
            : base(new MetadataToken(MetadataTokenType.ExportedType))
        {
            Attributes = attributes;
            TypeDefId = typeDefId;
            _name = new LazyValue<string>(name);
            _namespace = new LazyValue<string>(@namespace);
            _implementation = new LazyValue<IImplementation>(implementation);
        }
        
        internal ExportedType(MetadataImage image, MetadataRow<TypeAttributes, uint, uint, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            Attributes = row.Column1;
            TypeDefId = row.Column2;
            
            _name = new LazyValue<string>(() 
                => image.Header.GetStream<StringStream>().GetStringByOffset(row.Column3));
            
            _namespace = new LazyValue<string>(() 
                => image.Header.GetStream<StringStream>().GetStringByOffset(row.Column4));
            
            _implementation = new LazyValue<IImplementation>(() =>
            {
                var encoder = image.Header.GetStream<TableStream>().GetIndexEncoder(CodedIndex.Implementation);
                var implementationToken = encoder.DecodeIndex(row.Column5);
                IMetadataMember member;
                return image.TryResolveMember(implementationToken, out member)
                    ? (IImplementation) member
                    : null;
            });
        }

        public override MetadataImage Image
        {
            get { return _image; }
        }

        public TypeAttributes Attributes
        {
            get;
            set;
        }

        public uint TypeDefId
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name.Value; }
            set { _name.Value = value; }
        }

        public string Namespace
        {
            get { return _namespace.Value; }
            set { _namespace.Value = value; }
        }

        public IImplementation Implementation
        {
            get { return _implementation.Value; }
            set { _implementation.Value = value; }
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
