using System.Reflection;
using AsmResolver.DotNet.Blob;
using AsmResolver.DotNet.Collections;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single method in a type definition of a .NET module.
    /// </summary>
    public class MethodDefinition : IMetadataMember, IMemberDescriptor, IOwnedCollectionElement<TypeDefinition>
    {
        private readonly LazyVariable<string> _name;
        private readonly LazyVariable<TypeDefinition> _declaringType;
        private readonly LazyVariable<MethodSignature> _signature;

        /// <summary>
        /// Initializes a new method definition.
        /// </summary>
        /// <param name="token">The token of the method</param>
        protected MethodDefinition(MetadataToken token)
        {
            MetadataToken = token;
            _name  =new LazyVariable<string>(GetName);
            _declaringType = new LazyVariable<TypeDefinition>(GetDeclaringType);
            _signature = new LazyVariable<MethodSignature>(GetSignature);
        }

        /// <summary>
        /// Creates a new method definition.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="signature">The signature of the method</param>
        /// <remarks>
        /// For a valid .NET image, if <see cref="CallingConventionSignature.HasThis"/> of the signature referenced by
        /// <paramref name="signature"/> is set, the <see cref="MethodAttributes.Static"/> bit should be unset in
        /// <paramref name="attributes"/> and vice versa.
        /// </remarks>
        public MethodDefinition(string name, MethodAttributes attributes, MethodSignature signature)
            : this(new MetadataToken(TableIndex.Method, 0))
        {
            Name = name;
            Attributes = attributes;
            Signature = signature;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
            protected set;
        }

        /// <inheritdoc />
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <summary>
        /// Gets or sets the signature of the method This includes the return type, as well as the types of the
        /// parameters that this method defines.
        /// </summary>
        public MethodSignature Signature
        {
            get => _signature.Value;
            set => _signature.Value = value;
        }

        /// <inheritdoc />
        public string FullName
        {
            get
            {
                string parameterTypesString = string.Join(", ", Signature.ParameterTypes)
                                              + (Signature.IsSentinel ? ", ..." : string.Empty);
                return DeclaringType is null
                    ? $"{Signature.ReturnType} {Name}({parameterTypesString})"
                    : $"{Signature.ReturnType} {DeclaringType}::{Name}({parameterTypesString})";
            }
        }

        /// <summary>
        /// Gets or sets the attributes associated to the field.
        /// </summary>
        public MethodAttributes Attributes
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ModuleDefinition Module => DeclaringType.Module;

        /// <inheritdoc />
        public TypeDefinition DeclaringType
        {
            get => _declaringType.Value;
            set => _declaringType.Value = value;
        }

        ITypeDescriptor IMemberDescriptor.DeclaringType => DeclaringType;

        TypeDefinition IOwnedCollectionElement<TypeDefinition>.Owner
        {
            get => DeclaringType;
            set => DeclaringType = value;
        }

        /// <summary>
        /// Obtains the name of the method definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string GetName() => null;

        /// <summary>
        /// Obtains the declaring type of the method definition.
        /// </summary>
        /// <returns>The declaring type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DeclaringType"/> property.
        /// </remarks>
        protected virtual TypeDefinition GetDeclaringType() => null;
        
        /// <summary>
        /// Obtains the signature of the method definition.
        /// </summary>
        /// <returns>The signature.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Signature"/> property.
        /// </remarks>
        protected virtual MethodSignature GetSignature() => null;

        /// <inheritdoc />
        public override string ToString() => FullName;
    }
}