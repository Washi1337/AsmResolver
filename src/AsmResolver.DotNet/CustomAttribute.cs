using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Collections;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a custom attribute that is associated to a member in a .NET module. 
    /// </summary>
    public class CustomAttribute : IMetadataMember, IOwnedCollectionElement<IHasCustomAttribute>
    {
        private readonly LazyVariable<IHasCustomAttribute> _parent;
        private readonly LazyVariable<ICustomAttributeType> _constructor;
        private readonly LazyVariable<CustomAttributeSignature> _signature;

        /// <summary>
        /// Initializes an empty custom attribute.
        /// </summary>
        /// <param name="token">The token of the custom attribute.</param>
        protected CustomAttribute(MetadataToken token)
        {
            MetadataToken = token;
            _parent = new LazyVariable<IHasCustomAttribute>(GetParent);
            _constructor = new LazyVariable<ICustomAttributeType>(GetConstructor);
            _signature = new LazyVariable<CustomAttributeSignature>(GetSignature);
        }

        /// <summary>
        /// Creates a new custom attribute.
        /// </summary>
        /// <param name="constructor">The constructor of the attribute to call.</param>
        /// <param name="signature">The signature containing the arguments to the constructor.</param>
        public CustomAttribute(ICustomAttributeType constructor, CustomAttributeSignature signature)
            : this(new MetadataToken(TableIndex.CustomAttribute, 0))
        {
            Constructor = constructor;
            Signature = signature;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
        }

        /// <summary>
        /// Gets the member that this custom attribute is assigned to.
        /// </summary>
        public IHasCustomAttribute Parent
        {
            get => _parent.Value;
            private set => _parent.Value = value;
        }

        IHasCustomAttribute IOwnedCollectionElement<IHasCustomAttribute>.Owner
        {
            get => Parent;
            set => Parent = value;
        }

        /// <summary>
        /// Gets or sets the constructor that is invoked upon initializing the attribute.
        /// </summary>
        public ICustomAttributeType Constructor
        {
            get => _constructor.Value;
            set => _constructor.Value = value;
        }

        /// <summary>
        /// Gets or sets the signature containing the arguments passed onto the attribute's constructor.
        /// </summary>
        public CustomAttributeSignature Signature
        {
            get => _signature.Value;
            set => _signature.Value = value;
        }

        /// <summary>
        /// Obtains the parent member of the attribute.
        /// </summary>
        /// <returns>The member</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Parent"/> property.
        /// </remarks>
        protected virtual IHasCustomAttribute GetParent() => null;

        /// <summary>
        /// Obtains the constructor of the attribute.
        /// </summary>
        /// <returns>The constructor</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Constructor"/> property.
        /// </remarks>
        protected virtual ICustomAttributeType GetConstructor() => null;

        /// <summary>
        /// Obtains the signature of the attribute.
        /// </summary>
        /// <returns>The signature</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Signature"/> property.
        /// </remarks>
        protected virtual CustomAttributeSignature GetSignature() => null;

        /// <inheritdoc />
        public override string ToString()
        {
            return Constructor.FullName;
        }
    }
}