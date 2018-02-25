
using System.Linq;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class MethodSpecification : MetadataMember<MetadataRow<uint, uint>>, ICallableMemberReference
    {
        private readonly LazyValue<IMethodDefOrRef> _method;
        private readonly LazyValue<GenericInstanceMethodSignature> _signature;
        private string _fullName;

        public MethodSpecification(IMethodDefOrRef method, GenericInstanceMethodSignature signature)
            : base(null, new MetadataToken(MetadataTokenType.MethodSpec))
        {
            _method = new LazyValue<IMethodDefOrRef>(method);
            _signature = new LazyValue<GenericInstanceMethodSignature>(signature);
            CustomAttributes = new CustomAttributeCollection(this);
        }

        internal MethodSpecification(MetadataImage image, MetadataRow<uint, uint> row)
            : base(image, row.MetadataToken)
        {
            _method = new LazyValue<IMethodDefOrRef>(() =>
            {
                var encoder = image.Header.GetStream<TableStream>().GetIndexEncoder(CodedIndex.MethodDefOrRef);
                var methodToken = encoder.DecodeIndex(row.Column1);
                
                IMetadataMember member;
                return image.TryResolveMember(methodToken, out member) ? (IMethodDefOrRef) member : null;
            });

            _signature = new LazyValue<GenericInstanceMethodSignature>(() =>
                GenericInstanceMethodSignature.FromReader(image, image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column2)));
            
            CustomAttributes = new CustomAttributeCollection(this);
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
            get { return Method.Name; }
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
            get;
            private set;
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
