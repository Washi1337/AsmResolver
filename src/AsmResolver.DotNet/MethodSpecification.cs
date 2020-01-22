using System.Collections.Generic;
using System.Threading;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a reference to a generic method that is instantiated with type arguments.
    /// </summary>
    public class MethodSpecification : IMethodDescriptor, IHasCustomAttribute
    {
        private readonly LazyVariable<IMethodDefOrRef> _method;
        private readonly LazyVariable<GenericInstanceMethodSignature> _signature;
        private IList<CustomAttribute> _customAttributes;

        /// <summary>
        /// Creates a new empty method specification.
        /// </summary>
        /// <param name="token">The token of the specification.</param>
        protected MethodSpecification(MetadataToken token)
        {
            MetadataToken = token;
            _method = new LazyVariable<IMethodDefOrRef>(GetMethod);
            _signature = new LazyVariable<GenericInstanceMethodSignature>(GetSignature);
        }

        /// <summary>
        /// Creates a new reference to a generic instantiation of a method.
        /// </summary>
        /// <param name="method">The method to instantiate.</param>
        /// <param name="signature">The instantiation signature.</param>
        public MethodSpecification(IMethodDefOrRef method, GenericInstanceMethodSignature signature)
            : this(new MetadataToken(TableIndex.MethodSpec,0))
        {
            Method = method;
            Signature = signature;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
            protected set;
        }
        
        /// <summary>
        /// Gets or sets the method that was instantiated.
        /// </summary>
        public IMethodDefOrRef Method
        {
            get => _method.Value;
            set => _method.Value = value;
        }

        MethodSignature IMethodDescriptor.Signature => Method.Signature;

        /// <summary>
        /// Gets or sets the generic instantiation of the method.
        /// </summary>
        public GenericInstanceMethodSignature Signature
        {
            get => _signature.Value;
            set => _signature.Value = value;
        }

        /// <inheritdoc />
        public string Name => Method.Name;

        /// <inheritdoc />
        public string FullName =>
            FullNameGenerator.GetMethodFullName(Name, DeclaringType, Method.Signature, Signature.TypeArguments);

        /// <inheritdoc />
        public ModuleDefinition Module => Method.Module;

        /// <summary>
        /// Gets the declaring type of the method.
        /// </summary>
        public ITypeDefOrRef DeclaringType => Method.DeclaringType;
        
        ITypeDescriptor IMemberDescriptor.DeclaringType => DeclaringType;

        /// <inheritdoc />
        public IList<CustomAttribute> CustomAttributes
        {
            get
            {
                if (_customAttributes is null)
                    Interlocked.CompareExchange(ref _customAttributes, GetCustomAttributes(), null);
                return _customAttributes;
            }
        }

        /// <inheritdoc />
        public MethodDefinition Resolve() => Method.Resolve();

        /// <summary>
        /// Obtains the instantiated method.
        /// </summary>
        /// <returns>The method.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Method"/> property.
        /// </remarks>
        protected virtual IMethodDefOrRef GetMethod() => null;

        /// <summary>
        /// Obtains the method instantiation signature.
        /// </summary>
        /// <returns>The signature.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Signature"/> property.
        /// </remarks>
        protected virtual GenericInstanceMethodSignature GetSignature() => null;

        /// <summary>
        /// Obtains the list of custom attributes assigned to the member.
        /// </summary>
        /// <returns>The attributes</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="CustomAttributes"/> property.
        /// </remarks>
        protected virtual IList<CustomAttribute> GetCustomAttributes() =>
            new OwnedCollection<IHasCustomAttribute, CustomAttribute>(this);

        /// <inheritdoc />
        public override string ToString() => FullName;
    }

}