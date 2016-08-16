using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class TypeReferenceTable : MetadataTable<TypeReference>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.TypeRef; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.GetIndexEncoder(CodedIndex.ResolutionScope).IndexSize + // ResolutionScope
                   (uint)TableStream.StringIndexSize +                                       // Name
                   (uint)TableStream.StringIndexSize;                                        // Namespace
        }                                                                                    

        protected override TypeReference ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new TypeReference(Header, token, new MetadataRow<uint, uint, uint>
            {
                Column1 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.ResolutionScope).IndexSize), // Generation
                Column2 = reader.ReadIndex(TableStream.StringIndexSize),                                       // Name
                Column3 = reader.ReadIndex(TableStream.StringIndexSize),                                         // Mvid
            });
        }

        protected override void UpdateMember(NetBuildingContext context, TypeReference member)
        {
            var stringStream = context.GetStreamBuffer<StringStreamBuffer>();

            var row = member.MetadataRow;
            row.Column1 = TableStream.GetIndexEncoder(CodedIndex.ResolutionScope)
                .EncodeToken(member.ResolutionScope.MetadataToken);
            row.Column2 = stringStream.GetStringOffset(member.Name);
            row.Column3 = stringStream.GetStringOffset(member.Namespace);
        }

        protected override void WriteMember(WritingContext context, TypeReference member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.ResolutionScope).IndexSize, row.Column1);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column2);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column3);
        }
    }

    public class TypeReference : MetadataMember<MetadataRow<uint, uint, uint>>, ITypeDefOrRef, IResolutionScope
    {
        private CustomAttributeCollection _customAttributes;
        private readonly LazyValue<string> _name;
        private readonly LazyValue<string> _namespace;
        private string _fullName;

        public TypeReference(IResolutionScope resolutionScope, string @namespace, string name)
            : base(null, new MetadataToken(MetadataTokenType.TypeRef), new MetadataRow<uint, uint, uint>())
        {
            ResolutionScope = resolutionScope;
            _namespace = new LazyValue<string>(@namespace);
            _name = new LazyValue<string>(name);
        }

        internal TypeReference(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint, uint> row)
            : base(header, token, row)
        {
            var stringStream = header.GetStream<StringStream>();
            var tableStream = header.GetStream<TableStream>();

            var resolutionScopeToken = tableStream.GetIndexEncoder(CodedIndex.ResolutionScope).DecodeIndex(row.Column1);
            if (resolutionScopeToken.Rid != 0)
            {
                MetadataMember resolutionScope;
                if (tableStream.TryResolveMember(resolutionScopeToken, out resolutionScope))
                    ResolutionScope = resolutionScope as IResolutionScope;
            }

            _name = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column2));
            _namespace = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column3));
        }

        public ITypeDefOrRef DeclaringType
        {
            get { return ResolutionScope as ITypeDefOrRef; }
        }

        ITypeDescriptor ITypeDescriptor.DeclaringTypeDescriptor
        {
            get { return DeclaringType; }
        }

        public IResolutionScope ResolutionScope
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name.Value; }
            set
            {
                _name.Value = value;
                _fullName = null;
            }
        }

        public string Namespace
        {
            get { return _namespace.Value; }
            set
            {
                _namespace.Value = value;
                _fullName = null;
            }
        }

        public virtual string FullName
        {
            get
            {
                if (_fullName != null)
                    return _fullName;
                if (DeclaringType != null)
                    return _fullName = DeclaringType.FullName + '+' + Name;
                return _fullName = string.IsNullOrEmpty(Namespace) ? Name : Namespace + "." + Name;
            }
        }

        public bool IsValueType
        {
            get
            {
                var definition = Resolve();
                return definition != null && definition.IsValueType;
            }
        }
        
        public ITypeDescriptor GetElementType()
        {
            return this;
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

        public override string ToString()
        {
            return FullName;
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
    }
}
