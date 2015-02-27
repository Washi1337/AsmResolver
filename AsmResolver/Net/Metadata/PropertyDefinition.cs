using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public class PropertyDefinitionTable : MetadataTable<PropertyDefinition>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Property; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof(ushort) +
                   (uint)TableStream.StringIndexSize +
                   (uint)TableStream.BlobIndexSize;
        }

        protected override PropertyDefinition ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new PropertyDefinition(Header, token, new MetadataRow<ushort, uint, uint>()
            {
                Column1 = reader.ReadUInt16(),
                Column2 = reader.ReadIndex(TableStream.StringIndexSize),
                Column3 = reader.ReadIndex(TableStream.BlobIndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, PropertyDefinition member)
        {
            var row = member.MetadataRow;
            row.Column1 = (ushort)member.Attributes;
            row.Column2 = context.GetStreamBuffer<StringStreamBuffer>().GetStringOffset(member.Name);
            row.Column3 = context.GetStreamBuffer<BlobStreamBuffer>().GetBlobOffset(member.Signature);
        }

        protected override void WriteMember(WritingContext context, PropertyDefinition member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteUInt16(row.Column1);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column2);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column3);
        }
    }

    public class PropertyDefinition : MetadataMember<MetadataRow<ushort, uint, uint>>, IHasConstant, IHasSemantics, ICollectionItem
    {
        private CustomAttributeCollection _customAttributes;
        private PropertyMap _propertyMap;
        private string _fullName;
        private PropertySignature _signature;
        private MethodSemanticsCollection _semantics;

        internal PropertyDefinition(MetadataHeader header, MetadataToken token, MetadataRow<ushort, uint, uint> row)
            : base(header, token, row)
        {
            Attributes = (PropertyAttributes)row.Column1;
            Name = header.GetStream<StringStream>().GetStringByOffset(row.Column2);
            Signature = PropertySignature.FromReader(header, header.GetStream<BlobStream>().CreateBlobReader(row.Column3));
        }

        public PropertyAttributes Attributes
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string FullName
        {
            get
            {
                if (_fullName != null)
                    return _fullName;
                return _fullName = this.GetFullName(Signature);
            }
        }

        public PropertySignature Signature
        {
            get { return _signature; }
            set
            {
                _signature = value;
                _fullName = null;
            }
        }

        public PropertyMap PropertyMap
        {
            get
            {
                if (_propertyMap != null)
                    return _propertyMap;

                return _propertyMap =
                    Header.GetStream<TableStream>()
                        .GetTable<PropertyMap>()
                        .FirstOrDefault(x => x.Properties.Contains(this));
            }
        }

        public TypeDefinition DeclaringType
        {
            get { return PropertyMap.Parent; }
        }

        ITypeDefOrRef IMemberReference.DeclaringType
        {
            get { return DeclaringType; }
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

        public MethodSemanticsCollection Semantics
        {
            get
            {
                if (_semantics != null)
                    return _semantics;
                return _semantics = new MethodSemanticsCollection(this);
            }
        }

        public MethodDefinition GetMethod
        {
            get
            {
                var semantic = Semantics.FirstOrDefault(x => x.Attributes.HasFlag(MethodSemanticsAttributes.Getter));
                return semantic != null ? semantic.Method : null;
            }
        }

        public MethodDefinition SetMethod
        {
            get
            {
                var semantic = Semantics.FirstOrDefault(x => x.Attributes.HasFlag(MethodSemanticsAttributes.Setter));
                return semantic != null ? semantic.Method : null;
            }
        }

        public Constant Constant
        {
            get;
            set;
        }

        public object Owner
        {
            get { return PropertyMap; }
            set { _propertyMap = value as PropertyMap; }
        }
    }
}