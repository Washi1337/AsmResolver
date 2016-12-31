using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public class MethodSpecificationTable : MetadataTable<MethodSpecification>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.MethodSpec; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.GetTable<MethodDefinition>().IndexSize +
                   (uint)TableStream.BlobIndexSize;
        }

        protected override MethodSpecification ReadMember( MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new MethodSpecification(Header, token, new MetadataRow<uint, uint>()
            {
                Column1 = reader.ReadIndex(TableStream.GetTable<MethodDefinition>().IndexSize),
                Column2 = reader.ReadIndex(TableStream.BlobIndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, MethodSpecification member)
        {
            var row = member.MetadataRow;
            row.Column1 = TableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef).EncodeToken(member.Method.MetadataToken);
            row.Column2 = context.GetStreamBuffer<BlobStreamBuffer>().GetBlobOffset(member.Signature);
        }

        protected override void WriteMember(WritingContext context, MethodSpecification member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteIndex(TableStream.GetTable<MethodDefinition>().IndexSize, row.Column1);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column2);
        }
    }

    public class MethodSpecification : MetadataMember<MetadataRow<uint, uint>>, ICallableMemberReference
    {
        private readonly LazyValue<IMethodDefOrRef> _method;
        private readonly LazyValue<GenericInstanceMethodSignature> _signature;
        private string _fullName;
        private CustomAttributeCollection _customAttributes;

        internal MethodSpecification(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();

            _method = new LazyValue<IMethodDefOrRef>(() =>
            {
                var methodToken = tableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef).DecodeIndex(row.Column1);
                return methodToken.Rid != 0 ? (IMethodDefOrRef)tableStream.ResolveMember(methodToken) : null;
            });

            _signature = new LazyValue<GenericInstanceMethodSignature>(() =>
                GenericInstanceMethodSignature.FromReader(header, header.GetStream<BlobStream>().CreateBlobReader(row.Column2)));
        }

        public IMethodDefOrRef Method
        {
            get { return _method.Value; }
            set
            {
                _method.Value = value;
                _fullName = null;
            }
        }

        public GenericInstanceMethodSignature Signature
        {
            get { return _signature.Value; }
            set
            {
                _signature.Value = value;
                _fullName = null;
            }
        }

        CallingConventionSignature ICallableMemberReference.Signature
        {
            get { return Method.Signature; }
        }

        public string Name
        {
            get
            {
                return Method.Name;
            }
        }

        public string FullName
        {
            get
            {
                if (_fullName != null)
                    return _fullName;

                var parameterString = Method.Signature != null && Method.Signature.IsMethod
                    ? '(' +
                      string.Join(", ",
                          ((MethodSignature)Method.Signature).Parameters.Select(x => x.ParameterType.FullName)) + ')'
                    : string.Empty;

                return _fullName = Method.DeclaringType.FullName + "::" + Name + '<' + string.Join(", ", Signature.GenericArguments.Select(x => x.FullName)) + '>' + parameterString;
            }
        }

        public ITypeDefOrRef DeclaringType
        {
            get { return Method.DeclaringType; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get { return _customAttributes ?? (_customAttributes = new CustomAttributeCollection(this)); }
        }


        public MethodDefinition Resolve()
        {
            return (MethodDefinition)Method.Resolve();
        }

        IMetadataMember IResolvable.Resolve()
        {
            return Resolve();
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
