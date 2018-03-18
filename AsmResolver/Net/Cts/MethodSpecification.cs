
using System.Linq;
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
        private MetadataImage _image;

        public MethodSpecification(IMethodDefOrRef method, GenericInstanceMethodSignature signature)
            : base(new MetadataToken(MetadataTokenType.MethodSpec))
        {
            _method = new LazyValue<IMethodDefOrRef>(method);
            _signature = new LazyValue<GenericInstanceMethodSignature>(signature);
            CustomAttributes = new CustomAttributeCollection(this);
        }

        internal MethodSpecification(MetadataImage image, MetadataRow<uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
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

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _method.IsInitialized && _method.Value != null ? _method.Value.Image : _image; }
        }

        public IMethodDefOrRef Method
        {
            get { return _method.Value; }
            set
            {
                _method.Value = value;
                _fullName = null;
                _image = null;
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

                var methodSignature = (MethodSignature) Method.Signature;

                return _fullName = string.Format("{0} {1}::{2}<{3}>({4})",
                    methodSignature.ReturnType,
                    DeclaringType.FullName,
                    Name,
                    string.Join(", ", Signature.GenericArguments.Select(x => x.FullName)),
                    methodSignature.Parameters.Select(x => x.ParameterType).GetTypeArrayString());
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
