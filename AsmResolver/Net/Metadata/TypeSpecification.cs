using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public class TypeSpecificationTable : MetadataTable<TypeSpecification> 
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.TypeSpec; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.BlobIndexSize;
        }

        protected override TypeSpecification ReadMember( MetadataToken token, ReadingContext context)
        {
            return new TypeSpecification(Header, token, new MetadataRow<uint>()
            {
                Column1 = context.Reader.ReadIndex(TableStream.BlobIndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, TypeSpecification member)
        {
            member.MetadataRow.Column1 = context.GetStreamBuffer<BlobStreamBuffer>().GetBlobOffset(member.Signature);
        }

        protected override void WriteMember(WritingContext context, TypeSpecification member)
        {
            context.Writer.WriteIndex(TableStream.BlobIndexSize, member.MetadataRow.Column1);
        }
    }

    public class TypeSpecification : MetadataMember<MetadataRow<uint>>, ITypeDefOrRef
    {
        private readonly LazyValue<TypeSignature> _signature;
        private CustomAttributeCollection _customAttributes;

        public TypeSpecification(TypeSignature signature)
            : base(null, new MetadataToken(MetadataTokenType.TypeSpec), new MetadataRow<uint>())
        {
            _signature = new LazyValue<TypeSignature>(signature);
        }

        internal TypeSpecification(MetadataHeader header, MetadataToken token, MetadataRow<uint> row)
            : base(header, token, row)
        {
            _signature = new LazyValue<TypeSignature>(() => 
                TypeSignature.FromReader(header, header.GetStream<BlobStream>().CreateBlobReader(row.Column1)));
        }

        public TypeSignature Signature
        {
            get { return _signature.Value; }
            set { _signature.Value = value; }
        }

        public string Name
        {
            get { return Signature.Name; }
        }

        public string Namespace
        {
            get { return Signature.Namespace; }
        }

        ITypeDescriptor ITypeDescriptor.DeclaringTypeDescriptor
        {
            get { return DeclaringType; }
        }

        public IResolutionScope ResolutionScope
        {
            get { return Signature.ResolutionScope; }
        }

        public virtual string FullName
        {
            get { return Signature.FullName; }
        }

        public bool IsValueType
        {
            get { return Signature.IsValueType; }
        }

        public ITypeDescriptor GetElementType()
        {
            return Signature.GetElementType();
        }

        public ITypeDefOrRef DeclaringType
        {
            get { return ResolutionScope as ITypeDefOrRef; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get
            {
                if (_customAttributes != null)
                    return _customAttributes;
                _customAttributes = new CustomAttributeCollection(this);
                return _customAttributes;
            }
        }

        public TypeDefinition Resolve()
        {
            if (Header == null || Header.MetadataResolver == null)
                throw new MemberResolutionException(this);
            return Header.MetadataResolver.ResolveType(this);
        }

        IMetadataMember IResolvable.Resolve()
        {
            return Resolve();
        }

        public override string ToString()
        {
            return Signature.FullName;
        }
    }
}
