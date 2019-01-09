
using System;
using System.Linq;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents extra metadata added to a method reference to indicate the type parameters for a generic
    /// method instance.
    /// </summary>
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

                return image.TryResolveMember(methodToken, out var member) ? (IMethodDefOrRef) member : null;
            });

            _signature = new LazyValue<GenericInstanceMethodSignature>(() =>
                GenericInstanceMethodSignature.FromReader(image, image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column2)));
            
            CustomAttributes = new CustomAttributeCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image => _method.IsInitialized && _method.Value != null 
            ? _method.Value.Image 
            : _image;

        /// <summary>
        /// Gets or sets the generic method to instantiate. 
        /// </summary>
        public IMethodDefOrRef Method
        {
            get => _method.Value;
            set
            {
                _method.Value = value;
                _fullName = null;
                _image = null;
            }
        }

        /// <summary>
        /// Gets or sets the signature associated to the specification, containing the generic type arguments.
        /// </summary>
        public GenericInstanceMethodSignature Signature
        {
            get => _signature.Value;
            set
            {
                _signature.Value = value;
                _fullName = null;
            }
        }

        CallingConventionSignature ICallableMemberReference.Signature => Method.Signature;

        /// <inheritdoc />
        public string Name => Method.Name;

        string IMemberReference.Name
        {
            get => Name;
            set => throw new NotSupportedException();
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public ITypeDefOrRef DeclaringType => Method.DeclaringType;

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
        }

        /// <summary>
        /// Resolves the method to its method definition.
        /// </summary>
        /// <returns>The resolved method.</returns>
        public MethodDefinition Resolve()
        {
            return (MethodDefinition) Method.Resolve();
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
