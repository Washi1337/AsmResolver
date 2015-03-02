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

        public TypeReference(IResolutionScope resolutionScope, string @namespace, string name)
            : base(null, new MetadataToken(MetadataTokenType.TypeRef), new MetadataRow<uint, uint, uint>())
        {
            ResolutionScope = resolutionScope;
            Namespace = @namespace;
            Name = name;
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

            Name = stringStream.GetStringByOffset(row.Column2);
            Namespace = stringStream.GetStringByOffset(row.Column3);
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
            get;
            set;
        }

        public string Namespace
        {
            get;
            set;
        }

        public virtual string FullName
        {
            get
            {
                if (DeclaringType != null)
                    return DeclaringType.FullName + '+' + Name;
                return string.IsNullOrEmpty(Namespace) ? Name : Namespace + "." + Name;
            }
        }

        public bool IsValueType
        {
            get;
            set;
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
    }
}
