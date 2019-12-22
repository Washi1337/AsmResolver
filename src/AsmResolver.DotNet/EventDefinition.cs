using AsmResolver.DotNet.Collections;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single event in a type definition of a .NET module.
    /// </summary>
    public class EventDefinition : IMetadataMember, IMemberDescriptor, IOwnedCollectionElement<TypeDefinition>
    { 
        private readonly LazyVariable<string> _name;
        private readonly LazyVariable<TypeDefinition> _declaringType;
        private readonly LazyVariable<ITypeDefOrRef> _eventType;

        /// <summary>
        /// Initializes a new property definition.
        /// </summary>
        /// <param name="token">The token of the property.</param>
        protected EventDefinition(MetadataToken token)
        {
            MetadataToken = token;
            _name = new LazyVariable<string>(GetName);
            _eventType = new LazyVariable<ITypeDefOrRef>(GetEventType);
            _declaringType = new LazyVariable<TypeDefinition>(GetDeclaringType);
        }

        /// <summary>
        /// Creates a new property definition.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="eventType">The delegate type of the event.</param>
        public EventDefinition(string name, EventAttributes attributes, ITypeDefOrRef eventType)
            : this(new MetadataToken(TableIndex.Event,0))
        {
            Name = name;
            Attributes = attributes;
            EventType = eventType;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the attributes associated to the field.
        /// </summary>
        public EventAttributes Attributes
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <inheritdoc />
        public string FullName => FullNameGenerator.GetEventFullName(Name, DeclaringType, EventType);

        /// <summary>
        /// Gets or sets the delegate type of the event.
        /// </summary>
        public ITypeDefOrRef EventType
        {
            get => _eventType.Value;
            set => _eventType.Value = value;
        }

        /// <inheritdoc />
        public ModuleDefinition Module => DeclaringType?.Module;

        /// <summary>
        /// Gets the type that defines the property.
        /// </summary>
        public TypeDefinition DeclaringType
        {
            get => _declaringType.Value;
            private set => _declaringType.Value = value;
        }

        ITypeDescriptor IMemberDescriptor.DeclaringType => DeclaringType;

        TypeDefinition IOwnedCollectionElement<TypeDefinition>.Owner
        {
            get => DeclaringType;
            set => DeclaringType = value;
        }
        
        /// <summary>
        /// Obtains the name of the property definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string GetName() => null;
        
        /// <summary>
        /// Obtains the event type of the property definition.
        /// </summary>
        /// <returns>The event type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="EventType"/> property.
        /// </remarks>
        protected virtual ITypeDefOrRef GetEventType() => null;
        
        /// <summary>
        /// Obtains the declaring type of the property definition.
        /// </summary>
        /// <returns>The declaring type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DeclaringType"/> property.
        /// </remarks>
        protected virtual TypeDefinition GetDeclaringType() => null;

        /// <inheritdoc />
        public override string ToString() => FullName;
    }
}